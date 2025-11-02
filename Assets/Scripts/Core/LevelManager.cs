using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public PlayerController Player { get; private set; }
    public override void Awake()
    {
        base.Awake();
        Player = FindAnyObjectByType<PlayerController>();
    }
}
