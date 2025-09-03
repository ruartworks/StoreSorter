
using System.Collections.Generic;
using UnityEngine;

public class ItemLibrary : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    private Dictionary<int, Sprite> _map;

    private void Awake()
    {
        _map = new Dictionary<int, Sprite>();
        foreach (var it in database.items)
            _map[it.id] = it.sprite;
    }

    public Sprite GetSprite(int id) => _map.TryGetValue(id, out var s) ? s : null;
    public ItemDatabase Database => database;
}
