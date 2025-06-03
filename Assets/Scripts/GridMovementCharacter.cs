using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public enum MovementDirection
{
    Up,
    Down,
    Left,
    Right
}

public class GridMovementCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 gridSize = new Vector2(1f, 1f);
    [SerializeField] private ObstacleTilemap obstacleTilemap;
    [SerializeField] private TileSelection tileSelection;

    private Vector2 targetPosition;
    private bool isMoving = false;
    private MovementDirection currentDirection;
    private MovementDirection lastMovementDirection; /*ADDED*/
    private Coroutine movementCoroutine;

    //Animator anim; /*ADDED*/

    private bool isIdle = false; /*ADDED*/
    private float idleDelay = 0.1f; /*ADDED*/

    private void Start() /*ADDED*/
    {
        //anim = GetComponent<Animator>(); /*ADDED*/
        currentDirection = MovementDirection.Down; /*ADDED*/
    }

    private void Update()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        //!isMoving &&
        if (Input.GetMouseButtonDown(0))
        {

            targetPosition = tileSelection.GetHighlightedTilePosition();
            Vector2Int clickedTile = GridUtils.WorldToGrid(targetPosition);

            if (obstacleTilemap.IsTileObstacle(clickedTile)) /*ADDED*/
            {
                Vector2Int nearestNonObstacleTile = FindNearestNonObstacleTile(clickedTile); /*ADDED*/

                if (nearestNonObstacleTile != Vector2Int.zero) /*ADDED*/
                {
                    targetPosition = GridUtils.GridToWorld(nearestNonObstacleTile) + gridSize / 2; /*ADDED*/
                    FindPathToTargetPosition();
                }
            }
            else /*ADDED*/
            {
                if (targetPosition != Vector2.zero) /*ADDED*/
                {
                    FindPathToTargetPosition();
                }
            }

            lastMovementDirection = currentDirection; /*ADDED*/
        }

        if (isMoving)
        {
            MoveTowardsTarget();
        }
        else /*ADDED*/
        {

            //anim.SetBool("Walk Up", false);
            //anim.SetBool("Walk Down", false);
            //anim.SetBool("Walk Left", false);
            //anim.SetBool("Walk Right", false);

        }
    }

    private void FindPathToTargetPosition()
    {
        Vector2 startPosition = GridUtils.GridToWorld(GridUtils.WorldToGrid(transform.position));
        List<Vector2> path = AStar.FindPath(startPosition, targetPosition, gridSize, obstacleTilemap.IsTileObstacle);

        if (path != null && path.Count > 0)
        {
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine); // stop previous movement
            }

            movementCoroutine = StartCoroutine(MoveAlongPath(path));
        }

    }

    private IEnumerator MoveAlongPath(List<Vector2> path)
    {
        isMoving = true;
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < path.Count)
        {
            targetPosition = path[currentWaypointIndex] + gridSize / 2;

            while ((Vector2)transform.position != targetPosition)
            {
                float step = moveSpeed * Time.fixedDeltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

                yield return new WaitForFixedUpdate();
            }

            currentWaypointIndex++;
        }

        isMoving = false;
    }

    private void MoveTowardsTarget()
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        bool isHorizontalMovement = Mathf.Abs(direction.x) > Mathf.Abs(direction.y); /*ADDED*/
        bool isVerticalMovement = Mathf.Abs(direction.y) > Mathf.Abs(direction.x); /*ADDED*/

        if (isHorizontalMovement) /*ADDED*/
        {
            if (direction.x > 0)
            {
                currentDirection = MovementDirection.Right;
                //anim.SetBool("Walk Right", true);
            }
            else
            {
                currentDirection = MovementDirection.Left;
                //anim.SetBool("Walk Left", true);
            }
        }
        else if (isVerticalMovement) /*ADDED*/
        {
            if (direction.y > 0)
            {
                currentDirection = MovementDirection.Up;
                //anim.SetBool("Walk Up", true);
            }
            else
            {
                currentDirection = MovementDirection.Down;
                //anim.SetBool("Walk Down", true);
            }
        }
        else
        {
            // If not moving, set all animation bools to false after a delay
            if (!isIdle) /*ADDED*/
            {
                StartCoroutine(ResetAnimationBoolsWithDelay()); /*ADDED*/
            }
        }
    }

    private IEnumerator ResetAnimationBoolsWithDelay()
    {
        isIdle = true; /*ADDED*/
        yield return new WaitForSeconds(idleDelay); /*ADDED*/

        // Reset all animation bools to false
        //anim.SetBool("Walk Up", false);
        //anim.SetBool("Walk Down", false);
        //anim.SetBool("Walk Left", false);
        //anim.SetBool("Walk Right", false);

        isIdle = false; /*ADDED*/
    }

    private Vector2Int FindNearestNonObstacleTile(Vector2Int startTile) /*ADDED*/
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>(); /*ADDED*/
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); /*ADDED*/

        queue.Enqueue(startTile); /*ADDED*/
        visited.Add(startTile); /*ADDED*/

        while (queue.Count > 0) /*ADDED*/
        {
            Vector2Int currentTile = queue.Dequeue(); /*ADDED*/

            // Check if the current tile is not an obstacle
            if (!obstacleTilemap.IsTileObstacle(currentTile)) /*ADDED*/
            {
                return currentTile; // Found a non-obstacle tile
            }

            // Add neighboring tiles to the queue if not visited
            foreach (Vector2Int neighbor in GetAdjacentTiles(currentTile)) /*ADDED*/
            {
                if (!visited.Contains(neighbor)) /*ADDED*/
                {
                    queue.Enqueue(neighbor); /*ADDED*/
                    visited.Add(neighbor); /*ADDED*/
                }
            }
        }

        return Vector2Int.zero; // No available non-obstacle tile found /*ADDED*/
    }

    private List<Vector2Int> GetAdjacentTiles(Vector2Int position) /*ADDED*/
    {
        return new List<Vector2Int> /*ADDED*/
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right
        };
    }
}