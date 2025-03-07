using System;
using System.Collections.Generic;
using Godot;

public partial class Player : CharacterBody2D {
  private bool useMouse;
  private float charge = 0;
  private bool charging = false;
  private bool allowCharge = true;
  private float chargeCooldownTimer = 0.0f;
  private bool chargingAnnimation = false;
  private bool moving = false;
  private Vector2 MouseDirection;
  private Vector2 chargeDirection;
  private Vector2 playerRotation;
  private int maxHealth = 100;
  private int health = 100;
  private float randomTimer = 0;
  private Random random = new Random();

  [Export]
  private float chargeCooldown = 3f;

  [Export]
  private int _speed = 300;

  [Export]
  private ChargeDisplay ChargeDisplay;

  [Export]
  private float ChargeScale;

  [Export]
  private float chargeSpeed;

  [Export]
  private float ChargeKillThreshold = 10f;

  [Export]
  private float Friction;

  [Export]
  private Area2D point;

  [Export]
  private bool isInteractive = true;

  [Export]
  private bool winningMode = false;

  [Export]
  private int damage = 10;

  [Export]
  private AudioListener2D playerAudioListener;

  [Export]
  public AudioStream chargeSFX;

  [Export]
  public AudioStream jumpSFX;

  [Export]
  public AudioStream wallHitSFX;

  [Export]
  private string endScreenPath;
  private ShaderMaterial playerShader;

  private HashSet<IKillable> enemies = new HashSet<IKillable>();

  public override void _Ready() {
    point.BodyEntered += OnPointEnter;
    point.BodyExited += OnPointExit;
    playerShader = playerBodyAnimation.Material as ShaderMaterial;

    if (winningMode) {
      WinningAnimation();
    }
  }

  public void OnPointEnter(Node other) {
    var enemy = other as IKillable;
    if (enemy == null)
      return;

    enemies.Add(enemy);
  }

  public void OnPointExit(Node other) {
    var enemy = other as IKillable;
    if (enemy == null)
      return;

    enemies.Remove(enemy);
  }

  [Export]
  private AnimationPlayer playerAnimation;

  [Export]
  private AnimatedSprite2D playerBodyAnimation;

  [Export]
  private AnimatedSprite2D playerFaceAnimation;

  [Export]
  private AnimatedSprite2D playerDamageAnimation;
  [Export]
  private AnimatedSprite2D playerDeathAnimation;

  public void Heal(int value) {
    health += value;
    healthAnimation();
  }

  public void Damage(int damage) {
    health -= damage;
    DamageAnimation();

    healthAnimation();
    if (health <= 0) {
      Death();
    }
  }

  private void WinningAnimation() {
    playerAnimation.Play("winning");
    playerBodyAnimation.Play("winning");
    playerFaceAnimation.Play("winning");
  }

  private void healthAnimation() {
    switch (health) {
      case 100:
        playerDamageAnimation.Visible = false;
        break;
      case 80:
        playerDamageAnimation.Visible = true;
        playerDamageAnimation.Frame = 0;
        break;
      case 60:
        playerDamageAnimation.Visible = true;
        playerDamageAnimation.Frame = 1;
        break;
      case 40:
        playerDamageAnimation.Visible = true;
        playerDamageAnimation.Frame = 2;
        break;
      case 20:
        playerDamageAnimation.Visible = true;
        playerDamageAnimation.Frame = 4;
        break;
      default:
        break;
    }
  }

  public void Death() {
    //Play death animation
    playerDeathAnimation.Visible = true;
    playerDeathAnimation.Play("death");
    //Disable player input
    //Play death sound
    //get the end screen scene

    Timer timer = new Timer();
    AddChild(timer);
    timer.WaitTime = 2;
    timer.OneShot = true;
    timer.Connect("timeout", Callable.From(TransitionToDeathScreen));
    timer.Start();
  }

  private void TransitionToDeathScreen() {
    Callable
    .From(() => {
      GetTree().ChangeSceneToFile(endScreenPath);
    })
    .CallDeferred();
  }

  public override void _Input(InputEvent @event) {
    UseMouseCheck(@event);
  }

  private void UseMouseCheck(InputEvent @event) {
    if (@event is InputEventMouseMotion mouseEvent) {
      useMouse = true;
    } else if (@event is InputEventJoypadMotion joypadEvent) {
      useMouse = false;
    }
  }

  public void GetInput() {
    MouseDirection = GlobalPosition - GetGlobalMousePosition();
  }

  public override void _PhysicsProcess(double delta) {
    CooldownTimerTick(delta);

    if (allowCharge && chargeCooldownTimer <= 0 && charging) {
      if (charge < 100) {
        charge += chargeSpeed;
      }

      ChargeDisplay.updateCharge(charge);
    } else if (charge > 0) {
      charge -= Friction;
      Velocity = MoveSpeed(charge);
    } else if (charge == 0) {
      moving = false;
    }

    // if (charge < 20) {
    //   allowCharge = true;
    // }
    if (allowCharge == false) {
      playerBodyAnimation.Play("fly");
      playerFaceAnimation.Play("fly");
      if (charge < 20) {
        allowCharge = true;
        playerBodyAnimation.Play("idle");
        playerFaceAnimation.Play("idle");
      }
    }

    MoveAndSlide();
    DetectCollisions();
    UpdateShader();

    foreach (var enemy in enemies) {
      var node = enemy as Node;
      if (node != null && !IsInstanceValid(node)) {
        // cleanup now invalid enemies (can happen if they suicide before you kill)
        enemies.Remove(enemy);
      } else if (!charging && charge > ChargeKillThreshold) {
        // kill enemies
        enemy.Hit(damage);
        enemies.Remove(enemy);
      }
    }
  }

