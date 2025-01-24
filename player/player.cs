using Godot;

public partial class player : CharacterBody2D
{
	private int _speed = 300;

	public void GetInput()
	{
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Velocity = inputDir * _speed;
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		MoveAndCollide(Velocity * (float)delta);
	}
}
