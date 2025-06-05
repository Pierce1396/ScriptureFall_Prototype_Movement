using System.Collections.Generic;
using UnityEngine;

public static class GridUtils
{
    public static Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    public static Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x, gridPos.y);
    }

    public static bool IsGridPositionOccupied(Vector2Int position, Transform self, List<EnemyAI> enemies)
    {
        Vector2Int selfGrid = WorldToGrid(self.position);

        // Don't count yourself
        if (position == selfGrid)
            return false;

        // Check player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.transform != self)
        {
            if (WorldToGrid(player.transform.position) == position)
                return true;
        }

        // Check enemies
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy.transform != self)
            {
                Vector2Int enemyGrid = WorldToGrid(enemy.transform.position);
                if (enemyGrid == position)
                    return true;
            }
        }

        return false;
    }
}
