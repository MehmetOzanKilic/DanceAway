using UnityEngine;
using System.Collections.Generic;

public class Triangle : MonoBehaviour
{
    [SerializeField]private int baseHealth=200;    
    public Vector2Int position;
    public GameController gameController; // Reference to GameController
    private BeatTimer beatTimer;
    public AudioClip moveSound; // Sound to play when the triangle moves
    public Vector2Int nextPosition; // Make nextPosition public
    public Vector2Int currentDirection; // Make currentDirection public
    public int powerLevel = 2; // Starting power level for triangles
    public int moveCount = 0; // Move counter starting at 0
    [SerializeField]private float speed;
    [SerializeField]private GameObject spotlightPrefab;
    public int health;
    private SpriteRenderer spriteRenderer;

    private AudioSource audioSource;

    private List<Vector2Int> initialMoves = new List<Vector2Int>();

    private Rigidbody2D rb;


    public void Initialize(Vector2Int initialPosition, GameController controller, BeatTimer timer)
    {
        position = initialPosition;
        gameController = controller;
        beatTimer = timer;
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
        nextPosition = position;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        speed = gameController.tileSize/(beatTimer.beatInterval/3);
        health = powerLevel * baseHealth;
        // Set initial color based on power level
        UpdateColor();
    }

    void Start()
    {

    }

    private float moveTimer=0;
    private bool canMove=true;
    void FixedUpdate()
    {
        // Check if it time totop moving
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

        moveTimer += Time.deltaTime;
    }


    void OnDestroy()
    {
    }

    public void SetInitialMoves()
    {
        if (position.y == 6) // Coming from above
        {
            initialMoves.Add(Vector2Int.down);
            initialMoves.Add(Vector2Int.down);
        }
        else if (position.y == -1) // Coming from below
        {
            initialMoves.Add(Vector2Int.up);
            initialMoves.Add(Vector2Int.up);
        }
        else if (position.x == -1) // Coming from the left
        {
            initialMoves.Add(Vector2Int.right);
            initialMoves.Add(Vector2Int.right);
        }
        else if (position.x == 6) // Coming from the right
        {
            initialMoves.Add(Vector2Int.left);
            initialMoves.Add(Vector2Int.left);
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
        canMove=true;
        moveTimer=0;
        prePosition = position;
        position = nextPosition;
        //transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
        moveCount++; // Increment move counter

        // Check if the triangle steps on the player's position
        if (position == gameController.player.position)
        {
            gameController.player.TakeDamage(200); // Example damage value, you can adjust as needed
        }

        // Play movement sound
        PlayMoveSound();
    }

    public void MergeTriangles(int numberOfTriangles, int combinedHealth)
    {
        powerLevel = powerLevel * (int)Mathf.Pow(2, numberOfTriangles - 1);
        health = combinedHealth + (powerLevel*baseHealth/2);
        UpdateColor(); // Update the color based on the new power level
    }

    void UpdateColor()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        switch (powerLevel)
        {
            case 2:
                spriteRenderer.color = Color.white;
                break;
            case 4:
                spriteRenderer.color = Color.red;
                break;
            case 8:
                spriteRenderer.color = Color.green;
                break;
            case 16:
                spriteRenderer.color = Color.blue;
                break;
            default:
                spriteRenderer.color = Color.white; // Default color if power level exceeds 16
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            gameController.RemoveEnemy(this);
            Destroy(gameObject);
        }
    }

    private Vector2Int GetRandomDirection()
    {
        List<Vector2Int> validDirections = new List<Vector2Int>();

        if (position.y + 1 < 6) validDirections.Add(Vector2Int.up);
        if (position.y - 1 >= 0) validDirections.Add(Vector2Int.down);
        if (position.x - 1 >= 0) validDirections.Add(Vector2Int.left);
        if (position.x + 1 < 6) validDirections.Add(Vector2Int.right);

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
        if (nextPosition.x < 0 || nextPosition.x >= 6 || nextPosition.y < 0 || nextPosition.y >= 6)
        {
            return false; // Out of bounds
        }

        foreach (var triangle in gameController.enemies)
        {
            if (triangle.powerLevel != powerLevel && triangle.nextPosition == nextPosition)
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
