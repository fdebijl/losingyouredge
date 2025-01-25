using Godot;

public partial class BubbleExplosion : Node2D {
  [Export]
  private AnimatedSprite2D sprite;

  [Export]
  private AudioStream explodeSFX;

  [Export]
  private Area2D collider;

  [Export]
  private float despawnTimer = 1f;

  [Export]
  private int damage = 1;

  private bool damaged = false;

  public override void _Ready() {
    collider.BodyEntered += OnBodyEntered;
  }

  private void OnBodyEntered(Node other) {
    if (damaged) return;

    var player = other as Player;
    if (player == null) return;

    damaged = true;
    player.Damage(damage);
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    despawnTimer -= (float) delta;

    if (despawnTimer < 0) {
      AudioManager.PlaySFX(explodeSFX, 1f, false, GlobalPosition);
      GetParent().RemoveChild(this);
    }
  }
}
