using Godot;
using System;

public partial class EndScreen : Control
{
  [Export] private string gameScenePath;
  [Export] private Button restartButton;
  [Export] private Button exitButton;
  [Export] private string winText = "You Win!";
  [Export] private string loseText = "You Lose!";
  [Export] private Label label;

  public override void _Ready() {

    restartButton.ButtonDown += OnRestartButtonPressed;
    exitButton.ButtonDown += OnExitButtonPressed;
  }

  public void SetEndText(bool win) {
    var text = win ? winText : loseText;
    label.Text = text;
  }

  private void OnRestartButtonPressed() {
    GetTree().ChangeSceneToFile(gameScenePath);
  }

  private void OnExitButtonPressed() {
    GetTree().Quit();
  }
}
