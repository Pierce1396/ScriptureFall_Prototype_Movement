using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSelection : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private float offset = 0.5f;
    [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);

    private Vector2Int highlightedTilePosition = Vector2Int.zero;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("TileSelection requires a SpriteRenderer component.");
        }
    }

    private void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new Vector2Int(
            Mathf.FloorToInt(mouseWorldPos.x / gridSize.x) * Mathf.RoundToInt(gridSize.x),
            Mathf.FloorToInt(mouseWorldPos.y / gridSize.y) * Mathf.RoundToInt(gridSize.y)
        );

        // Check if the mouse is over an obstacle tile in the ObstacleTilemap.
        bool isObstacleTile = false;
        if (obstacleTilemap != null)
        {
            Vector3Int cellPos = obstacleTilemap.WorldToCell(mouseWorldPos);
            if (obstacleTilemap.HasTile(cellPos) && obstacleTilemap.GetTile(cellPos) != null)
            {
                // Mouse is over an obstacle tile.
                isObstacleTile = true;
            }
        }

        if (!isObstacleTile)
        {
            highlightedTilePosition = gridPos;
            Vector2 worldPos = GridUtils.GridToWorld(gridPos) + new Vector2(offset, offset);
            transform.position = worldPos;
        }
    }

    public Vector2Int HighlightedTilePosition
    {
        get { return highlightedTilePosition; }
    }

    public bool IsHighlightedTileClicked(Vector2 clickedPosition)
    {
        Vector2Int gridPos = GridUtils.WorldToGrid(clickedPosition);
        return gridPos == highlightedTilePosition;
    }

    public Vector2 GetHighlightedTilePosition()
    {
        return GridUtils.GridToWorld(highlightedTilePosition);
    }

    public bool IsTileObstacle(Vector2Int position)
    {
        Vector3 worldPosition = GridUtils.GridToWorld(position);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        // Check if there's a collider hit (i.e., an obstacle)
        return hit.collider != null;
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
