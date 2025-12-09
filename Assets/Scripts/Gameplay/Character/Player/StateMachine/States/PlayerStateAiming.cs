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



    public override void OnExit()
    {
        base.OnExit();
        StateMachin.Player.AnimatorController.SetBool("IsAiming", false);
        StateMachin.Player.Input.AimingCanceled -= HandleAimingCanceled;
    }

    void HandleAimingCanceled()
    {
        StateMachin.ChangeState<PlayerStateIdle>();
    }
}