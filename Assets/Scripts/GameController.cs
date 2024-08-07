using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public BeatTimer beatTimer;
    public GameState currentState;
    public GameObject[,] grid = new GameObject[6, 6];
    public GameObject tilePrefab; // Reference to the tile prefab
    public GameObject trianglePrefab; // Reference to the triangle prefab
    public float tileSize = 5.0f;
    public Player player;
    public List<Triangle> enemies;
    public List<SpotlightSquare> spotlights;
    public SpectatorCrowd crowd;
    public DJ dj;
    public LevelManager levelManager;
    public UIManager uiManager;
    public AudioManager audioManager;
    [SerializeField]private int totalTrianglesToSpawn;
    private int trianglesSpawned;
    
    [SerializeField] private Text healthText; // UI element to display player's health

    void Start()
    {
        CenterCamera();
        InitializeGrid();
        StartGame();

        beatTimer.OnBeat += HandleBeat;
    }

    void HandleBeat()
    {
        MoveActors();
        SpawnEnemies();
    }

    void CenterCamera()
    {
        float centerX = (6 * tileSize - tileSize) / 2.0f;
        float centerY = (6 * tileSize - tileSize) / 2.0f;
        Camera.main.transform.position = new Vector3(centerX, centerY, Camera.main.transform.position.z);
    }

    void InitializeGrid()
    {
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                grid[x, y] = Instantiate(tilePrefab, new Vector2(x * tileSize, y * tileSize), Quaternion.identity);
                grid[x, y].transform.localScale = new Vector3(tileSize, tileSize, 1);
                grid[x, y].name = $"Tile_{x}_{y}";
                if((x+y)%2==1) grid[x,y].GetComponent<SpriteRenderer>().color = Color.cyan;
            }
        }
    }

    void StartGame()
    {
        ChangeState(GameState.Play);
        trianglesSpawned = 0;
        LoadLevel(1);
        SpawnEnemies(); // Ensure enemies are spawned when the game starts
        UpdateHealthText(); // Update health display at the start of the game
    }

    void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log("Game State Changed to: " + newState);
    }

    void PauseGame()
    {
        ChangeState(GameState.Pause);
    }

    void ResumeGame()
    {
        ChangeState(GameState.Play);
    }

    void EndGame()
    {
        ChangeState(GameState.End);
    }

    void Update()
    {
        if (currentState == GameState.Play)
        {
            player.HandleInput();
            MoveActors();
            CheckCollisions();
            UpdateUI();
        }
    }

    void HandleInput()
    {
    }

    void CheckCollisions()
    {
    }

    void MoveActors()
    {
    }

    void LoadLevel(int levelNumber)
    {
        //levelManager.LoadLevel(levelNumber);
    }

    void GenerateLevel()
    {
    }

    void SpawnEnemies()
    {
        int spawnCount = Mathf.Min(6, totalTrianglesToSpawn - trianglesSpawned);
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

        // Add the player's current position to occupied positions
        occupiedPositions.Add(player.position);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2Int spawnPosition = GetRandomSpawnPosition(occupiedPositions);
            if (spawnPosition != Vector2Int.zero)
            {
                GameObject triangleObject = Instantiate(trianglePrefab, new Vector2(spawnPosition.x * tileSize, spawnPosition.y * tileSize), Quaternion.identity);
                Triangle triangle = triangleObject.GetComponent<Triangle>();
                triangle.position = spawnPosition;
                triangle.gameController = this;
                enemies.Add(triangle);
                occupiedPositions.Add(spawnPosition);

                // Ensure the triangle's first move is inside the grid
                triangle.currentDirection = GetInitialDirection(spawnPosition);
                triangle.nextPosition = triangle.position + triangle.currentDirection;

                trianglesSpawned++;
            }
        }
    }

    Vector2Int GetRandomSpawnPosition(HashSet<Vector2Int> occupiedPositions)
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();

        // Add all possible positions on the left and right sides
        for (int y = 0; y < 6; y++)
        {
            Vector2Int leftPosition = new Vector2Int(0, y);
            Vector2Int rightPosition = new Vector2Int(5, y);
            if (!occupiedPositions.Contains(leftPosition))
            {
                possiblePositions.Add(leftPosition);
            }
            if (!occupiedPositions.Contains(rightPosition))
            {
                possiblePositions.Add(rightPosition);
            }
        }

        if (possiblePositions.Count == 0)
        {
            return Vector2Int.zero; // No valid position found
        }

        int randIndex = Random.Range(0, possiblePositions.Count);
        return possiblePositions[randIndex];
    }

    Vector2Int GetInitialDirection(Vector2Int spawnPosition)
    {
        // If the triangle spawns on the left side, it should move right
        if (spawnPosition.x == 0)
        {
            return Vector2Int.right;
        }
        // If the triangle spawns on the right side, it should move left
        else if (spawnPosition.x == 5)
        {
            return Vector2Int.left;
        }

        // Default to moving up if neither condition is met (though this should not happen with current spawn logic)
        return Vector2Int.up;
    }

    public void RemoveEnemy(Triangle enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    public void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = player.health.ToString();
        }
    }

    void UpdateUI()
    {
        //uiManager.UpdateUI();
    }

    void ShowGameUI()
    {
    }

    void ShowPauseMenu()
    {
    }

    void ShowGameOverScreen()
    {
    }

    void PlayMusic()
    {
        //audioManager.PlayMusic();
    }

    void StopMusic()
    {
        //audioManager.StopMusic();
    }

    void PlaySoundEffect(string effectName)
    {
        //audioManager.PlaySoundEffect(effectName);
    }

    public void OnPlayerScoreUpdated()
    {
    }

    void OnEnemyDefeated(Triangle enemy)
    {
    }
}
