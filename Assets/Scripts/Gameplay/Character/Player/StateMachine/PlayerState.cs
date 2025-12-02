using UnityEngine;

public class PlayerState : IState
{
    protected PlayerStateMachin StateMachin;
    public PlayerState(PlayerStateMachin stateMachin)
    {
        this.StateMachin = stateMachin;
    }
    public virtual void OnEnter()
    {
        Debug.Log($"[Player] Entering {GetType()}");
    }

    public virtual void OnExit()
    {
        Debug.Log($"[Player] Exiting {GetType()}");
    }

    public virtual void Update()
    {
        
    }
}
