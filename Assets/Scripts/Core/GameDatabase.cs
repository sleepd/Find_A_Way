using UnityEngine;

public class GameDatabase : Singleton<GameDatabase>
{
    [Header("Databases")]
    [SerializeField] private ItemDatabase itemDatabase;

    public ItemDatabase Items => itemDatabase;

    public override void Awake()
    {
        base.Awake();

        if (itemDatabase == null)
        {
            Debug.LogWarning("GameDatabase does not reference an ItemDatabase asset.");
        }
    }
}
