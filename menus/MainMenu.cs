using Godot;

public partial class MainMenu : Control{
  [Export] private string gameScenePath;
  [Export] private Button startButton;
  [Export] private Button exitButton;

  public override void _Ready() {
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
