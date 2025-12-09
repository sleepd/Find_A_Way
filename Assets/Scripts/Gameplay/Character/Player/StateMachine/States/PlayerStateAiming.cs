using UnityEngine;

public class PlayerStateAiming : PlayerState
{
    public PlayerStateAiming(PlayerStateMachin stateMachin) : base(stateMachin)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        StateMachin.Player.AnimatorController.SetBool("IsAiming", true);
        StateMachin.Player.Input.AimingCanceled += HandleAimingCanceled;
    }

    public override void Update()
    {
        base.Update();
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


    public override void OnExit()
    {
        base.OnExit();
        StateMachin.Player.AnimatorController.SetBool("IsAiming", false);
        StateMachin.Player.Input.AimingCanceled -= HandleAimingCanceled;
    }

    void HandleAimingCanceled()
    {
        var moveDir = StateMachin.Player.Input.MoveDirection;
        StateMachin.ChangeState(moveDir.sqrMagnitude > 0f
            ? typeof(PlayerStateRunning)
            : typeof(PlayerStateIdle));
    }
}
