using System;
using System.Collections.Generic;

/// <summary>
/// Register player states here for centralized maintenance.
/// </summary>
public static class PlayerStateDictionary
{
    public static Dictionary<Type, IState> Build(PlayerStateMachin stateMachin)
    {
        return new Dictionary<Type, IState>
        {
            { typeof(PlayerStateIdle), new PlayerStateIdle(stateMachin) },
            { typeof(PlayerStateRunning), new PlayerStateRunning(stateMachin) },
            { typeof(PlayerStateAiming), new PlayerStateAiming(stateMachin) }
        };
    }
}
