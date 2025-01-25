using Godot;
using System.Collections.Generic;

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
  [Export] private float ChargeKillThreshold = 10f;
  [Export] private float Friction;
  [Export] private Area2D point;

  private HashSet<BubbleAi> bubbles = new HashSet<BubbleAi>();

  public override void _Ready() {
    point.BodyEntered += OnPointEnter;
    point.BodyExited += OnPointExit;
  }

  public void OnPointEnter(Node other) {
    var bubble = other as BubbleAi;
    if (bubble == null) return;

    bubbles.Add(bubble);
  }

  public void OnPointExit(Node other) {
    var bubble = other as BubbleAi;
    if (bubble == null) return;

    bubbles.Remove(bubble);
  }

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
		if (charging) {
			charge += chargeSpeed;
			ChargeDisplay.updateCharge(charge);
		} else if (charge > 0) {
			Velocity = MoveSpeed(charge);
			charge -= Friction;
		}
		MoveAndSlide();

    foreach (var bubble in bubbles) {
      if (!IsInstanceValid(bubble)) {
          // cleanup now invalid bubbles (can happen if they suicide before you kill)
          bubbles.Remove(bubble);
      } else if (!charging && charge > ChargeKillThreshold) {
          // kill bubbles
          bubble.Pop();
          bubbles.Remove(bubble);
      }
    }
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
