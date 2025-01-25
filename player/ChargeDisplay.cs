using Godot;

public partial class ChargeDisplay : Node2D
{
  private Texture2D barYellow;
  private Texture2D barRed;

  public override void _Ready()
  {
    barYellow = (Texture2D)GD.Load("res://player/assets/barHorizontal_yellow.png");
    barRed = (Texture2D)GD.Load("res://player/assets/barHorizontal_red.png");
  }
	[Export] private TextureProgressBar moveChargeBar;
	public void updateCharge(float charge)
	{
    if (charge >= moveChargeBar.MaxValue * 0.8)
    {
      moveChargeBar.TextureProgress = barRed;
    }
    else if (charge > moveChargeBar.MaxValue * 0.45)
    {
      moveChargeBar.TextureProgress = barYellow;
    }
		moveChargeBar.Value = charge;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
