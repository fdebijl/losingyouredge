using System.Linq;
using Godot;

public partial class Door : StaticBody2D {
  [Export]
  public Godot.Collections.Array<Node2D> triggers;

  public override void _PhysicsProcess(double delta) {
    bool allDead = true;

    foreach (
        IKillable trigger in triggers.Where(n => n != null && IsInstanceValid(n)).Cast<IKillable>()
    ) {
      var node = trigger as Node;
      if (node == null || !IsInstanceValid(node)) {
        continue;
      }

      if (!trigger.IsDead()) {
        allDead = false;
        break;
      }
    }

    if (allDead) {
      this.DoOnAllDead();
    }
  }

  private void DoOnAllDead() {
    QueueFree();
  }
}
