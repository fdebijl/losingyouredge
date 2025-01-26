using System;
using Godot;

public partial class WinScreen : Control {
  [Export]
  private string gameScenePath;

  [Export]
  private Button restartButton;

  [Export]
  private Button exitButton;

  public override void _Ready() {
    restartButton.ButtonDown += OnRestartButtonPressed;
    exitButton.ButtonDown += OnExitButtonPressed;
  }

  private void OnRestartButtonPressed() {
    Callable
    .From(() => {
      GetTree().ChangeSceneToFile(gameScenePath);
    })
    .CallDeferred();
  }

  private void OnExitButtonPressed() {
    GetTree().Quit();
  }
}
