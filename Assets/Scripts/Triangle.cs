using UnityEngine;
using System.Collections.Generic;

public class Triangle : MonoBehaviour
{
    public Vector2Int position;
    public GameController gameController; // Reference to GameController
    public AudioClip moveSound; // Sound to play when the triangle moves
    public Vector2Int nextPosition; // Make nextPosition public
    public Vector2Int currentDirection; // Make currentDirection public

    private AudioSource audioSource;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        position = new Vector2Int((int)(transform.position.x / gameController.tileSize), (int)(transform.position.y / gameController.tileSize));

        audioSource = GetComponent<AudioSource>();

        // Initialize the next position and current direction
        // These will now be set by the GameController

        // Rotate the triangle to face the initial direction
        RotateTowardsDirection(currentDirection);

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
        // Only update direction if the current direction leads to a valid position
        if (nextPosition.x >= 0 && nextPosition.x < 6 && nextPosition.y >= 0 && nextPosition.y < 6)
        {
            Move();
        }

        currentDirection = GetRandomDirection();
        nextPosition = position + currentDirection;

        // Rotate the triangle to face the direction of the next move
        RotateTowardsDirection(currentDirection);
    }

    public void Move()
    {
        // Update the triangle's position
        position = nextPosition;
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);

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

    private void RotateTowardsDirection(Vector2Int direction)
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
