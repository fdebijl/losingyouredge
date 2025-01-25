using Godot;

public partial class BubbleSpawner : StaticBody2D, IKillable
{
  [Export]
  public PackedScene BubbleScene;

  [Export]
  public Line2D PatrolPath;

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
  public AudioStream killSFX;

  // TODO: Wire up if needed
  // [Export]
  // public float IdleSecondsAfterSpawn = 1.0f;

  [Export]
  public bool IsActive = true;

  private float _spawnTimer = 0f;

  private int _spawnedCounter = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
    _spawnTimer = SecondsBetweenSpawn;
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

    var bubbleCount = GetTree().GetNodesInGroup($"Bubble-{Name}").Count;

    if (this._spawnTimer <= 0 && bubbleCount < this.MaxNumberOfAliveBubbles && this._spawnedCounter < this.MaxNumberOfTotalSpawns) {
      this._spawnedCounter++;
      var bubble = this.BubbleScene.Instantiate() as BubbleAi;
      var offset = RandomUtil.RandomPointInCircle(RandomUtil.Rng.RandfRange(0f, SpawnRadius));
      bubble.GlobalPosition = this.GlobalPosition + offset;
      bubble.path = PatrolPath;
      bubble.AddToGroup($"Bubble-{Name}");
      GetParent().AddChild(bubble);
      _spawnTimer = SecondsBetweenSpawn;
    }
  }
}
