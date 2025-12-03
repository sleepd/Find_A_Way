using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachineBase
{
    private Dictionary<Type, IState> _stateMap;
    private IState _currentState;

    protected StateMachineBase()
    {
        
    }

    public void Initialize(Dictionary<Type, IState> stateMap, Type initialStateType = null)
    {
        if (stateMap == null)
        {
            throw new ArgumentNullException(nameof(stateMap));
        }

        if (_stateMap != null)
        {
            Debug.LogWarning("State machine already initialized.");
            return;
        }

        _stateMap = stateMap;

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
        if (_stateMap == null)
        {
            Debug.LogWarning("State machine not initialized.");
            return;
        }

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
        if (_stateMap == null)
        {
            return;
        }

        _currentState?.Update();
    }
}
