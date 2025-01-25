using Godot;
using System.Collections.Generic;

public partial class player : CharacterBody2D
{
	private float charge = 0;
	private bool charging = false;
	private bool allowCharge = true;
	private bool chargingAnnimation = false;
	private Vector2 MousePosition;
	private Vector2 chargeDirection;
  private Vector2 currentMovement;
  private int health = 100;

	[Export] private int _speed = 300;
	[Export] private ChargeDisplay ChargeDisplay;
  [Export] private float ChargeScale;
  [Export] private float chargeSpeed;
  [Export] private float ChargeKillThreshold = 10f;
  [Export] private float Friction;
  [Export] private Area2D point;

  private HashSet<IKillable> enemies = new HashSet<IKillable>();

  public override void _Ready() {
    point.BodyEntered += OnPointEnter;
    point.BodyExited += OnPointExit;
  }

  public void OnPointEnter(Node other) {
    var enemy = other as IKillable;
    if (enemy == null) return;

    enemies.Add(enemy);
  }

  public void OnPointExit(Node other) {
    var enemy = other as IKillable;
    if (enemy == null) return;

    enemies.Remove(enemy);
  }

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
		if (allowCharge && charging) {
			charge += chargeSpeed;
			ChargeDisplay.updateCharge(charge);
		} else if (charge > 0) {
			charge -= Friction;
			Velocity = currentMovement + MoveSpeed(charge);
      if (charge < 20) {
        allowCharge = true;
      }
		}
		MoveAndSlide();

    foreach (var enemy in enemies) {
      var node = enemy as Node;
      if (node != null && !IsInstanceValid(node)) {
          // cleanup now invalid enemies (can happen if they suicide before you kill)
          enemies.Remove(enemy);
      } else if (!charging && charge > ChargeKillThreshold) {
          // kill enemies
          enemy.Kill();
          enemies.Remove(enemy);
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
		 JumpAnimation(true);
    }
    else if (Input.IsActionJustReleased("chargeControllerButton"))
    {
		  JumpAnimation(false);
      chargeDirection.X = Input.GetAxis("ui_left", "ui_right");
      chargeDirection.Y = Input.GetAxis("ui_up", "ui_down");

      ChargeDisplay.updateCharge(0);
      charging = false;
      allowCharge = false;
    }
    else if (Input.IsActionJustReleased("chargeMouseButton"))
    {
		  JumpAnimation(false);
      chargeDirection =  (Position - MousePosition).Normalized();
      ChargeDisplay.updateCharge(0);
      charging = false;
      allowCharge = false;
    }
  }

	private void JumpAnimation(bool on)
	{
		if (on && chargingAnnimation == false)
		{
			playerBodyAnimation.Play("jump");
			playerFaceAnimation.Play("jump");
			chargingAnnimation = true;
		}
		else if (!on && chargingAnnimation == true)
		{
			playerAnimation.Stop();
			playerBodyAnimation.Stop();
			playerFaceAnimation.Stop();
			chargingAnnimation = false;
		}

		if (charge > 80 && chargingAnnimation == true && on)
		{
			playerAnimation.Play("jump");
		}
	}
}
