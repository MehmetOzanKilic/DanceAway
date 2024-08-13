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
    public GameObject spotLightPrefab;
    public float tileSize = 5.0f;
    public Player player;
    public List<Triangle> enemies = new List<Triangle>();
    public List<SpotlightSquare> spotlights = new List<SpotlightSquare>();
    public SpectatorCrowd crowd;
    public DJ dj;
    public LevelManager levelManager;
    public UIManager uiManager;
    public AudioManager audioManager;
    private int totalTrianglesToSpawn;
    private int trianglesSpawned;

    [SerializeField] private Text healthText; // UI element to display player's health

    private int beatCounter = 0;
    private bool isSpawningEnemies = false; // Flag to track enemy spawning
    [SerializeField]private int levelNo=1;

    void Start()
    {
        CenterCamera();
        InitializeGrid();
        StartGame();

        beatTimer.OnBeat += HandleBeat;
    }

    void HandleBeat()
    {
        Debug.Log("HandleBeat called");

        beatCounter++;

        foreach (var enemy in enemies)
        {
            enemy.Move();
        }
        
        foreach (var spotlight in spotlights)
        {
            spotlight.Move();
        }

        if (beatCounter % 2 == 0 && isSpawningEnemies)
        {
            SpawnRemainingEnemies();
        }

        if(enemies.Count == 0 && !isSpawningEnemies)
        {
            levelNo++;
            totalTrianglesToSpawn = levelNo;
            trianglesSpawned = 0;
            isSpawningEnemies = true;
        }

        HandleMerging();
        HandleSpotlightMerging();
        SwitchColor();
    }

    void SwitchColor()
    {
        for(int x = 0; x<6; x++)
        {
            for (int y = 0; y<6; y++)
            {
                grid[x,y].GetComponent<SpriteRenderer>().color =  GetRandomColor();
            }
        }
    }

    private Color GetRandomColor()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        return new Color(r, g, b);
    }

    void HandleMerging()
    {
        var mergeGroups = new Dictionary<Vector2Int, List<Triangle>>();

        // Group triangles by their positions
        foreach (var enemy in enemies)
        {
            if (!mergeGroups.ContainsKey(enemy.position))
            {
                mergeGroups[enemy.position] = new List<Triangle>();
            }
            mergeGroups[enemy.position].Add(enemy);
        }

        // Merge triangles in the same position with the same power level and move count greater than 0
        foreach (var group in mergeGroups.Values)
        {
            if (group.Count > 1)
            {
                var mergeCandidates = new List<Triangle>();
                int powerLevel = group[0].powerLevel;

                foreach (var triangle in group)
                {
                    if (triangle.powerLevel == powerLevel && triangle.moveCount > 0)
                    {
                        mergeCandidates.Add(triangle);
                    }
                }

                if (mergeCandidates.Count > 1)
                {
                    MergeTriangles(mergeCandidates);
                }
            }
        }
    }

    void HandleSpotlightMerging()
    {
        var mergeGroups = new Dictionary<Vector2Int, List<SpotlightSquare>>();

        // Group spotlights by their positions
        foreach (var spotlight in spotlights)
        {
            if (!mergeGroups.ContainsKey(spotlight.position))
            {
                mergeGroups[spotlight.position] = new List<SpotlightSquare>();
            }
            mergeGroups[spotlight.position].Add(spotlight);
        }

        // Merge spotlights in the same position with the same power level and move count greater than 0
        foreach (var group in mergeGroups.Values)
        {
            if (group.Count > 1)
            {
                var mergeCandidates = new List<SpotlightSquare>();
                int powerLevel = group[0].powerLevel;

                foreach (var spotlight in group)
                {
                    if (spotlight.powerLevel == powerLevel && spotlight.moveCount > 0)
                    {
                        mergeCandidates.Add(spotlight);
                    }
                }

                if (mergeCandidates.Count > 1)
                {
                    MergeSpotlights(mergeCandidates);
                }
            }
        }
    }

    public void MergeTriangles(List<Triangle> mergeCandidates)
    {
        int numberOfTriangles = mergeCandidates.Count;
        Triangle baseTriangle = mergeCandidates[0];

        int healthSum=0;
        foreach (var triangle in mergeCandidates)
        {
            if (triangle != baseTriangle)
            {
                RemoveEnemy(triangle);
                Destroy(triangle.gameObject);
                healthSum+=triangle.health;
            }
        }

        baseTriangle.MergeTriangles(numberOfTriangles,baseTriangle.health+healthSum);
    }

    public void MergeSpotlights(List<SpotlightSquare> mergeCandidates)
    {
        int numberOfSpotlights = mergeCandidates.Count;
        SpotlightSquare baseSpotlight = mergeCandidates[0];

        foreach (var spotlight in mergeCandidates)
        {
            if (spotlight != baseSpotlight)
            {
                RemoveSpotlight(spotlight);
                Destroy(spotlight.gameObject);
            }
        }

        baseSpotlight.MergeSpotlights(numberOfSpotlights);
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
                if ((x + y) % 2 == 1) grid[x, y].GetComponent<SpriteRenderer>().color = Color.cyan;
            }
        }
    }

    void StartGame()
    {
        ChangeState(GameState.Play);
        totalTrianglesToSpawn = levelNo;
        trianglesSpawned = 0;
        LoadLevel(1);
        SpawnInitialEnemies(); // Ensure enemies are spawned when the game starts
        UpdateHealthText(); // Update health display at the start of the game
    }

    void SpawnInitialEnemies()
    {
        int spawnCount = Mathf.Min(6, totalTrianglesToSpawn - trianglesSpawned);
        SpawnEnemies(spawnCount);

        // Set the flag to true if there are more triangles to spawn
        if (totalTrianglesToSpawn > 6)
        {
            isSpawningEnemies = true;
        }
    }

    void SpawnRemainingEnemies()
    {
        if (trianglesSpawned < totalTrianglesToSpawn)
        {
            int spawnCount = Mathf.Min(6, totalTrianglesToSpawn - trianglesSpawned);
            SpawnEnemies(spawnCount);

            if (trianglesSpawned >= totalTrianglesToSpawn)
            {
                isSpawningEnemies = false; // Unset the flag once all triangles are spawned
            }
        }
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

    private int nameCounter = 0;

    void SpawnEnemies(int spawnCount)
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

        // Add the player's current position to occupied positions
        occupiedPositions.Add(player.position);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2Int spawnPosition = GetOutsideSpawnPosition(occupiedPositions);
            if (spawnPosition != Vector2Int.zero)
            {
                GameObject triangleObject = Instantiate(trianglePrefab, new Vector2(spawnPosition.x * tileSize, spawnPosition.y * tileSize), Quaternion.identity);
                Triangle triangle = triangleObject.GetComponent<Triangle>();
                triangle.Initialize(spawnPosition, this, beatTimer);  // Use the Initialize method
                enemies.Add(triangle);
                occupiedPositions.Add(spawnPosition);
                triangle.name = ++nameCounter + "Triangle";

                // Ensure the triangle's first two moves are towards the inside of the grid
                triangle.SetInitialMoves();

                trianglesSpawned++;
            }
        }
    }

    private void SpawnSpotlight(Vector2Int position, int powerLevel)
    {
        GameObject spotlightObject = Instantiate(spotLightPrefab, new Vector2(position.x * tileSize, position.y * tileSize), Quaternion.identity);
        SpotlightSquare spotlight = spotlightObject.GetComponent<SpotlightSquare>();
        spotlight.Initialize(position, this, beatTimer, powerLevel);
        addSpotlight(spotlight);
    }

    public void addSpotlight(SpotlightSquare spotlight)
    {
        spotlights.Add(spotlight);
    }

    Vector2Int GetOutsideSpawnPosition(HashSet<Vector2Int> occupiedPositions)
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        Vector2Int playerPos = player.position;
        List<Vector2Int> excludedPositions = GetExcludedPositions(playerPos);

        // Generate positions just outside the grid on all sides, excluding those closest to the player
        for (int y = 0; y < 6; y++)
        {
            Vector2Int abovePosition = new Vector2Int(y, 6); // Above the top row
            Vector2Int belowPosition = new Vector2Int(y, -1); // Below the bottom row
            Vector2Int leftPosition = new Vector2Int(-1, y); // Left of the leftmost column
            Vector2Int rightPosition = new Vector2Int(6, y); // Right of the rightmost column

            if (!occupiedPositions.Contains(abovePosition) && !excludedPositions.Contains(abovePosition))
                possiblePositions.Add(abovePosition);
            if (!occupiedPositions.Contains(belowPosition) && !excludedPositions.Contains(belowPosition))
                possiblePositions.Add(belowPosition);
            if (!occupiedPositions.Contains(leftPosition) && !excludedPositions.Contains(leftPosition))
                possiblePositions.Add(leftPosition);
            if (!occupiedPositions.Contains(rightPosition) && !excludedPositions.Contains(rightPosition))
                possiblePositions.Add(rightPosition);
        }

        if (possiblePositions.Count == 0)
        {
            return Vector2Int.zero; // No valid position found
        }

        int randIndex = Random.Range(0, possiblePositions.Count);
        return possiblePositions[randIndex];
    }

    List<Vector2Int> GetExcludedPositions(Vector2Int playerPos)
    {
        List<Vector2Int> excludedPositions = new List<Vector2Int>();

        // Determine the side(s) closest to the player
        int minDistanceToEdge = Mathf.Min(playerPos.x, 5 - playerPos.x, playerPos.y, 5 - playerPos.y);

        if (playerPos.y == 5 - minDistanceToEdge)
        {
            for (int x = 0; x < 6; x++)
            {
                excludedPositions.Add(new Vector2Int(x, 6)); // Exclude top row spawns
            }
        }
        if (playerPos.y == minDistanceToEdge)
        {
            for (int x = 0; x < 6; x++)
            {
                excludedPositions.Add(new Vector2Int(x, -1)); // Exclude bottom row spawns
            }
        }
        if (playerPos.x == minDistanceToEdge)
        {
            for (int y = 0; y < 6; y++)
            {
                excludedPositions.Add(new Vector2Int(-1, y)); // Exclude left column spawns
            }
        }
        if (playerPos.x == 5 - minDistanceToEdge)
        {
            for (int y = 0; y < 6; y++)
            {
                excludedPositions.Add(new Vector2Int(6, y)); // Exclude right column spawns
            }
        }

        return excludedPositions;
    }

    public void RemoveEnemy(Triangle enemy)
    {
        if (enemies.Contains(enemy))
        {
            SpawnSpotlight(enemy.position, enemy.powerLevel);
            enemies.Remove(enemy);
        }
    }

    public void RemoveSpotlight(SpotlightSquare spotlight)
    {
        if (spotlights.Contains(spotlight))
        {
            spotlights.Remove(spotlight);
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
