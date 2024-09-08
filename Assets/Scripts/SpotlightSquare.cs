using UnityEngine;
using System.Collections.Generic;

public class SpotlightSquare: MonoBehaviour
{
    public Vector2Int position;
    public Vector2Int previousPosition; // To track the previous position

    public GameController gameController; // Reference to GameController
    private BeatTimer beatTimer;
    public AudioClip moveSound; // Sound to play when the spotlight moves
    public Vector2Int nextPosition; // Make nextPosition public
    public Vector2Int currentDirection; // Make currentDirection public
    public int powerLevel; // Starting power level for spotlights
    public int moveCount = 0; // Move counter starting at 0
    [SerializeField] private float speed;
    private int health;
    private SpriteRenderer spriteRenderer;

    private AudioSource audioSource;

    private List<Vector2Int> initialMoves = new List<Vector2Int>();

    private Rigidbody2D rb;
    public static Dictionary<GameObject, SpotlightSquare> cachedSpotlights = new Dictionary<GameObject, SpotlightSquare>();

    public void Initialize(Vector2Int initialPosition, GameController controller, BeatTimer timer, int initialPowerLevel)
    {
        position = initialPosition;
        gameController = controller;
        beatTimer = timer;
        powerLevel = initialPowerLevel; // Set power level from the triangle it spawned from
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
        nextPosition = position;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        speed = gameController.tileSize / beatTimer.beatInterval;
        health = powerLevel * 600; // Same health formula as Triangle

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

    private float moveTimer = 0;
    private bool canMove = true;

    void FixedUpdate()
    {
        /*// Check if it's time to stop moving
        if (moveTimer >= beatTimer.beatInterval / 3 && canMove)
        {
            transform.position=new Vector3(nextPosition.x*gameController.tileSize,nextPosition.y*gameController.tileSize,0);
            canMove = false;
            DecideNextMove();
        }

        // Move only if allowed and it's within the first half of the beat interval
        if (canMove && moveTimer < beatTimer.beatInterval / 3)
        {
            float moveDistance = speed * Time.fixedDeltaTime; // Distance to move in one frame
            rb.MovePosition(rb.position + new Vector2(currentDirection.x * moveDistance, currentDirection.y * moveDistance));
        }
        else
        {
            // Stop the movement after half-beat
            rb.velocity = Vector2.zero;
        }

        moveTimer += Time.deltaTime;*/
        if (canMove)
        {
            float moveDistance = speed * Time.fixedDeltaTime / 2; // Distance to move in one frame
            rb.MovePosition(rb.position + new Vector2(currentDirection.x * moveDistance, currentDirection.y * moveDistance));
        }
        
    }

    void OnDestroy()
    {
        if(cachedSpotlights.ContainsKey(gameObject))
        {
            cachedSpotlights.Remove(gameObject);
        }
    }

    private Vector2Int prePosition;

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
        canMove = true;
        moveTimer = 0;
        previousPosition = position; // Store the current position as the previous position
        position = nextPosition;     // Update the current position to the next position
        moveCount++; // Increment move counter

        // Play movement sound
        PlayMoveSound();
    }


    public void MergeSpotlights(int numberOfSpotlights)
    {
        powerLevel = powerLevel * (int)Mathf.Pow(2, numberOfSpotlights - 1);
        health = powerLevel * 350;
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
                color = Color.cyan;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 8:
                color = Color.magenta;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 16:
                color = Color.red;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 32:
                color = Color.blue;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 64:
                color = Color.green;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            case 128:
                color = Color.yellow;
                color.a = 0.5f;
                spriteRenderer.color = color;
                break;
            default:
                color = Color.grey;
                color.a = 0.5f;
                spriteRenderer.color = color; // Default color if power level exceeds 16
                break;
        }
    }

    private Vector2Int GetRandomDirection()
    {
        List<Vector2Int> validDirections = new List<Vector2Int>();

        if (position.y + 1 < gameController.height-1) validDirections.Add(Vector2Int.up);
        if (position.y - 1 >= 0) validDirections.Add(Vector2Int.down);
        if (position.x - 1 >= 0) validDirections.Add(Vector2Int.left);
        if (position.x + 1 < gameController.width-1) validDirections.Add(Vector2Int.right);

        if (validDirections.Count == 0) return Vector2Int.zero; // Stay in place if no valid moves

        int rand = Random.Range(0, validDirections.Count);
        return validDirections[rand];
    }

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

    private void PlayMoveSound()
    {
        if (moveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(moveSound);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Placeholder for handling collisions
    }
}
