using Godot;
using System.Linq;

public partial class BubbleAi : CharacterBody2D
{
  [Export]
  public float BaseSpeed = 2.0f;

  public float Speed = 2.0f;

  [Export(PropertyHint.Range, "0, 100")]
  public float kepsylon = 5.0f;

  [Export(PropertyHint.Range, "0, 100")]
  public float seekKepsylon = 20.0f;

  [Export(PropertyHint.Range, "0, 100")]
  public float explodeKepsylon = 5.0f;

  [Export]
  public NavigationAgent2D _agent;

  [Export]
  public Line2D path;

  [Export]
  private float maxOffset = 180.0f;

  [Export]
  private PackedScene explosion;

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

  public override void _PhysicsProcess(double delta) {
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
      var parent = GetParent();
      var obj = explosion.Instantiate();
      parent.AddChild(obj);

      // TODO: death animation += on finish call this
      parent.RemoveChild(this);
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
    if (path.GetPointCount() == 0) {
      return Vector2.Zero;
    }

    if (player != null && this.GlobalPosition.DistanceTo(player.GlobalPosition) < seekKepsylon) {
      return player.GlobalPosition;
    }

    var target = path.Points[_step] + offset;

    if (this.GlobalPosition.DistanceTo(target) < this.kepsylon) {
      Step();
    }

    return target;
  }

  public Vector2 Navigate(Vector2 target) {
    if (!_navMesh.HasValue) return Vector2.Zero;
    _agent.TargetPosition = target;
    _agent.SetNavigationMap(_navMesh.Value);
    var to =_agent.GetNextPathPosition();
    var path = _agent.GetCurrentNavigationPath();

    // have path update fwd
    if (path != null && !path.IsEmpty()) {
      return (to - GlobalPosition).Normalized();
    }

    return Vector2.Zero;
  }
}
