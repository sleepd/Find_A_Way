using UnityEngine;

public class PlayerStateIdle : PlayerState
{
    public PlayerStateIdle(PlayerStateMachin stateMachin) : base(stateMachin)
    {
    }

    public override void Update()
    {
        base.Update();
        var moveDir = StateMachin.PlayerController.Input.MoveDirection;
        if (moveDir.sqrMagnitude > 0f)
        {
            StateMachin.PlayerController.Movement.Move(moveDir);
        }
    }
}
