using Unity.VisualScripting;
using UnityEngine;

public class PlayerState : IState
{
    public virtual void OnEnter()
    {
        Debug.Log($"[Player] Entering {typeof(This)}");
    }

    public virtual void OnExit()
    {
        Debug.Log($"[Player] Exiting {typeof(This)}");
    }

    public void Update()
    {
        
    }
}