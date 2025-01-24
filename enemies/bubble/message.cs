using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Basic sprite controller
/// Handles taking a direction and updating movement
/// Handles taking a target and updating movement using nav mesh
/// Flips sprite based on movement
/// NOTE: Assumes sprite is left facing
/// NOTE: If target is set GetDirection is not called
/// </summary>
public partial class SpriteController : CharacterBody3D {
  [Export]
  public float BaseSpeed = 2.0f;

  public float Speed = 2.0f;

  [Export]
  private bool _checkOffMesh = false;

  [Export]
  private float OffMeshTolerance = 0.1f;

  [Export]
  protected AnimatedSprite3D _sprite;

  [Export]
  private float _spriteFlipDelay = 0.1f;
  private float _spriteFlipTimer = 0f;

  [Export]
  protected NavigationAgent3D _agent;
  protected Vector3 ? _target;
  protected Rid ? _navMesh = null;

  protected Vector3 _previousVelocity = Vector3.Zero;
  protected Vector3 _previousPosition = Vector3.Zero;

  protected KinematicCollision3D _collision = null;
  protected bool _offMesh = false;

  protected ShaderMaterial _shader;

  static readonly string[] _animations = new string[2] { "idle", "walk" };

  public override void _Ready() {
    Speed = BaseSpeed;
    var navMesh = GetWorld3D().NavigationMap;
    if (NavigationServer3D.MapIsActive(navMesh)) {
      _navMesh = navMesh;
    } else {
      NavigationServer3D.MapChanged += (Rid rid) => {
        _navMesh = rid;
      };
    }

    _shader = _sprite.MaterialOverride as ShaderMaterial;
  }

  private float _slowDebuffTimer = 0f;

  /// <summary>
  /// Apply slow that last given duration
  /// </summary>
  /// <param name="slow">Percent of based speed to slow down (e.g. 0.95 is 95% of BaseSpeed)</param>
  /// <param name="slowDebuffTime">Length the debuff should last</param>
  public void ApplySlow(float slow, float slowDebuffTime) {
    Speed = BaseSpeed * slow;
    _slowDebuffTimer = slowDebuffTime;
  }

  private void UpdateSlowDebuff(double delta) {
    if (_slowDebuffTimer <= 0f) {
      Speed = BaseSpeed;
      return;
    }

    _slowDebuffTimer -= (float) delta;
  }

  public override string[] _GetConfigurationWarnings() {
    var issues = new List<string>();

    if (_sprite == null)
      issues.Add("Missing sprite");

    return issues.ToArray();
  }

  public virtual Vector3 GetDirection() {
    return Vector3.Zero;
  }

  public virtual void UpdateAnimation() {
    // update animation
    string desiredAnimation;

    if (!_animations.Any(x => x.Equals(_sprite.Animation)) && _sprite.IsPlaying())
      return;
    if (Velocity.Length() > 0f) {
      desiredAnimation = "walk";
    } else {
      desiredAnimation = "idle";
    }
    if (_sprite.Animation != desiredAnimation) {
      _sprite.Animation = desiredAnimation;
      _sprite.Play(desiredAnimation);
    }

  }

  public Texture2D CurrentFrame() {
    return _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame);
  }

  public virtual void UpdateShader() {
    if (_shader == null)
      return;

    // Get the texture for the current frame
    /* var texture = CurrentFrame(); */

    /* _sprite.SetInstanceShaderParameter("sick", false); */
    /* _shader.SetShaderParameter("frame", CurrentFrame()); */
  }

  /// <summary>
  /// Navigate to target position
  ///
  /// Only call if agent, target, and nav mesh exist
  /// </summary>
  /// <returns>Vector3</returns>
  private Vector3 Navigate() {
    _agent.SetNavigationMap(_navMesh.Value);
    _agent.TargetPosition = _target.Value;
    var to = _agent.GetNextPathPosition();
    var path = _agent.GetCurrentNavigationPath();

    if (_offMesh) {
      var onMesh = NavigationServer3D.MapGetClosestPoint(_navMesh.Value, GlobalPosition);
      if (onMesh.DistanceTo(GlobalPosition) > _agent.Radius) {
        to = onMesh;
      }
    }

    // // have path update fwd
    // if (!path.IsEmpty()) {
    //   return (to - GlobalPosition).NormalizedXZ();
    // }

    return Vector3.Zero;
  }


  /// <summary>
  /// Flip sprite based on velocity relative to camera
  /// </summary>
  private void FlipSprite(Vector3 velocity, double delta) {
    _spriteFlipTimer += (float) delta;
    Camera3D camera = GetViewport().GetCamera3D();
    Vector3 cameraForward = -camera.GlobalTransform.Basis.Z;
    cameraForward.Y = 0; // Ignore vertical component
    cameraForward = cameraForward.Normalized();

    // Get camera's right vector
    Vector3 cameraRight = new Vector3(cameraForward.Z, 0, -cameraForward.X);

    // Project velocity onto camera's right vector to get local horizontal movement
    float horizontalMovement = velocity.Dot(cameraRight);

    // Flip sprite based on movement relative to camera view
    if (Mathf.Abs(horizontalMovement) > 0.1f && _spriteFlipTimer > _spriteFlipDelay) {
      _sprite.FlipH = horizontalMovement < 0;
      _spriteFlipTimer = 0f;
    }
  }

  /// <summary>
  /// 1. Get direction
  ///   - Uses Navigate() if agent, target, and nav mesh exist
  ///   - Otherwise uses GetDirection()
  /// 2. Flip sprite based on direction
  /// 3. MoveAndCollide() and UpdateAnimation()
  /// </summary>
  /// <param name="delta">time delta</param>
  public override void _PhysicsProcess(double delta) {
    // update velocity
    var velocity = Velocity;
    var direction = Vector3.Zero;

    // use target if set
    if (_agent != null && _target.HasValue && _navMesh.HasValue) {
      direction = Navigate();
    } else {
      // else use direction
      direction = GetDirection();
    }

    if (direction != Vector3.Zero) {
      velocity.X = direction.X * Speed;
      velocity.Z = direction.Z * Speed;
    } else {
      velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
      velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
    }

    FlipSprite(velocity, delta);

    if (_navMesh.HasValue && _checkOffMesh) {
      var predictedPosition = GlobalPosition + Velocity * (float) delta;
      var point = NavigationServer3D.MapGetClosestPoint(_navMesh.Value, predictedPosition);
      _offMesh = point.DistanceTo(predictedPosition) > OffMeshTolerance;
    }

    _previousPosition = GlobalPosition;
    _previousVelocity = Velocity;
    Velocity = velocity;
    _collision = MoveAndCollide(Velocity * (float)delta);

    UpdateAnimation();
    UpdateShader();
    UpdateSlowDebuff(delta);
  }
}
