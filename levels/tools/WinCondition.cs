using System.Linq;
using Godot;

public partial class WinCondition : Node {
  [Export]
  public Godot.Collections.Array<Node2D> triggers;

  [Export]
  private string endScreenPath;

  public override void _Process(double delta) {
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
    Callable
    .From(() => {
      GetTree().ChangeSceneToFile(endScreenPath);
    })
    .CallDeferred();
  }
}
