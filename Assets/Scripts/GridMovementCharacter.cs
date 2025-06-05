using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovementCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);
    [SerializeField] private ObstacleTilemap obstacleTilemap;
    [SerializeField] private TileSelection tileSelection;

    [Header("Enemy References")]
    public List<EnemyAI> enemies; // Assign in Inspector or auto-find in Start

    private Vector2 targetPosition;
    private bool isMoving = false;
    private Coroutine movementCoroutine;

    private void Start()
    {
        // Auto-populate enemies if not assigned manually
        if (enemies == null || enemies.Count == 0)
        {
            enemies = new List<EnemyAI>(FindObjectsOfType<EnemyAI>());
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            targetPosition = tileSelection.GetHighlightedTilePosition();

            if (targetPosition != Vector2.zero)
            {
                FindPathToTargetPosition();
            }
        }
    }

    private void FindPathToTargetPosition()
    {
        Vector2 startPosition = GridUtils.GridToWorld(GridUtils.WorldToGrid(transform.position));
        List<Vector2> path = AStar.FindPath(startPosition, targetPosition, gridSize, obstacleTilemap.IsTileObstacle);

        if (path != null && path.Count > 0)
        {
            // Check if any enemy has detected the player
            bool detectedByAnyEnemy = enemies.Exists(e => e.HasDetectedPlayer());

            int maxSteps = path.Count;

            if (detectedByAnyEnemy)
            {
                int enemyMaxSteps = 4;               // Enemy moves 4 steps
                int allowedPlayerSteps = enemyMaxSteps / 2;  // Player can move half that (2 steps)
                maxSteps = Mathf.Min(maxSteps, allowedPlayerSteps);
            }

            if (maxSteps > 0)
            {
                List<Vector2> truncatedPath = path.GetRange(0, maxSteps);

                if (movementCoroutine != null)
                {
                    StopCoroutine(movementCoroutine);
                }

                movementCoroutine = StartCoroutine(MoveAlongPath(truncatedPath));
            }
        }
    }

    private IEnumerator MoveAlongPath(List<Vector2> path)
    {
        isMoving = true;

        foreach (Vector2 waypoint in path)
        {
            Vector2 target = waypoint + gridSize / 2;

            while ((Vector2)transform.position != target)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isMoving = false;

        // After player moves, trigger enemy movement
        foreach (var enemy in enemies)
        {
            enemy.MoveTowardPlayer();
        }
    }
}
