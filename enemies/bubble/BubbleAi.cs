using Godot;
using System.Linq;

public partial class BubbleAi : CharacterBody2D {
  [Export(PropertyHint.Range, "0,100,0.1")]
  public float BaseSpeed = 2.0f;

  [Export(PropertyHint.Range, "0,100,0.1")]
  public float ChaseSpeed = 4.0f;

  public float Speed = 2.0f;

  [Export(PropertyHint.Range, "0,100,1")]
  public float kepsylon = 5.0f;

  [Export(PropertyHint.Range, "0,100,1")]
  public float seekKepsylon = 200.0f;

  [Export(PropertyHint.Range, "0,100,1")]
  public float explodeKepsylon = 20.0f;

  [Export]
  public NavigationAgent2D _agent;

  [Export]
  public AnimatedSprite2D spriteBody;

  [Export]
  public AnimatedSprite2D spriteFace;

  [Export]
  public AudioStream popSFX;

  [Export]
  public Line2D path;

  [Export]
  private float maxOffset = 180.0f;

  [Export]
  private PackedScene explosion;

  [Export]
  private float explosionTimer = 1f;

  [Export]
  private Color warningColor = Colors.Red;

  private bool exploding = false;

  private bool seeking = false;

  private bool popped = false;

  private Vector2 offset;

  private Rid ? _navMesh;

  private Node2D player;

  private int _stepDirection = 1;
  private int _step = 0;

  public override void _Ready() {
    Speed = BaseSpeed;
    offset = RandomUtil.RandomPointInCircle(RandomUtil.Rng.RandfRange(0f, maxOffset));
    var navMesh = GetWorld2D().NavigationMap;

    if (NavigationServer2D.MapIsActive(navMesh)) {
      _navMesh = navMesh;
    } else {
      NavigationServer2D.MapChanged += (Rid rid) => {
        _navMesh = rid;
      };
    }
  }

  public void Pop(bool spawnBomb = false) {
    if (popped) return;

    popped = true;
    AudioManager.PlaySFX(popSFX);
    spriteBody.Animation = "Explode";

    // despawn once explode animation has finished
    spriteBody.AnimationFinished += () => {
      var parent = GetParent();

      if (spawnBomb) {
        var obj = explosion.Instantiate() as Node2D;
        obj.GlobalPosition = this.GlobalPosition;
        parent.AddChild(obj);
      }

      parent.RemoveChild(this);
    };
    spriteBody.Play();
  }

  public override void _PhysicsProcess(double delta) {
    // popped stop processing things
    if (popped)
      return;

    if (seeking && !exploding) {
      spriteBody.Animation = "Idle";
      spriteBody.Play();

      spriteFace.Animation = "Seek";
      spriteFace.Play();
    } else if (!exploding) {
      spriteFace.Animation = "Idle";
      spriteFace.Play();
    }

    // handle exploding
    if (exploding) {
      if (explosionTimer <= 0) {
        Pop(true);
      }

      explosionTimer -= (float) delta;
      return;
    }

    // get player in scene
    player = this.GetTree().GetNodesInGroup("Player").Cast<Node2D>().FirstOrDefault();
    var target = GetTarget();
    var direction = Navigate(target);
    this.Velocity = direction * Speed;

    // hit some wall while trying to move, try going to next step in path
    var collided = MoveAndCollide(this.Velocity * (float)delta);
    if (collided != null) {
      Step();
    }

    // within exploding radius. fucken explode.
    if (player != null && this.GlobalPosition.DistanceTo(player.GlobalPosition) < explodeKepsylon) {
      exploding = true;
      spriteFace.Animation = "Explode";
      spriteFace.Play();
      var tween = GetTree().CreateTween();
      var originalModulate = spriteBody.Modulate;

      // 1/3 of the time
      tween.TweenProperty(spriteBody, "modulate", warningColor, explosionTimer / 3 / 2);
      tween.TweenProperty(spriteBody, "modulate", originalModulate, explosionTimer / 3 / 2);

      // 1/3 of the time
      tween.TweenProperty(spriteBody, "modulate", warningColor, explosionTimer / 3 / 4);
      tween.TweenProperty(spriteBody, "modulate", originalModulate, explosionTimer / 3 / 4);
      tween.TweenProperty(spriteBody, "modulate", warningColor, explosionTimer / 3 / 4);
      tween.TweenProperty(spriteBody, "modulate", originalModulate, explosionTimer / 3 / 4);

      // 1/3 of the time
      tween.TweenProperty(spriteBody, "modulate", warningColor, explosionTimer / 3 / 8);
      tween.TweenProperty(spriteBody, "modulate", originalModulate, explosionTimer / 3 / 8);
      tween.TweenProperty(spriteBody, "modulate", warningColor, explosionTimer / 3 / 8);
      tween.TweenProperty(spriteBody, "modulate", originalModulate, explosionTimer / 3 / 8);
      tween.TweenProperty(spriteBody, "modulate", warningColor, explosionTimer / 3 / 8);
      tween.TweenProperty(spriteBody, "modulate", originalModulate, explosionTimer / 3 / 8);
      tween.TweenProperty(spriteBody, "modulate", warningColor, explosionTimer / 3 / 8);
      tween.TweenProperty(spriteBody, "modulate", originalModulate, explosionTimer / 3 / 8);
    }
  }

  private void Step() {
    if (_step == path.GetPointCount() - 1) {
      _stepDirection = -1;
    } else if (_step == 0) {
      _stepDirection = 1;
    }

    _step += _stepDirection;
  }

  public Vector2 GetTarget() {
    if (player != null && this.GlobalPosition.DistanceTo(player.GlobalPosition) < seekKepsylon) {
      seeking = true;
      return player.GlobalPosition;
    }

    if (path == null || path.GetPointCount() == 0) {
      return this.GlobalPosition;
    }

    seeking = false;
    var target = path.Points[_step] + offset;

    if (this.GlobalPosition.DistanceTo(target) < this.kepsylon) {
      Step();
    }

    return target;
  }

  // get direction to navigate to target
  public Vector2 Navigate(Vector2 target) {
    if (!_navMesh.HasValue)
      return Vector2.Zero;
    _agent.TargetPosition = target;
    _agent.SetNavigationMap(_navMesh.Value);
    var to = _agent.GetNextPathPosition();
    var path = _agent.GetCurrentNavigationPath();

    // have path update fwd
    if (path != null && !path.IsEmpty()) {
      return (to - GlobalPosition).Normalized();
    }

    return Vector2.Zero;
  }
}
