using System.Linq;
using Godot;

public partial class Boss : Node2D {
  [Export]
  private BubbleSpawner spawner;

  [Export]
  private float MinTimeBetweenAttacks = 1f;

  [Export]
  private float MaxTimeBetweenAttacks = 5f;

  [Export]
  private float RequiredPlayerRange = 1000f;

  [Export]
  public Line2D PatrolPath;

  [Export]
  private AnimatedSprite2D laserSprite;

  [Export]
  private Area2D laserCollider;

  [Export]
  private AudioStream bossLaserSFX;

  [Export]
  private AudioStream bossGrowlSFX;

  [Export]
  private float bossGrowlPeriod = 3.0f;

  [Export]
  private int laserDamage = 20;

  [Export]
  private float laserDuration = 0.5f;

  [Export]
  private float telegraphDuration = 1.5f;

  [Export]
  private float laserFireRotationDelay = 0.5f;

  [Export]
  private float laserTelegraphScale = 0.25f;

  [Export]
  private float laserActiveScale = 2f;

  [Export]
  private int health = 100;

  private Player player;

  private float attackTimer;
  private float laserTimer = 0f;
  private bool laserActive = false;
  private float telegraphTimer = 0f;
  private bool telegraphActive = false;

  public override void _Ready() {
    spawner.BossMode = true;
    spawner.PatrolPath = PatrolPath;
    spawner.IsActive = false;
    laserSprite.Visible = false;
    laserCollider.ProcessMode = ProcessModeEnum.Disabled;
    laserCollider.BodyEntered += OnBodyEntered;
    SetAttackTimer();
  }

  private void OnBodyEntered(Node other) {
    if (!laserActive)
      return;

    var player = other as Player;
    if (player == null)
      return;

    player.Damage(laserDamage);
  }

  private void SetAttackTimer() {
    attackTimer = RandomUtil.Rng.RandfRange(MinTimeBetweenAttacks, MaxTimeBetweenAttacks);
  }

  public override void _PhysicsProcess(double delta) {
    if (spawner.IsDead()) {
      laserSprite.Visible = false;
      laserCollider.ProcessMode = ProcessModeEnum.Disabled;
      return;
    }

    if (bossGrowlPeriod <= 0f) {
      GD.Print("Growl");
      AudioManager.PlaySFX(bossGrowlSFX, 1, false, GlobalPosition);
      bossGrowlPeriod = RandomUtil.Rng.RandfRange(3.0f, 5.0f);
    } else {
      bossGrowlPeriod -= (float)delta;
    }

    player = this.GetTree().GetNodesInGroup("Player").Cast<Player>().FirstOrDefault();
    if (player == null)
      return;

    if (this.GlobalPosition.DistanceTo(player.GlobalPosition) > RequiredPlayerRange)
      return;

    if (!laserActive && !telegraphActive) {
      LookAt(player.GlobalPosition);
      Rotation = Rotation - Mathf.Pi / 2f;
    }

    if (attackTimer <= 0f) {
      SetAttackTimer();
      spawner.playerAnimation.Play("grow");

      telegraphActive = true;
      telegraphTimer = telegraphDuration;
      laserSprite.Visible = true;
      spawner.IsActive = false;

      // telegraph firing
    } else if (telegraphActive && telegraphTimer > 0f) {
      telegraphTimer -= (float)delta;

      var a = Mathf.Sin(telegraphTimer) - 0.50f;
      laserSprite.Modulate = new Color(1f, 1f, 1f, a);
      laserSprite.Scale = new Vector2(laserTelegraphScale, laserSprite.Scale.Y);

      // telegraph finished
    } else if (telegraphActive && telegraphTimer <= 0f) {
      // Fire at player
      laserActive = true;
      laserCollider.ProcessMode = ProcessModeEnum.Inherit;
      laserTimer = laserDuration;
      telegraphActive = false;

      // laser firing
    } else if (laserActive && laserTimer > 0f) {
      AudioManager.PlaySFX(bossLaserSFX, 1, false, GlobalPosition);
      laserSprite.Modulate = new Color(1f, 1f, 1f, 1f);
      laserSprite.Scale = new Vector2(laserActiveScale, laserSprite.Scale.Y);
      laserTimer -= (float)delta;

      // laser finished
    } else if (laserActive && laserTimer <= 0f) {
      laserActive = false;
      laserSprite.Visible = false;
      laserCollider.ProcessMode = ProcessModeEnum.Disabled;
    } else if (!laserActive) {
      spawner.playerAnimation.Play("shake");
      attackTimer -= (float)delta;
      spawner.IsActive = true;
    }
  }
}
