using Godot;

public partial class SpawnerActivatorTrigger : Area2D {
  [Export] public Godot.Collections.Array<BubbleSpawner> spawners;

  public void On_SpawnerActivatorTrigger_body_entered(Node body) {
    GD.Print("SpawnerActivatorTrigger body entered");
    if (body is Player) {
      GD.Print("SpawnerActivatorTrigger body is player");
      foreach (BubbleSpawner spawner in spawners) {
        spawner.IsActive = true;
      }
    }
  }
}
