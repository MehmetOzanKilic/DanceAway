using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Collections;

public class Triangle : MonoBehaviour
{
    [SerializeField]private int baseHealth=200;    
    public Vector2Int position;
    public Vector2Int previousPosition; // To track the previous position

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
    public bool isChasingPlayer = false;


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
        animator = GetComponent<Animator>();
        childAnimator = transform.GetChild(0).GetComponent<Animator>();
        animator.Play("Triangle_Idle");
        
        if (childAnimator != null)
        {
            bool hasState = childAnimator.HasState(0, Animator.StringToHash("Triangle_Damage_Idle"));
            Debug.Log("Animator has Triangle_Damage_Idle: " + hasState);
        }
        childAnimator.Play("Triangle_Damage_Idle");

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
        if (position.y ==  gameController.height) // Coming from above
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
        else if (position.x == gameController.width) // Coming from the right
        {
            initialMoves.Add(Vector2Int.left);
            initialMoves.Add(Vector2Int.left);
        }
    }
    public bool flag2D = false;
    public void DecideNextMove()
    {
        if(position.y == 0)
        {
            currentDirection = Vector2Int.down;
        }
        else if (isChasingPlayer)
        {
            currentDirection = GetDirectionTowardsPlayer();
        }
        else if (initialMoves.Count > 0)
        {
            currentDirection = initialMoves[0];
            initialMoves.RemoveAt(0);
        }
        else
        {
            if(flag2D)
            currentDirection = GetRandomValidDirection();
            else
            currentDirection = GetRandomValidDirection1D();
        }

        nextPosition = position + currentDirection;
        RotateTowardsDirection(currentDirection);
    }

    private int chasingNo = 0;
    // New method to get direction towards the player
    private Vector2Int GetDirectionTowardsPlayer()
    {
        chasingNo++;
        if(chasingNo%2==0)
        {
            return Vector2Int.down;
        }

        Vector2Int playerPos = gameController.player.position;
        Vector2Int direction = Vector2Int.zero;

        if (position.x < playerPos.x) direction = Vector2Int.right;
        else if (position.x > playerPos.x) direction = Vector2Int.left;
        else if (position.y < playerPos.y) direction = Vector2Int.up;
        else if (position.y > playerPos.y) direction = Vector2Int.down;

        if (IsValidMove(position + direction))
            return direction;

        return GetRandomValidDirection(); // Fallback to random move if the direct path is blocked
    }


    public void Move()
    {
        if(position.y == 0)
        {
            nextPosition.y = gameController.height-1;
        }
        canMove=true;
        moveTimer=0;
        previousPosition = position;
        position = nextPosition;

        //transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
        moveCount++; // Increment move counter

        // Check if the triangle steps on the player's position
        if (position == gameController.player.position)
        {
            gameController.player.TakeDamage(powerLevel); // Example damage value, you can adjust as needed
        }

        // Play movement sound
        PlayMoveSound();
        StartCoroutine(ResetAnimation("Triangle_Moving"));
    }

    private IEnumerator ResetAnimation(string animationName)
    {
        // Get the runtime animator controller
        animator.Play(animationName);

        // Wait until the animation has finished playing
        yield return new WaitForSeconds(0.45f);

        // Optionally, reset the animation to the default state
        animator.Play("Triangle_Idle"); // Replace "Idle" with the name of your default animation state
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

        StartCoroutine(DamageTaken());
        if (health <= 0)
        {
            gameController.RemoveEnemy(this);
            Destroy(gameObject);
        }
    }
    private Animator animator;
    private Animator childAnimator;
    private IEnumerator DamageTaken()
    {
        childAnimator.Play("Triangle_Damage");

        yield return new WaitForSeconds(beatTimer.beatInterval/2
        );
        
        childAnimator.Play("Triangle_Damage_Idle");
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

    private Vector2Int GetRandomValidDirection1D()
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.down,
            Vector2Int.down,
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
        if(direction == Vector2.zero)
        {
            direction = Vector2Int.down;
        }
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
