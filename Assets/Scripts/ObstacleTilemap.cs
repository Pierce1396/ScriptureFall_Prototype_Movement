using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    private HashSet<Vector3Int> obstacleTilePositions = new HashSet<Vector3Int>();

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        
        InitializeObstacleTiles();
    }

    private void InitializeObstacleTiles()
    {
        obstacleTilePositions.Clear(); // Clear the HashSet

        BoundsInt bounds = tilemap.cellBounds; // Get the bounds of the tilemap
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds); // Get all tiles in the tilemap

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Vector3Int tilePosition = new Vector3Int(bounds.x + x, bounds.y + y, 0);
                    obstacleTilePositions.Add(tilePosition); // Add obstacle tile positions to HashSet
                }
            }
        }
    }

    public bool IsTileObstacle(Vector2 position)
    {
        Vector3Int gridPos = tilemap.WorldToCell(position);

        // Check if the grid position is in the HashSet
        return obstacleTilePositions.Contains(gridPos);
    }
}
