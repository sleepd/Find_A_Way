using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachineBase
{
    private readonly Dictionary<Type, IState> _stateMap;
    private IState _currentState;

    protected StateMachineBase(Dictionary<Type, IState> stateMap, Type initialStateType = null)
    {
        _stateMap = stateMap ?? throw new ArgumentNullException(nameof(stateMap));

        if (initialStateType != null)
        {
            ChangeState(initialStateType);
        }
    }

    public virtual void ChangeState<TState>() where TState : IState
    {
        ChangeState(typeof(TState));
    }

    public virtual void ChangeState(Type stateType)
    {
        if (stateType == null)
        {
            throw new ArgumentNullException(nameof(stateType));
        }

        if (!_stateMap.TryGetValue(stateType, out var nextState))
        {
            Debug.LogWarning($"State {stateType.Name} not registered.");
            return;
        }

        if (_currentState == nextState)
        {
            return;
        }

        _currentState?.OnExit();
        _currentState = nextState;
        _currentState.OnEnter();
    }
    
    public virtual void Update()
    {
        _currentState?.Update();
    }
}
