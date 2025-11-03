using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "GameData/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> _items;
    private Dictionary<string, ItemData> _lookup;

    void OnEnable()
    {
        _lookup = new Dictionary<string, ItemData>();
        foreach (var item in _items)
        {
            if (!_lookup.ContainsKey(item.Id))
                _lookup.Add(item.Id, item);
        }
    }
    
    public ItemData GetItemById(string id)
    {
        _lookup.TryGetValue(id, out var data);
        return data;
    }
}