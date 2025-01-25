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
  [Export] public AnimatedSprite2D Sprite;
  [Export] public bool BossMode = false;

  [ExportGroup("Spawner Settings")]
  [Export(PropertyHint.Range, "0,100,0.1")]  public float SecondsBetweenSpawn = 1.0f;
  [Export] public float SpawnDelay = 0.0f;
  [Export] public float SpawnRadius = 5.0f;
  [Export] public int MaxNumberOfAliveBubbles = 10;
  [Export] public int MaxNumberOfTotalSpawns = 30;
  [Export] public bool IsActive = true;

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
    }

    _spawnTimer = SecondsBetweenSpawn;
    this.Sprite.Frame = IDLE_FRAME;
  }

  public bool IsDead() {
    return this._isDead;
  }

  public void Kill() {
    this.IsActive = false;
    this._isDead = true;
    AudioManager.PlaySFX(killSFX);
    this.Sprite.Frame = DESTROYED_FRAME;
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
    } else {
      this.Sprite.Frame = IDLE_FRAME;
    }

    if (this._spawnTimer <= 0 && this.CanSpawn()) {
      this._spawnedCounter++;
      this.SpawnBubble();
    }
  }

  private void SpawnBubble() {
    var bubble = this.BubbleScene.Instantiate() as BubbleAi;
    var offset = RandomUtil.RandomPointInCircle(RandomUtil.Rng.RandfRange(0f, SpawnRadius));

    bubble.GlobalPosition = this.GlobalPosition + offset;
    bubble.path = PatrolPath;
    bubble.BaseSpeed = this.BaseSpeed;
    bubble.ChaseSpeedMultiplier = this.ChaseSpeedMultiplier;
    bubble.maxOffset = this.MaxOffset;
    bubble.explosionTimer = this.ExplosionTimer;
    bubble.kepsylon = this.kepsylon;
    bubble.seekKepsylon = this.seekKepsylon;
    bubble.explodeKepsylon = this.explodeKepsylon;
    bubble.AddToGroup($"Bubble-{Name}");

    GetParent().AddChild(bubble);
    this._spawnTimer = this.SecondsBetweenSpawn;
  }

  private bool CanSpawn() {
    var bubbleCount =  GetTree().GetNodesInGroup($"Bubble-{Name}").Count;
    return bubbleCount < this.MaxNumberOfAliveBubbles && this._spawnedCounter < this.MaxNumberOfTotalSpawns;
  }
}
