using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public ObstacleTilemap obstacleTilemap;
    public Vector2 gridSize = new Vector2(1f, 1f);

    [Header("Detection & Movement Settings")]
    public float detectionRadius = 5f;
    public int maxStepsPerTurn = 2;
    public float moveSpeed = 5f;

    private bool hasDetectedPlayer = false;

    public void MoveTowardPlayer()
    {
        Vector2 playerPos = GridUtils.GridToWorld(GridUtils.WorldToGrid(player.position));
        Vector2 enemyPos = GridUtils.GridToWorld(GridUtils.WorldToGrid(transform.position));

        float distanceToPlayer = Vector2.Distance(playerPos, enemyPos);

        if (distanceToPlayer <= detectionRadius)
        {
            hasDetectedPlayer = true;
        }

        if (hasDetectedPlayer)
        {
            List<Vector2> path = AStar.FindPath(enemyPos, playerPos, gridSize, obstacleTilemap.IsTileObstacle);
            if (path != null && path.Count > 0)
            {
                int steps = Mathf.Min(maxStepsPerTurn, path.Count);
                StartCoroutine(MoveAlongPath(path.GetRange(0, steps)));
            }
        }
    }

    private IEnumerator MoveAlongPath(List<Vector2> path)
    {
        foreach (var step in path)
        {
            Vector2 targetPos = step + gridSize / 2f;
            while ((Vector2)transform.position != targetPos)
            {
                float moveStep = moveSpeed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, targetPos, moveStep);
                yield return null;
            }
        }
    }

    public bool HasDetectedPlayer()
    {
        return hasDetectedPlayer;
    }
}