  private void DetectCollisions() {
    if (!moving) {
      return;
    }
    for (int i = 0; i < GetSlideCollisionCount(); i++) {
      var collisionLayer = -1;
      var collision = GetSlideCollision(i);

      // basic static body
      var collider = collision.GetCollider() as CollisionObject2D;
      if (collider != null) {
        collisionLayer = (int)collider.CollisionLayer;
      } else {
        var tileMapLayer = collision.GetCollider() as TileMapLayer;
        if (tileMapLayer == null)
          return;

        var tileSet = tileMapLayer.TileSet;
        collisionLayer = (int)tileSet.GetPhysicsLayerCollisionLayer(0);
        /* coTileMapLayer tileMapLayer.CollisionLayer; */
      }

      if (collisionLayer == -1)
        return;

      if (collisionLayer == 8) {
        AudioManager.PlaySFX(wallHitSFX, 1, false, GlobalPosition);
        BentAnimation();
        Velocity = Vector2.Zero;
        charge = 0;
        allowCharge = true;
        moving = false;
      }
    }
  }

  private void SetMoveCooldown() {
    chargeCooldownTimer = chargeCooldown;
  }

  private void CooldownTimerTick(double delta) {
    if (chargeCooldownTimer > 0) {
      chargeCooldownTimer -= (float)delta;
    }
  }

  private Vector2 MoveSpeed(float charge) {
    float chargeScale = charge * ChargeScale;
    return chargeScale * chargeDirection;
  }

  public override void _Process(double delta) {
    if (!isInteractive) {
      return;
    }

    GetInput();

    if (allowCharge) {
      if (useMouse) {
        Rotation = Mathf.Atan2(MouseDirection.Y, MouseDirection.X) + Mathf.Pi / 2;
      } else {
        playerRotation.X = Input.GetJoyAxis(0, JoyAxis.LeftX);
        playerRotation.Y = Input.GetJoyAxis(0, JoyAxis.LeftY);

        if (Mathf.Abs(playerRotation.X) >= 0.09 && Mathf.Abs(playerRotation.Y) >= 0.09) {
          Rotation = Mathf.Atan2(playerRotation.Y, playerRotation.X) + Mathf.Pi / 2;
        }
      }
    }

    if (!charging) {
      IdleAnimation(delta);
    }

    if (Input.IsActionPressed("chargeButton")) {
      if (allowCharge && !moving) {
        if (!charging) {
          AudioManager.PlaySFX(chargeSFX, 1, false, GlobalPosition, "", "SfxLower");
        }
        charging = true;
        JumpAnimation(true);
      }
    } else if (Input.IsActionJustReleased("chargeButton")) {
      JumpAnimation(false);
      chargeDirection.X = Mathf.Sin(Rotation);
      chargeDirection.Y = -Mathf.Cos(Rotation);

      AudioManager.PlaySFX(jumpSFX, 1, false, GlobalPosition);

      ChargeDisplay.updateCharge(0);
      charging = false;
      allowCharge = false;
      moving = true;
      SetMoveCooldown();
    }
  }

  private void JumpAnimation(bool on) {
    if (on && chargingAnnimation == false) {
      playerBodyAnimation.Play("jump");
      playerFaceAnimation.Play("jump");
      chargingAnnimation = true;
    } else if (!on && chargingAnnimation == true) {
      playerAnimation.Stop();
      playerBodyAnimation.Stop();
      playerFaceAnimation.Stop();
      chargingAnnimation = false;
    }

    if (charge > 80 && chargingAnnimation == true && on) {
      playerAnimation.Play("jump");
    }
  }

  private void IdleAnimation(double delta) {
    // Decrease the timer
    randomTimer -= (float)delta;

    // When the timer reaches 0, decide whether to play the animation
    if (randomTimer <= 0) {
      // Random chance to play the animation
      if (random.NextDouble() > 0.5) {
        playerBodyAnimation.Play("idle");
        playerFaceAnimation.Play("idle");
      }

      // Reset the timer with a random interval (e.g., 1 to 5 seconds)
      randomTimer = (float)random.NextDouble() * 2 + 1;
    }
  }

  public void BentAnimation() {
    playerBodyAnimation.Play("bent");
    playerFaceAnimation.Play("bent");
    playerAnimation.Play("jump");

    Timer timer = new Timer();
    AddChild(timer);
    timer.WaitTime = 0.3;
    timer.OneShot = true;
    timer.Connect("timeout", Callable.From(OnBentAnimationTimeout));
    timer.Start();
  }

  private void OnBentAnimationTimeout() {
    playerAnimation.Stop();
    playerBodyAnimation.Play("idle");
    playerFaceAnimation.Play("idle");
  }

  public void DamageAnimation() {
    playerFaceAnimation.Play("damage");

    Timer timer = new Timer();
    AddChild(timer);
    timer.WaitTime = 0.6;
    timer.OneShot = true;
    timer.Connect("timeout", Callable.From(OnDamageAnimationTimeout));
    timer.Start();
  }

  private void OnDamageAnimationTimeout() {
    playerFaceAnimation.Play("idle");
  }

  private void UpdateShader() {
    float hp01 = (float)health / (float)maxHealth;

    RenderingServer.GlobalShaderParameterSet("swirl_strength", Mathf.Max(1.0f - hp01, 0.01f));
    playerShader.SetShaderParameter("health", hp01);
  }
}
