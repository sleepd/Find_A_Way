using System;
public class PlayerStateMachin : StateMachineBase
{
    public PlayerStateMachin(PlayerController playerController) : base()
    {
        Player = playerController;
    }

    public PlayerController Player {get; private set;}
}
