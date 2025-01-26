using System.Linq;
using Godot;

public partial class Door : StaticBody2D {
  [Export] public Godot.Collections.Array<Node2D> triggers;

	public override void _Process(double delta)	{
    bool allDead = true;

    foreach (IKillable trigger in triggers.Cast<IKillable>()) {
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
