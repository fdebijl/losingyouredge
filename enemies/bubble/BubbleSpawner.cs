using Godot;

public partial class BubbleSpawner : StaticBody2D, IKillable
{
  // How many seconds before spawning to show the ready sprite
  const float READY_SPRITE_TIME = 2f;
  // How many seconds before and after spawning to show the spawning sprite
  const float SPAWN_SPRITE_TIME = 0.5f;

  const int IDLE_FRAME = 0;
  const int READY_FRAME = 1;
  const int SPAWNING_FRAME = 2;

  [Export]
  public PackedScene BubbleScene;

  [Export]
  public Line2D PatrolPath;

  [Export]
  public AudioStream killSFX;

  [Export]
  public AnimatedSprite2D Sprite;

  [ExportGroup("Spawner Settings")]
  [Export(PropertyHint.Range, "0,100,0.1")]
  public float SecondsBetweenSpawn = 1.0f;

  [Export]
  public float SpawnRadius = 5.0f;

  [Export]
  public int MaxNumberOfAliveBubbles = 10;

  [Export]
  public int MaxNumberOfTotalSpawns = 30;

  [Export]
  public bool IsActive = true;

  private float _spawnTimer = 0f;

  private int _spawnedCounter = 0;

	public override void _Ready() {
    _spawnTimer = SecondsBetweenSpawn;
    this.Sprite.Frame = IDLE_FRAME;
  }

  public void Kill() {
    this.IsActive = false;
    AudioManager.PlaySFX(killSFX);
    GetParent().RemoveChild(this);
  }

  public override void _PhysicsProcess(double delta) {
    if (!this.IsActive) {
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
    bubble.AddToGroup($"Bubble-{Name}");
    GetParent().AddChild(bubble);
    this._spawnTimer = this.SecondsBetweenSpawn;
  }

  private bool CanSpawn() {
    var bubbleCount =  GetTree().GetNodesInGroup($"Bubble-{Name}").Count;
    return bubbleCount < this.MaxNumberOfAliveBubbles && this._spawnedCounter < this.MaxNumberOfTotalSpawns;
  }
}
