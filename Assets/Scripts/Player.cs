using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public int score;
    public int health = 1000; // Player starting health
    public Vector2Int position;
    public GameController gameController; // Reference to GameController
    public AudioClip moveSound; // Sound to play when the player moves
    public AudioClip hitSound; // Sound to play when the player gets hit
    private AudioSource audioSource;
    private bool canMove = false; // Tracks if the player can move

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        position = new Vector2Int(2, 4); // Starting position at the bottom-left tile
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
        audioSource = GetComponent<AudioSource>();

        // Subscribe to the CanMove event from the BeatTimer
        gameController.beatTimer.CanMove += AllowMovement;
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

    public void Move(Vector2Int direction)
    {
        float timeToNextBeat = Mathf.Abs(gameController.beatTimer.nextBeatTime - gameController.beatTimer.timer);

        if (!canMove || timeToNextBeat > gameController.beatTimer.beatDivider)
        {
            return; // Ignore movement if not allowed
        }

        canMove = false; // Disallow further movement until the next beat

        int scoreIncrement = 0;
        if(gameController.enemies.Count > 0)
        {
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

        }
        

        Vector2Int newPosition = position + direction;

        // Check if the new position is within the grid bounds
        if (newPosition.x >= 0 && newPosition.x < 6 && newPosition.y >= 0 && newPosition.y < 6)
        {
            // Update player's position
            position = newPosition;
            transform.position = new Vector2(newPosition.x * gameController.tileSize, newPosition.y * gameController.tileSize);

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
        }
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
            TakeDamage(50 * triangle.powerLevel); // Example damage value, you can adjust as needed
        }
    }
    private int mult;
    private void CheckSpotlightCollision()
    {
        var spotlight = gameController.spotlights.FirstOrDefault(s => s.nextPosition == position);

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
        gameController.UpdateHealthText(); // Update health display
        if (health <= 0)
        {
            // Handle player death (e.g., end game, restart level)
            Debug.Log("Player has died");
        }

        // Play hit sound
        PlayHitSound();
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
            Move(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right);
        }
    }
}
