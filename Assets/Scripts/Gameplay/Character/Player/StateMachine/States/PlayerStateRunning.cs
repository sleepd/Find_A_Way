public class PlayerStateRunning : PlayerState
{
    public PlayerStateRunning(PlayerStateMachin stateMachin) : base(stateMachin)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        StateMachin.PlayerController.AnimatorController.SetBool("IsRunning", true);
    }

    public override void OnExit()
    {
        base.OnExit();
        StateMachin.PlayerController.AnimatorController.SetBool("IsRunning", false);
    }
}