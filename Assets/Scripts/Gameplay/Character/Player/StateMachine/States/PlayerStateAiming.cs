using UnityEngine;

public class PlayerStateAiming : PlayerState
{
    private const float AimingBlendDampTime = 0.1f;

    private static readonly Vector2[] EightDirections =
    {
        new Vector2(1f, 0f),
        new Vector2(1f, 1f).normalized,
        new Vector2(0f, 1f),
        new Vector2(-1f, 1f).normalized,
        new Vector2(-1f, 0f),
        new Vector2(-1f, -1f).normalized,
        new Vector2(0f, -1f),
        new Vector2(1f, -1f).normalized
    };

    public PlayerStateAiming(PlayerStateMachin stateMachin) : base(stateMachin)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        StateMachin.Player.AnimatorController.SetBool("IsAiming", true);
        StateMachin.Player.Input.AimingCanceled += HandleAimingCanceled;
        StateMachin.Player.Input.FireStarted += HandleFireStarted;
        StateMachin.Player.Input.FireCanceled += HandleFireCanceled;
    }

    public override void Update()
    {
        base.Update();
        UpdateAimRotation();
        UpdateAimingMoveAnimation();
        UpdateAimingLocomotion();
    }

    void UpdateAimRotation()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var playerTransform = StateMachin.Player.transform;
        var ray = camera.ScreenPointToRay(StateMachin.Player.Input.PointerPosition);
        var aimPlane = new Plane(Vector3.up, new Vector3(0f, playerTransform.position.y, 0f));

        if (!aimPlane.Raycast(ray, out var enter))
        {
            return;
        }

        var target = ray.GetPoint(enter);
        var direction = target - playerTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        StateMachin.Player.Movement.Rotate(direction);
    }

    void UpdateAimingMoveAnimation()
    {
        var moveDir = StateMachin.Player.Input.MoveDirection;
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            StateMachin.Player.AnimatorController.SetFloat("MoveX", 0f, AimingBlendDampTime, Time.deltaTime);
            StateMachin.Player.AnimatorController.SetFloat("MoveY", 0f, AimingBlendDampTime, Time.deltaTime);
            return;
        }

        var transform = StateMachin.Player.transform;
        var forward = transform.forward;
        forward.y = 0f;

        var forward2D = new Vector2(forward.x, forward.z);
        if (forward2D.sqrMagnitude < 0.0001f)
        {
            forward2D = Vector2.up;
        }
        forward2D.Normalize();

        var right2D = new Vector2(forward2D.y, -forward2D.x);
        var input2D = new Vector2(moveDir.x, moveDir.z);

        // Project move input into the player's local space and snap to the nearest of eight directions.
        var projected = new Vector2(Vector2.Dot(input2D, right2D), Vector2.Dot(input2D, forward2D));
        var snapped = SnapToOctant(projected);

        StateMachin.Player.AnimatorController.SetFloat("MoveX", snapped.x, AimingBlendDampTime, Time.deltaTime);
        StateMachin.Player.AnimatorController.SetFloat("MoveY", snapped.y, AimingBlendDampTime, Time.deltaTime);
    }

    void UpdateAimingLocomotion()
    {
        var moveDir = StateMachin.Player.Input.MoveDirection;
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            return;
        }
        var moveDelta = moveDir.normalized * StateMachin.Player.WalkingSpeed * Time.deltaTime;
        StateMachin.Player.Movement.Move(moveDelta);
    }

    static Vector2 SnapToOctant(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        var normalized = input.normalized;
        var bestDir = EightDirections[0];
        var bestDot = Vector2.Dot(normalized, bestDir);

        for (int i = 1; i < EightDirections.Length; i++)
        {
            var dot = Vector2.Dot(normalized, EightDirections[i]);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestDir = EightDirections[i];
            }
        }

        return bestDir;
    }

    public override void OnExit()
    {
        base.OnExit();
        StateMachin.Player.AnimatorController.SetBool("IsAiming", false);
        StateMachin.Player.Input.AimingCanceled -= HandleAimingCanceled;
        StateMachin.Player.Input.FireStarted -= HandleFireStarted;
        StateMachin.Player.Input.FireCanceled -= HandleFireCanceled;

        // Ensure firing stops when leaving the aiming state.
        HandleFireCanceled();
    }

    void HandleAimingCanceled()
    {
        var moveDir = StateMachin.Player.Input.MoveDirection;
        StateMachin.ChangeState(moveDir.sqrMagnitude > 0f
            ? typeof(PlayerStateRunning)
            : typeof(PlayerStateIdle));
    }

    void HandleFireStarted()
    {
        var weapon = StateMachin.Player.WeaponLoadoutController.CurrentWeapon;
        if (weapon != null)
        {
            weapon.BeginFire();
        }
    }

    void HandleFireCanceled()
    {
        var weapon = StateMachin.Player.WeaponLoadoutController.CurrentWeapon;
        if (weapon != null)
        {
            weapon.EndFire();
        }
    }
}
