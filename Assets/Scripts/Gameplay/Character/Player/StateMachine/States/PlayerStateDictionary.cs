using System;
using System.Collections.Generic;

/// <summary>
/// 统一在这里注册玩家状态，便于集中维护。
/// </summary>
public static class PlayerStateDictionary
{
    public static Dictionary<Type, IState> Build(PlayerStateMachin stateMachin)
    {
        // 如果状态构造函数需要参数，可在这里统一传入。
        return new Dictionary<Type, IState>
        {
            { typeof(PlayerStateIdle), new PlayerStateIdle(stateMachin) },
            { typeof(PlayerStateRunning), new PlayerStateRunning(stateMachin) },
            { typeof(PlayerStateAiming), new PlayerStateAiming(stateMachin) }
        };
    }
}
