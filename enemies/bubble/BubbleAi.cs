using Godot;
using System.Linq;

public partial class BubbleAi : CharacterBody2D
{
  [Export]
  public float BaseSpeed = 2.0f;

  public float Speed = 2.0f;

  [Export(PropertyHint.Range, "0, 100")]
  public float kepsylon = 2.0f;

  [Export(PropertyHint.Range, "0, 100")]
  public float seekKepsylon = 50.0f;

  [Export(PropertyHint.Range, "0, 100")]
  public float explodeKepsylon = 50.0f;

  [Export]
  public NavigationAgent2D _agent;

  [Export]
  public Line2D path;

  [Export]
  private float maxOffset = 10.0f;

  [Export]
  public float offset;

  private Rid ? _navMesh;

  private int _stepDirection = 1;
  private int _step = 0;

  public override void _Ready() {
    Speed = BaseSpeed;
    offset = RandomUtil.RandomPointInCircle();
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
    var target = GetTarget();
    var direction = Navigate(target);
    this.Velocity = direction * Speed;

    MoveAndCollide(this.Velocity * (float)delta);
  }

  public Vector2 GetTarget() {
    if (path.GetPointCount() == 0) {
      return Vector2.Zero;
    }

    var player = this.GetTree().GetNodesInGroup("Player").Cast<Node2D>().FirstOrDefault();
    if (player != null && this.GlobalPosition.DistanceTo(player.GlobalPosition) < seekKepsylon) {
      return player.GlobalPosition;
    }

    var target = path.Points[_step];

    if (this.GlobalPosition.DistanceTo(target) < this.kepsylon) {
      if (_step == path.GetPointCount() - 1) {
        _stepDirection = -1;
      } else if (_step == 0) {
        _stepDirection = 1;
      }

      _step += _stepDirection;
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

// E 0:00:01:0783   void System.Linq.ThrowHelper.ThrowNoElementsException(): System.InvalidOperationException: Sequence contains no elements
//   <C++ Error>    System.InvalidOperationException
//   <C++ Source>   :0 @ void System.Linq.ThrowHelper.ThrowNoElementsException()
//   <Stack Trace>  :0 @ void System.Linq.ThrowHelper.ThrowNoElementsException()
//                  :0 @ TSource System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable`1[TSource])
//                  BubbleAi.cs:54 @ Godot.Vector2 BubbleAi.GetTarget()
//                  BubbleAi.cs:42 @ void BubbleAi._PhysicsProcess(double)
//                  Node.cs:2389 @ bool Godot.Node.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
//                  CanvasItem.cs:1505 @ bool Godot.CanvasItem.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
//                  Node2D.cs:546 @ bool Godot.Node2D.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
//                  CollisionObject2D.cs:678 @ bool Godot.CollisionObject2D.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
//                  PhysicsBody2D.cs:113 @ bool Godot.PhysicsBody2D.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
//                  CharacterBody2D.cs:786 @ bool Godot.CharacterBody2D.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
//                  BubbleAi_ScriptMethods.generated.cs:68 @ bool BubbleAi.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
//                  CSharpInstanceBridge.cs:24 @ Godot.NativeInterop.godot_bool Godot.Bridge.CSharpInstanceBridge.Call(nint, Godot.NativeInterop.godot_string_name*, Godot.NativeInterop.godot_variant**, int, Godot.NativeInterop.godot_variant_call_error*, Godot.NativeInterop.godot_variant*)
