using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int score;
    private int maxHealth=100;
    public int health; // Player starting health
    public Vector2Int position;
    public Vector2Int prePosition;
    public GameController gameController; // Reference to GameController
    public AudioClip moveSound; // Sound to play when the player moves
    public AudioClip hitSound; // Sound to play when the player gets hit
    private AudioSource audioSource;
    private bool canMove = false; // Tracks if the player can move
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        position = new Vector2Int(3,0); // Starting position at the bottom-left tile
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        animator.Play("idle");
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;

        // Subscribe to the CanMove event from the BeatTimer
        gameController.beatTimer.CanMove += AllowMovement;
    }

    private float moveTimer=0f;
    void FixedUpdate()
    {
        if (moveTimer >= gameController.beatTimer.beatInterval/3 && canMove)
        {
            transform.position=new Vector3(position.x*gameController.tileSize,position.y*gameController.tileSize,0);
            moveTimer=0;
        }

        moveTimer += Time.deltaTime;
    }

    void OnDestroy()
    {
        // Unsubscribe from the CanMove event to prevent memory leaks
        gameController.beatTimer.CanMove -= AllowMovement;
    }

    void AllowMovement()
    {
        canMove = true;
    }
    private Vector2Int newPosition;
    private int moveCount=0;  
    private int avarage=0;
    [SerializeField]private Text scoreText;

    public int moveAllowed=1;
    public void Move(Vector2Int direction)
    {
        if(gameController.enemies.Count > 0)moveCount++;
        float timeToNextBeat = Mathf.Abs(gameController.beatTimer.nextBeatTime - gameController.beatTimer.timer);

        if (!canMove || timeToNextBeat > gameController.beatTimer.tolerance*8)
        {
            return; // Ignore movement if not allowed
        }

        canMove = false; // Disallow further movement until the next beat


        newPosition = position + direction;

        // Check if the new position is within the grid bounds
        if (newPosition.x >= 0 && newPosition.x < 6 && newPosition.y >= 0 && newPosition.y < 6)
        {
            int scoreIncrement = 0;
            if(gameController.enemies.Count > 0)
            {
                print(moveCount);
                if (gameController.beatTimer.backGround.color == Color.black)
                {
                    scoreIncrement = 200; // Black threshold
                }
                else if (gameController.beatTimer.backGround.color == Color.red)
                {
                    scoreIncrement = 150; // Red threshold
                }
                else if (gameController.beatTimer.backGround.color == Color.blue)
                {
                    scoreIncrement = 100; // Red threshold
                }
                else if (gameController.beatTimer.backGround.color == Color.green)
                {
                    scoreIncrement = 50; // Red threshold
                }
                else
                {
                    scoreIncrement = 0;
                }

            }
            // Update player's position
            prePosition = position;
            position = newPosition;
            rb.AddForce(direction*2000);
        
            // Play movement sound
            PlayMoveSound();

            // Hit the closest triangle(s)
            HitClosestTriangles(scoreIncrement);

            // Check if the player lands on a triangle
            CheckPlayerCollision();

            // Check if the player is on the same tile as a spotlight
            CheckSpotlightCollision();

            print(mult + "  " + scoreIncrement);
            score += scoreIncrement*mult;
            scoreText.text = score.ToString();
            gameController.avarage = score/moveCount;

            gameController.CheckPlayerHeartCollision();
        }

        StartCoroutine(ResetAnimation("Player_Moving"));
    }

    private void PlayHand()
    {
        gameController.PlayHand();
    }

    private IEnumerator ResetAnimation(string animationName)
    {
        // Get the runtime animator controller
        if(!takingDamage)animator.Play(animationName);

        // Wait until the animation has finished playing
        yield return new WaitForSeconds(0.2f);

        // Optionally, reset the animation to the default state
        if(!takingDamage)animator.Play("idle"); // Replace "Idle" with the name of your default animation state
    }

    private void HitClosestTriangles(int damage)
    {
        if (gameController.enemies.Count == 0)
        {
            return; // No enemies to hit
        }

        // Calculate the minimum distance to any triangle
        float minDistance = gameController.enemies.Min(t => Vector2Int.Distance(position, t.position));

        // Find all triangles at the minimum distance
        var closestTriangles = gameController.enemies
            .Where(t => Mathf.Approximately(Vector2Int.Distance(position, t.position), minDistance))
            .ToList();

        // Deal damage to the closest triangle(s)
        foreach (var triangle in closestTriangles)
        {
            triangle.TakeDamage(damage);
        }
    }

    private void CheckPlayerCollision()
    {
        var triangle = gameController.enemies.FirstOrDefault(t => t.position == position);
        if (triangle != null)
        {
            TakeDamage(triangle.powerLevel); // Example damage value, you can adjust as needed
        }
    }
    private int mult;
    private void CheckSpotlightCollision()
    {
        var spotlight = gameController.spotlights.FirstOrDefault(s => s.position == position);

        if (spotlight != null)
        {
            mult = spotlight.powerLevel; // Multiply score by the spotlight's power level
            Debug.Log($"Player's score multiplied by {spotlight.powerLevel}. New score: {score}");
        }
        else
        {
            mult = 1;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // Handle player death (e.g., end game, restart level)
            Debug.Log("Player has died");
            gameController.OpenEndScreen();
        }

        // Play hit sound
        UpdateHealthBar();
        PlayHitSound();
        StartCoroutine(DamageTaken());
    }

    private void UpdateHealthBar()
    {
        float perc = (float)health/maxHealth;
        int number = (int)(46*perc);

        for (int i = 0; i < 46; i++)
        {
            if(i>number)gameController.health[i].SetActive(false);
            else if (i<=number)gameController.health[i].SetActive(true);
        }
    }

    private bool takingDamage = false;
    private IEnumerator DamageTaken()
    {
        animator.Play("Player_Damage");
        canMove = false;
        takingDamage = true;

        yield return new WaitForSeconds(gameController.beatTimer.beatInterval);

        animator.Play("idle");
        takingDamage=false;
    }

    private void PlayMoveSound()
    {
        if (moveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(moveSound);
        }
    }

    private void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Placeholder for handling collisions
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.eulerAngles = new Vector3(0,0,0);
            Move(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.eulerAngles = new Vector3(0,0,180);
            Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.eulerAngles = new Vector3(0,0,90);
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.eulerAngles = new Vector3(0,0,270);
            Move(Vector2Int.right);
        }
    }
}
