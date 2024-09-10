using UnityEngine;
using System.Collections.Generic;

public class SpotlightSquare: MonoBehaviour
{
    [HideInInspector]public Vector2Int position;
    [HideInInspector]public Vector2Int previousPosition; // To track the previous position
    private GameController gameController; // Reference to GameController
    private BeatTimer beatTimer;
    private Vector2Int nextPosition; // Make nextPosition public
    private Vector2Int currentDirection; // Make currentDirection public
    [HideInInspector]public int powerLevel; // Starting power level for spotlights
    [HideInInspector]public int moveCount = 0; // Move counter starting at 0
    [SerializeField] private float speed;
    private SpriteRenderer spriteRenderer;
    private List<Vector2Int> initialMoves = new List<Vector2Int>();
    private Rigidbody2D rb;
    [HideInInspector]public static Dictionary<GameObject, SpotlightSquare> cachedSpotlights = new Dictionary<GameObject, SpotlightSquare>();

    public void Initialize(Vector2Int initialPosition, GameController controller, BeatTimer timer, int initialPowerLevel)
    {
        // Setting initial parameters.
        position = initialPosition;
        gameController = controller;
        beatTimer = timer;
        powerLevel = initialPowerLevel; // Set power level from the triangle it spawned from
        // Setting initial position
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
        nextPosition = position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        speed = gameController.tileSize / beatTimer.beatInterval;
        // Adding each triangle to a static cache to use during collision checks in the player script
        if (!cachedSpotlights.ContainsKey(gameObject))
        {
            cachedSpotlights[gameObject] = this;
        }
        // Set initial color based on power level
        UpdateColor();
    }

    void Start()
    {
    }

    void FixedUpdate()
    {
        // Spotlight move half as slow to give the player chance to be on the spotlight
        float moveDistance = speed * Time.fixedDeltaTime / 2; // Distance to move in one frame
        rb.MovePosition(rb.position + new Vector2(currentDirection.x * moveDistance, currentDirection.y * moveDistance));
    }

    void OnDestroy()
    {
        if(cachedSpotlights.ContainsKey(gameObject))
        {
            cachedSpotlights.Remove(gameObject);
        }
    }

    public void DecideNextMove()
    {
        if (initialMoves.Count > 0)
        {
            currentDirection = initialMoves[0];
            initialMoves.RemoveAt(0);
        }
        else
        {
            currentDirection = GetRandomValidDirection();
        }

        nextPosition = position + currentDirection;
        RotateTowardsDirection(currentDirection);
    }

    public void Move()
    {
        DecideNextMove();
        previousPosition = position; // Store the current position as the previous position
        position = nextPosition;     // Update the current position to the next position
        moveCount++; // Increment move counter
    }


    public void MergeSpotlights(int numberOfSpotlights)
    {
        powerLevel = powerLevel * (int)Mathf.Pow(2, numberOfSpotlights - 1);
        UpdateColor(); // Update the color based on the new power level
    }

    void UpdateColor()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Color color;
        switch (powerLevel)
        {
            case 2:
                color = Color.white;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 4:
                color = Color.red;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 8:
                color = Color.green;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 16:
                color = Color.blue;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 32:
                color = Color.yellow;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 64:
                color = Color.gray;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            default:
                color = Color.black;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
        }
    }

    // Same as Triangle.
    private Vector2Int GetRandomValidDirection()
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (directions.Count > 0)
        {
            int index = Random.Range(0, directions.Count);
            Vector2Int dir = directions[index];
            Vector2Int nextPos = position + dir;

            if (IsValidMove(nextPos))
            {
                return dir;
            }

            directions.RemoveAt(index);
        }

        return Vector2Int.zero; // Stay in place if no valid moves
    }

    private bool IsValidMove(Vector2Int nextPosition)
    {
        if (nextPosition.x < 0 || nextPosition.x >= gameController.width || nextPosition.y < 0 || nextPosition.y >= gameController.height)
        {
            return false; // Out of bounds
        }

        foreach (var spotlight in gameController.spotlights)
        {
            if (spotlight.powerLevel != powerLevel && spotlight.nextPosition == nextPosition)
            {
                return false;
            }
        }
        return true;
    }

    public void RotateTowardsDirection(Vector2Int direction)
    {
        Vector3 directionVector = new Vector3(direction.x, direction.y, 0);
        transform.up = directionVector;
    }
}
