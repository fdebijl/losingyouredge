using Godot;

public partial class player : CharacterBody2D
{
	private float charge = 0;
	private bool charging = false;
	private bool moving = false;
	private Vector2 MousePosition;
	private Vector2 chargeDirection;
  private int health = 100;

	[Export] private int _speed = 300;
	[Export] private ChargeDisplay ChargeDisplay;
  [Export] private float ChargeScale;
  [Export] private float chargeSpeed;

  public void Damage(int damage)
  {
    health -= damage;
    if (health <= 0)
    {
      Death();
    }
  }

  public void Death()
  {
   //Play death animation
   //Disable player input
   //Play death sound
  }

	public void GetInput()
	{
		MousePosition = GetViewport().GetMousePosition();
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();

		if (charging) {
			charge += chargeSpeed;
			ChargeDisplay.updateCharge(charge);
		} else if (charge > 0) {
			Velocity = MoveSpeed(charge);
			charge -= 1;
		}
		MoveAndCollide(Velocity * (float)delta);
	}

	private Vector2 MoveSpeed(float charge)
	{
		float chargeScale = charge * ChargeScale;
		return chargeScale  * (Position - chargeDirection).Normalized();
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
