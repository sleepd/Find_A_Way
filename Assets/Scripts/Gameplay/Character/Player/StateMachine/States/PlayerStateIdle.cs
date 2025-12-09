using UnityEngine;

public class PlayerStateIdle : PlayerState
{
    public PlayerStateIdle(PlayerStateMachin stateMachin) : base(stateMachin)
    {
    }

    public override void Update()
    {
        base.Update();
        var moveDir = StateMachin.Player.Input.MoveDirection;
        if (moveDir.sqrMagnitude > 0f)
        {
            StateMachin.ChangeState<PlayerStateRunning>();
        }
    }

    public override void OnEnter()
    {
        base.OnEnter();
        StateMachin.Player.Input.AimingStarted += HandleAimingStarted;
    }

    public override void OnExit()
    {
        base.OnExit();
        StateMachin.Player.Input.AimingStarted -= HandleAimingStarted;
    }

    void HandleAimingStarted()
    {
        StateMachin.ChangeState<PlayerStateAiming>();
    }
}
