using Godot;
using System;

public partial class ChargeDisplay : Node2D
{

	//var bar_green = preload("res://assets/barHorizontal_green.png");

//
	// onready var movebar = $moveChargeBar;
	[Export] private TextureProgressBar moveChargeBar;
	public void updateCharge(float charge)
	{
		GD.Print("Updating charge: " + charge);
		moveChargeBar.Value = charge;
		GD.Print("Charge: " + moveChargeBar.Value);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
