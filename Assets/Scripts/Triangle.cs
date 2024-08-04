using UnityEngine;
using System.Collections.Generic;

public class Triangle : MonoBehaviour
{
    public Vector2Int position;
    public GameController gameController; // Reference to GameController
    public AudioClip moveSound; // Sound to play when the triangle moves

    private AudioSource audioSource;
    private Vector2Int nextPosition;
    private Vector2Int currentDirection;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        position = new Vector2Int((int)(transform.position.x / gameController.tileSize), (int)(transform.position.y / gameController.tileSize));
        
        // Initialize the next position and current direction
        nextPosition = position;
        currentDirection = Vector2Int.up;

        // Subscribe to the OnBeat event from the BeatTimer
        gameController.beatTimer.OnBeat += DecideNextMove;
    }

    void OnDestroy()
    {
        // Unsubscribe from the OnBeat event to prevent memory leaks
        gameController.beatTimer.OnBeat -= DecideNextMove;
    }

    void DecideNextMove()
    {
        currentDirection = GetRandomDirection();
        // Determine the next direction and position
        if (nextPosition.x >= 0 && nextPosition.x < 6 && nextPosition.y >= 0 && nextPosition.y < 6)
        {
            Move();
        }
        
        nextPosition = position + currentDirection;
        
    }

    public void Move()
    {
        // Update the triangle's position
        position = nextPosition;
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);

        // Rotate the triangle to face the direction it will move to next
        Vector3 direction = new Vector3(currentDirection.x, currentDirection.y, 0);
        transform.up = direction;

        // Play movement sound
        PlayMoveSound();
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
