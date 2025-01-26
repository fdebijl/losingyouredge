using Godot;

public partial class SpawnerActivatorTrigger : Area2D {
  [Export] public Godot.Collections.Array<BubbleSpawner> spawners;
  [Export] public bool TriggerEveryTIme = false;

  private bool _hasTriggered = false;

  public void On_SpawnerActivatorTrigger_body_entered(Node body) {
    if (body is Player && (TriggerEveryTIme || !_hasTriggered)) {
      _hasTriggered = true;

      foreach (BubbleSpawner spawner in spawners) {
        spawner.IsActive = true;
      }
    }
  }
}
