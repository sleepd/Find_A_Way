public class PlayerStateRunning : PlayerState
{
    public PlayerStateRunning(PlayerStateMachin stateMachin) : base(stateMachin)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        StateMachin.Player.AnimatorController.SetBool("IsRunning", true);
    }

    public override void OnExit()
    {
        base.OnExit();
        StateMachin.Player.AnimatorController.SetBool("IsRunning", false);
    }

    public override void Update()
    {
        base.Update();
        var moveDir = StateMachin.Player.Input.MoveDirection;
        if (moveDir.sqrMagnitude == 0f)
        {
            StateMachin.ChangeState<PlayerStateIdle>();
            return;
        }
        StateMachin.Player.Movement.Rotate(moveDir);
    }
}