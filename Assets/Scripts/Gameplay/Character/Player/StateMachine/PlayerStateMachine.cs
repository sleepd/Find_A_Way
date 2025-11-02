using System;
using System.Collections.Generic;

public class PlayerStateMachin : StateMachineBase
{
    public PlayerStateMachin(Dictionary<Type, IState> stateMap, Type initialStateType = null) : base(stateMap, initialStateType)
    {
    }
}