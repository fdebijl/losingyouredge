using Godot;

public partial class MainMenu : Control {
  [Export] private string gameScenePath;
  [Export] private Button startButton;
  [Export] private Button exitButton;
  [Export] private AudioStream bgmStart;

  public override void _Ready() {
    AudioManager.PlaySFX(bgmStart, 1, false, GlobalPosition);
    startButton.ButtonDown += OnStartButtonPressed;
    exitButton.ButtonDown += OnExitButtonPressed;
    startButton.GrabFocus();
  }

  private void OnStartButtonPressed() {
    GetTree().ChangeSceneToFile(gameScenePath);
  }

  private void OnExitButtonPressed() {
    GetTree().Quit();
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventJoypadButton joypadEvent) {
      if (joypadEvent.ButtonIndex == JoyButton.A && joypadEvent.Pressed) {
        OnStartButtonPressed();
      }
    }

    if (@event.IsActionPressed("ui_accept")) {
      OnStartButtonPressed();
    } else if (@event.IsActionPressed("ui_cancel")) {
      OnExitButtonPressed();
    }
  }
}
