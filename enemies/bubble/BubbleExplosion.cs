using Godot;
using System;

public partial class BubbleExplosion : Node
{
	[Export]
	private PackedScene explosion;

	[Export]
	private float despawnTimer = 1f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var obj = explosion.Instantiate();
		AddChild(obj);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		despawnTimer -= (float) delta;

		if (despawnTimer < 0) {
			GetParent().RemoveChild(this);
		}
	}
}
