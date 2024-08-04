using UnityEngine;

public class Player : MonoBehaviour
{
    public int score;
    public Vector2Int position;
    public GameController gameController; // Reference to GameController
    public AudioClip moveSound; // Sound to play when the player moves
    private float timer=0f;

    private AudioSource audioSource;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        position = new Vector2Int(0, 0); // Starting position at the bottom-left tile
        transform.position = new Vector2(position.x * gameController.tileSize, position.y * gameController.tileSize);
    }

    public void Move(Vector2Int direction)
    {
        
        if (gameController.beatTimer.beatDivider < gameController.beatTimer.nextBeatTime - gameController.beatTimer.timer || 
            gameController.beatTimer.beatDivider < gameController.beatTimer.timer-gameController.beatTimer.nextBeatTime)
        {
            return; // Ignore movement if not within the beat window
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
        }
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
