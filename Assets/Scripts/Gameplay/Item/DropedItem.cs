using UnityEngine;

public class DropedItem : MonoBehaviour, ICollectable
{
    public string Id { get; private set; }

#if UNITY_EDITOR
    [SerializeField] private ItemData _editorItemData;

    private void OnValidate()
    {
        if (!Application.isPlaying && _editorItemData != null)
        {
            Id = _editorItemData.Id;
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = _editorItemData.Icon;
            }
        }
    }
#endif

    public void Init(string id)
    {
        Id = id;
        ItemData item = GameDatabase.Instance.Items.GetItemById(id);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = item.Icon;
    }

    public void Pickup()
    {
        ItemData item = GameDatabase.Instance.Items.GetItemById(Id);
        LevelManager.Instance.Player.Inventory.AddItem(item, 1);
        Disapear();
    }

    void Disapear()
    {
        Destroy(gameObject);
    }
}