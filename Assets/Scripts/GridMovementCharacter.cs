using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GridMovementCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);
    [SerializeField] private ObstacleTilemap obstacleTilemap;
    [SerializeField] private TileSelection tileSelection;

    [Header("Enemy References")]
    public List<EnemyAI> enemies;

    [Header("UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject gameOverPanel;

    private Vector2 targetPosition;
    private bool isMoving = false;
    private Coroutine movementCoroutine;
    private Coroutine healthTickCoroutine;
    private bool isHealthTicking = false;


    private void Start()
    {
        if (enemies == null || enemies.Count == 0)
        {
            enemies = new List<EnemyAI>(FindObjectsOfType<EnemyAI>());
        }

        gameObject.tag = "Player";

        if (healthBar != null)
        {
            healthBar.maxValue = 50;
            healthBar.value = 50;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        Vector2 playerGridPos = GridUtils.WorldToGrid(transform.position);
        Vector2Int highlightedGridPos = tileSelection.HighlightedTilePosition;

        int distance = Mathf.Abs((int)(highlightedGridPos.x - playerGridPos.x)) +
                       Mathf.Abs((int)(highlightedGridPos.y - playerGridPos.y));

        bool detectedByAnyEnemy = enemies.Exists(e => e.HasDetectedPlayer());
        int maxAllowedSteps = detectedByAnyEnemy ? 2 : int.MaxValue;

        Color redWithAlpha = new Color(1f, 0f, 0f, 200f / 255f);
        Color blackWithAlpha = new Color(0f, 0f, 0f, 200f / 255f);

        tileSelection.SetColor(distance > maxAllowedSteps ? redWithAlpha : blackWithAlpha);

        if (Input.GetMouseButtonDown(0))
        {
            targetPosition = tileSelection.GetHighlightedTilePosition();

            if (targetPosition != Vector2.zero && distance <= maxAllowedSteps)
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
            bool detectedByAnyEnemy = enemies.Exists(e => e.HasDetectedPlayer());
            int maxSteps = detectedByAnyEnemy ? Mathf.Min(path.Count, 2) : path.Count;

            if (maxSteps > 0)
            {
                List<Vector2> truncatedPath = path.GetRange(0, maxSteps);

                if (movementCoroutine != null) StopCoroutine(movementCoroutine);
                movementCoroutine = StartCoroutine(MoveAlongPath(truncatedPath));
            }
        }
    }

    private IEnumerator MoveAlongPath(List<Vector2> path)
    {
        isMoving = true;

        if (healthTickCoroutine != null) StopCoroutine(healthTickCoroutine);
        healthTickCoroutine = StartCoroutine(HealthTick());

        foreach (Vector2 waypoint in path)
        {
            Vector2Int targetGrid = GridUtils.WorldToGrid(waypoint);

            if (GridUtils.IsGridPositionOccupied(targetGrid, transform, enemies))
            {
                Debug.Log("attack");
                DamagePlayer(2); // Enemy attack
                break;
            }

            Vector2 target = waypoint + gridSize / 2;

            while ((Vector2)transform.position != target)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isMoving = false;

        foreach (var enemy in enemies)
        {
            enemy.MoveTowardPlayer();
        }
    }

    private IEnumerator HealthTick()
    {
        isHealthTicking = true;

        while (isMoving && healthBar != null && healthBar.value > 0)
        {
            DamagePlayer(.25f);
            yield return new WaitForSeconds(1f);
        }

        isHealthTicking = false;
    }

    private void DamagePlayer(float amount)
    {
        if (healthBar == null) return;

        healthBar.value = Mathf.Max(healthBar.value - amount, 0);

        if (healthBar.value <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("Game Over");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }
}
