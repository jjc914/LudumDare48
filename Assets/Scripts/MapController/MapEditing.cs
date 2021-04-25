using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEditing : MonoBehaviour
{
    [SerializeField] private Tilemap _baseTilemap;
    [SerializeField] private Tilemap _bleedTilemap;

    [SerializeField] private TileBase _unbreakable;

    private MapGeneration _mg;

    private void Awake()
    {
        _mg = GetComponent<MapGeneration>();
    }

    public void Explode(Vector3 worldPos, int radius)
    {
        Vector3Int cellPos = _baseTilemap.WorldToCell(worldPos);

        // TODO: add some randomness to explosion
        for (int y = cellPos.y - radius; y <= cellPos.y + radius; y++)
        {
            for (int x = cellPos.x - radius; x < cellPos.x + radius; x++)
            {
                float dst = Vector3.Distance(cellPos, new Vector3(x, y, cellPos.z));

                if (dst <= radius)
                {
                    if (_baseTilemap.GetTile(new Vector3Int(x, y, 0)) != null && !_baseTilemap.GetTile(new Vector3Int(x, y, 0)).Equals(_mg._tiles.unbreakable)) 
                    {
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0), null);
                        _bleedTilemap.SetTile(new Vector3Int(x, y + 1, 0), null);
                        _bleedTilemap.SetTile(new Vector3Int(x, y - 1, 0), null);
                    }
                }
            }
        }
    }
}
