using Godot;

public partial class MainMenu : Control{
  [Export] private string gameScenePath;
  [Export] private Button startButton;
  [Export] private Button exitButton;
  [Export] private AudioStream bgmStart;

  public override void _Ready() {
    AudioManager.PlaySFX(bgmStart, 1, false, GlobalPosition);
    startButton.ButtonDown += OnStartButtonPressed;
    exitButton.ButtonDown += OnExitButtonPressed;
  }

  private void OnStartButtonPressed() {
    GetTree().ChangeSceneToFile(gameScenePath);
  }

  private void OnExitButtonPressed() {
    GetTree().Quit();
  }
}
