using Godot;

public partial class player : CharacterBody2D
{
	private int _speed = 300;
	private Vector2 MousePosition;
	private int charge = 0;
	private bool charging = false;
	private Vector2 chargeDirection;

	public void GetInput()
	{
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Velocity = inputDir * _speed;
		
		MousePosition = GetViewport().GetMousePosition();
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		
		if (charging) {
			charge += 1;
		} else if (charge > 0) {
			Velocity = charge * (Position - chargeDirection);
			charge -= 1;
		}
		MoveAndCollide(Velocity * (float)delta);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton){
			if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
			{
				charging = true;
			}
			else if (eventMouseButton.ButtonIndex == MouseButton.Left && !eventMouseButton.Pressed)
			{
				chargeDirection = eventMouseButton.Position;
				charging = false;
			}
		}
	}
}
