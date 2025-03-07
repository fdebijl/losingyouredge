using Godot;

public partial class BubbleSpawner : StaticBody2D, IKillable {
  // How many seconds before spawning to show the ready sprite
  const float READY_SPRITE_TIME = 2f;
  // How many seconds before and after spawning to show the spawning sprite
  const float SPAWN_SPRITE_TIME = 0.5f;

  const int IDLE_FRAME = 0;
  const int READY_FRAME = 1;
  const int SPAWNING_FRAME = 2;
  const int DESTROYED_FRAME = 3;

  [Export] public PackedScene BubbleScene;
  [Export] public Line2D PatrolPath;
  [Export] public AudioStream killSFX;
  [Export] public AudioStream hitSFX;
  [Export] public AnimatedSprite2D Sprite;
	[Export] public AnimationPlayer playerAnimation;

  [Export] public bool BossMode = false;
  [Export] public int BossHealth = 100;

  [ExportGroup("Spawner Settings")]
  [Export(PropertyHint.Range, "0,100,0.1")]  public float SecondsBetweenSpawn = 1.0f;
  [Export] public float SpawnDelay = 0.0f;
  [Export] public float SpawnRadius = 5.0f;
  [Export] public int MaxNumberOfAliveBubbles = 10;
  [Export] public int MaxNumberOfTotalSpawns = 30;
  [Export] public bool IsActive = true;
  [Export] public Node2D Parent = null;

  [ExportGroup("Bubble Overrides")]
  [Export] public float BaseSpeed = 80f;
  [Export] public float ChaseSpeedMultiplier = 1.2f;
  [Export] public float MaxOffset = 180.0f;
  [Export] public float ExplosionTimer = 1f;
  [Export(PropertyHint.Range, "0,100,1")] public float kepsylon = 5.0f;
  [Export(PropertyHint.Range, "0,10000,1")] public float seekKepsylon = 200.0f;
  [Export(PropertyHint.Range, "0,100,1")] public float explodeKepsylon = 20.0f;


  private float _spawnTimer = 0f;
  private int _spawnedCounter = 0;
  private bool _isDead = false;

	public override void _Ready() {
    if (BossMode) {
      this.Sprite.Animation = "Boss";
    } else {
      this.Sprite.Animation = "Default";
    }

    _spawnTimer = SecondsBetweenSpawn;
    this.Sprite.Frame = IDLE_FRAME;
    playerAnimation.Play("shake");
  }

  public bool IsDead() {
    return this._isDead;
  }

  public void TryKill() {
    // belongs to to Boss -- have to kill that
    if (BossMode && BossHealth > 0) return;

    this.IsActive = false;
    this._isDead = true;
    if (BossMode) {
      this.Sprite.Animation = "death";
    }
    playerAnimation.Stop();
    AudioManager.PlaySFX(killSFX, 1f, false, GlobalPosition);
    this.Sprite.Frame = DESTROYED_FRAME;
  }

  public void Hit(int damage) {
    if (BossMode) {
      BossHealth -= damage;
    }

    AudioManager.PlaySFX(hitSFX, 1f, false, GlobalPosition);
    TryKill();
  }

  public override void _PhysicsProcess(double delta) {
    if (!this.IsActive) {
      return;
    }

    if (this.SpawnDelay > 0) {
      this.SpawnDelay -= (float)delta;
      return;
    }

    this._spawnTimer -= (float)delta;

    if (this.CanSpawn() && this._spawnedCounter > 0 && (this._spawnTimer < SPAWN_SPRITE_TIME || this._spawnTimer >= SecondsBetweenSpawn - SPAWN_SPRITE_TIME)) {
      this.Sprite.Frame = SPAWNING_FRAME;
    } else if (this.CanSpawn() && this._spawnTimer < READY_SPRITE_TIME) {
      this.Sprite.Frame = READY_FRAME;
      playerAnimation.Play("rotate");
    } else {
      this.Sprite.Frame = IDLE_FRAME;
      if (!BossMode) {
        playerAnimation.Play("shake");
      }
    }

    if (this._spawnTimer <= 0 && this.CanSpawn()) {
      this._spawnedCounter++;
      this.SpawnBubble();
    }
  }

  private void SpawnBubble() {
    var bubble = this.BubbleScene.Instantiate() as BubbleAi;
    var offset = RandomUtil.RandomPointInCircle(RandomUtil.Rng.RandfRange(0f, SpawnRadius));

    bubble.path = PatrolPath;
    bubble.BaseSpeed = this.BaseSpeed;
    bubble.ChaseSpeedMultiplier = this.ChaseSpeedMultiplier;
    bubble.maxOffset = this.MaxOffset;
    bubble.explosionTimer = this.ExplosionTimer;
    bubble.kepsylon = this.kepsylon;
    bubble.seekKepsylon = this.seekKepsylon;
    bubble.explodeKepsylon = this.explodeKepsylon;
    bubble.AddToGroup($"Bubble-{Name}");

    bubble.Spawner = this;
    var parent = Parent == null ? GetParent() : Parent.GetParent();
    parent.AddChild(bubble);
    bubble.GlobalPosition = this.GlobalPosition + offset;
    this._spawnTimer = this.SecondsBetweenSpawn;
  }

  private bool CanSpawn() {
    var bubbleCount =  GetTree().GetNodesInGroup($"Bubble-{Name}").Count;
    return bubbleCount < this.MaxNumberOfAliveBubbles && this._spawnedCounter < this.MaxNumberOfTotalSpawns;
  }
}
