using Godot;

public partial class player : CharacterBody2D
{
	private int _speed = 300;
	private Vector2 MousePosition;
	private float charge = 0;
	private bool charging = false;
	private Vector2 chargeDirection;

	[Export] private ChargeDisplay ChargeDisplay;
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
			ChargeDisplay.updateCharge(charge);
		} else if (charge > 0) {
			Velocity = MoveSpeed(charge);
			charge -= 1;
		}
		MoveAndCollide(Velocity * (float)delta);
	}

	private Vector2 MoveSpeed(float charge)
	{
		float chargeScale = (float)(charge * 0.01);
		return chargeScale  * (Position - chargeDirection);
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
				ChargeDisplay.updateCharge(0);
				charging = false;
			}
		}
	}
}
