using System.Linq;
using Godot;

public partial class WinCondition : Node {
  [Export]
  public Godot.Collections.Array<Node2D> triggers;

  [Export]
  private string endScreenPath;

  public override void _PhysicsProcess(double delta) {
    bool allDead = true;

    foreach (
        IKillable trigger in triggers.Where(n => n != null && IsInstanceValid(n)).Cast<IKillable>()
    ) {
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
    Timer timer = new Timer();
    AddChild(timer);
    timer.WaitTime = 3;
    timer.OneShot = true;
    timer.Connect("timeout", Callable.From(TransitionToWinScreen));
    timer.Start();
  }

  private void TransitionToWinScreen() {
    Callable
    .From(() => {
      GetTree().ChangeSceneToFile(endScreenPath);
    })
    .CallDeferred();
  }
}
