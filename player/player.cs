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
  [Export] private float Friction;
	[Export] private AnimationPlayer playerAnimation;
	[Export] private AnimatedSprite2D playerBodyAnimation;
	[Export] private AnimatedSprite2D playerFaceAnimation;
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
    GD.Print(charge);
		if (charging) {
			charge += chargeSpeed;
			ChargeDisplay.updateCharge(charge);
		} else if (charge > 0) {
			Velocity = MoveSpeed(charge);
			charge -= Friction;
		}
		MoveAndCollide(Velocity * (float)delta);
	}

	private Vector2 MoveSpeed(float charge)
	{
		float chargeScale = charge * ChargeScale;
		return chargeScale  * chargeDirection;
	}

  public override void _Process(double delta)
  {
    GetInput();
    if (Input.IsActionPressed("chargeControllerButton") || Input.IsActionPressed("chargeMouseButton"))
    {
     charging = true;
    }
    else if (Input.IsActionJustReleased("chargeControllerButton"))
    {
      chargeDirection.X = Input.GetAxis("ui_left", "ui_right");
      chargeDirection.Y = Input.GetAxis("ui_up", "ui_down");

      ChargeDisplay.updateCharge(0);
      charging = false;
    }
    else if (Input.IsActionJustReleased("chargeMouseButton"))
    {
      chargeDirection =  (Position - MousePosition).Normalized();
      ChargeDisplay.updateCharge(0);
      charging = false;
    }
  }
}
