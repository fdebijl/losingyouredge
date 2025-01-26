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
  private int laserDamage = 10;

  [Export]
  private float laserDuration = 0.5f;

  [Export]
  private float laserFireRotationDelay = 0.5f;

  [Export]
  private int health = 100;

  private Player player;
  private float attackTimer;
  private float laserTime = 0f;
  private bool laserActive = false;

  public override void _Ready() {
    spawner.BossMode = true;
    spawner.PatrolPath = PatrolPath;
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

    player = this.GetTree().GetNodesInGroup("Player").Cast<Player>().FirstOrDefault();
    if (player == null)
      return;

    if (this.GlobalPosition.DistanceTo(player.GlobalPosition) > RequiredPlayerRange)
      return;

    if (!laserActive && attackTimer >= laserFireRotationDelay) {
      LookAt(player.GlobalPosition);
      Rotation = Rotation - Mathf.Pi / 2f;
    }

    if (attackTimer <= 0f) {
      SetAttackTimer();

      // Fire at player
      laserActive = true;
      laserSprite.Visible = true;
      laserCollider.ProcessMode = ProcessModeEnum.Inherit;
      laserTime = laserDuration;
    } else if (laserActive && laserTime > 0f) {
      var a = Mathf.Sin(laserTime) + 0.25f;
      laserSprite.Modulate = new Color(1f, 1f, 1f, a);
      laserTime -= (float)delta;
    } else if (laserActive && laserTime <= 0f) {
      laserActive = false;
      laserSprite.Visible = false;
      laserCollider.ProcessMode = ProcessModeEnum.Disabled;
    } else if (!laserActive) {
      attackTimer -= (float)delta;
    }
  }
}
