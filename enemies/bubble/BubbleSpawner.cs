using Godot;

public partial class BubbleSpawner : Node2D
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
  public int MaxSpawns = 10;

  // TODO: Wire up
  [Export]
  public float IdleSecondsAfterSpawn = 1.0f;

  [Export]
  public bool IsActive = true;

  private float _spawnTimer = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
    _spawnTimer = SecondsBetweenSpawn;
  }

  public override void _PhysicsProcess(double delta) {
    if (!this.IsActive) {
      return;
    }

    this._spawnTimer -= (float)delta;

    if (this._spawnTimer <= 0) {
      var bubble = this.BubbleScene.Instantiate() as BubbleAi;
      var offset = RandomUtil.RandomPointInCircle(RandomUtil.Rng.RandfRange(0f, SpawnRadius));
      bubble.GlobalPosition = this.GlobalPosition + offset;
      bubble.path = PatrolPath;
      GetParent().AddChild(bubble);
      _spawnTimer = SecondsBetweenSpawn;
    }
  }
}
