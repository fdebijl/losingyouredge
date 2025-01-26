using System;
using Godot;

public partial class LoseScreen : Control {
  [Export]
  private string gameScenePath;

  [Export]
  private Button restartButton;

  [Export]
  private Button exitButton;

  public override void _Ready() {
    restartButton.ButtonDown += OnRestartButtonPressed;
    exitButton.ButtonDown += OnExitButtonPressed;
    restartButton.GrabFocus();
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

    public override void _Input(InputEvent @event) {
    if (@event is InputEventJoypadButton joypadEvent) {
      if (joypadEvent.ButtonIndex == JoyButton.A && joypadEvent.Pressed) {
        OnRestartButtonPressed();
      }
    }

    if (@event.IsActionPressed("ui_accept")) {
      OnRestartButtonPressed();
    } else if (@event.IsActionPressed("ui_cancel")) {
      OnExitButtonPressed();
    }
  }
}
