using Godot;

public partial class BubbleSpawner : Node2D
{
  [Export]
  public PackedScene BubbleScene;

  [ExportGroup("Spawner Settings")]
  [Export(PropertyHint.Range, "0,100,1")]
  public float SpawnRate = 1.0f;

  [Export]
  public float SpawnRadius = 5.0f;

  [Export]
  public Line2D path;

  // TODO: Wire up
  [Export]
  public float IdleAfterSpawn = 1.0f;

  // TODO: Wire up
  [Export]
  public bool IsActive = true;

  private float _spawnTimer = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
    _spawnTimer = SpawnRate;
  }

  public override void _PhysicsProcess(double delta) {
    if (!this.IsActive) {
      return;
    }

    this._spawnTimer -= (float)delta;

    if (this._spawnTimer <= 0) {
      var bubble = this.BubbleScene.Instantiate() as BubbleAi;
      var offset = RandomUtil.RandomPointInCircle(SpawnRadius);
      bubble.GlobalPosition = this.GlobalPosition + offset;
      bubble.path = path;
      GetParent().AddChild(bubble);
      _spawnTimer = SpawnRate;
    }
  }
}
