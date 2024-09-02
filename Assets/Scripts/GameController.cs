using System.Collections;
using System.Collections.Generic;
using Common.Enums;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Common.Enums
{
    public enum BeatState
    {
        PerfectBeat,
        CloseBeat,
        MiddleBeat,
        FarBeat,
        OffBeat
    }
}


public class GameController : MonoBehaviour
{
    
    public BeatState State;
    [SerializeField]private GameObject filter;
    [SerializeField]public int width=5;
    [SerializeField]public int height=5; 
    private SpriteRenderer filterSR;
    public int waitSeconds=1;
    AudioSource[] audioSources;  
    [SerializeField]private GameObject healthPrefab;
    public BeatTimer beatTimer;
    public GameState currentState;
    public GameObject[,] grid;
    public GameObject[] health = new GameObject[100];
    public GameObject tilePrefab; // Reference to the tile prefab
    public GameObject trianglePrefab; // Reference to the triangle prefab
    public GameObject spotLightPrefab;
    public GameObject heartPrefab;
    public float tileSize = 8.0f;
    public Player player;
    public List<Triangle> enemies = new List<Triangle>();
    public List<SpotlightSquare> spotlights = new List<SpotlightSquare>();
    public List<GameObject> hearts = new List<GameObject>();
    public SpectatorCrowd crowd;
    public DJ dj;
    public LevelManager levelManager;
    public UIManager uiManager;
    public AudioManager audioManager;
    private int totalTrianglesToSpawn;
    private int trianglesSpawned;
    private int beatCounter = 0;
    private bool isSpawningEnemies = false; // Flag to track enemy spawning
    [SerializeField]private int levelNo=1;
    [SerializeField]private float spawnChance = 0.4f;
    private float change;
    [SerializeField]private CrowdController crowdController;

    public GameObject endScreen;
    void Start()
    {
        grid = new GameObject[width, height];
        CenterCamera();
        InitializeGrid();
        StartGame();
        audioSources = GetComponents<AudioSource>();
        for(int i = 0; i < 6; i++)
        {
            audioSources[i].Stop();
            audioSources[i].pitch = 0.8333f;
        }
        levelText.gameObject.SetActive(false);
        filterSR = filter.GetComponent<SpriteRenderer>();
        change = Random.Range(0, 360);
        endScreen.SetActive(false);
        crowdController.Initialize(beatTimer,width,height,tileSize);
        

        beatTimer.OnBeat += HandleBeat;
        //Camera.main.fieldOfView = 100;
    }

    private bool canSpawn = true;
    void HandleBeat()
    {

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
            if(canSpawn)SpawnRemainingEnemies();
        }

        if(enemies.Count == 0 && !isSpawningEnemies)
        {
            beatTimer.LoadLevelFlag();
        }

        HandleMerging();
        HandleSpotlightMerging();
        SwitchColor();
    }
    public void UpdateChasingTriangle()
    {
        Triangle highestPowerTriangle = null;

        foreach (var triangle in enemies)
        {
            if (highestPowerTriangle == null || triangle.powerLevel > highestPowerTriangle.powerLevel)
            {
                if (highestPowerTriangle != null)
                    highestPowerTriangle.isChasingPlayer = false; // Stop the previous chaser

                highestPowerTriangle = triangle;
            }
        }

        if (highestPowerTriangle != null)
            highestPowerTriangle.isChasingPlayer = true;
    }


    public int avarage;
    public void PlayHand()
    {   
        //avarage = 200;
        print("avarage: " + avarage);
        if(avarage<100)
        {
            audioSources[1].Stop();
            crowdController.LessNodders(20);
        }
        if(avarage<150)
        {
            audioSources[2].Stop();
        }
        if(avarage>=100)
        {
            audioSources[1].volume = 0.3f;
            audioSources[1].Play();
            crowdController.MoreNodders(10);
        }
        if(avarage>=150)
        {
            audioSources[2].volume = 0.3f;
            audioSources[2].Play();
            crowdController.MoreNodders(20);
        }
        
        if(levelNo>=30)
        {
            audioSources[5].volume = 0.3f;
            audioSources[5].Play();
        }
        else if(levelNo>=20)
        {
            audioSources[4].volume = 0.3f;
            audioSources[4].Play();
        }
        else if(levelNo>=10)
        {
            audioSources[3].volume = 0.3f;
            audioSources[3].Play();
        } 
    }

    public void PlayBack()
    {
        audioSources[0].Play();
    }

    private bool flag5=true;
    public void LoadLevel()
    {
        if(((levelNo+1)%5)==0 && flag5)
        {   
            levelNo++;
            flag5 = false;
            StartCoroutine(Load5());
            flag5 = true;
        }

        else if (flag5)
        {
            levelNo++;
        }

        totalTrianglesToSpawn = levelNo;
        trianglesSpawned = 0;
        isSpawningEnemies = true;
    }

    private IEnumerator Load5()
    {
        canSpawn=false;
        OpenLevelText();
        StopMusic();
        yield return new WaitForSeconds(waitSeconds/3);
        ChangePitch();
        CloseLevelText();
        StartMusic();
        yield return new WaitForSeconds(waitSeconds);
        canSpawn=true;
    }

    private void StartMusic()
    {
        beatTimer.beatCounter=0;
        beatTimer.play=true;
    }

    private void StopMusic()
    {
        for (int i = 0; i < 6; i++)
        {
            audioSources[i].Stop();
        }
    }

    private void ChangePitch()
    {
        if(levelNo == 5)
        {
            beatTimer.beatInterval = 0.571f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 0.875f;
            }
        }
        else if(levelNo == 10)
        {
            beatTimer.beatInterval = 0.545f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 0.9167f;
            }
        }
        else if(levelNo == 15)
        {
            beatTimer.beatInterval = 0.5217f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 0.9583f;
            }
        }
        else if(levelNo == 20)
        {
            beatTimer.beatInterval = 0.5f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 1f;
            }
        }
        else if(levelNo == 25)
        {
            beatTimer.beatInterval = 0.48f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 1.0417f;
            }
        }
        else if(levelNo == 30)
        {
            beatTimer.beatInterval = 0.4615f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 1.0833f;
            }
        }
        else if(levelNo == 35 )
        {
            beatTimer.beatInterval = 0.4444f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 1.125f;
            }
        }
        else
        {
            beatTimer.beatInterval = 0.4286f;
            for (int i = 0; i < 6; i++)
            {
                audioSources[i].pitch = 1.1667f;
            }
        }
    }

    [SerializeField]private GameController levelShower;
    [SerializeField]private Text levelText;
    private void OpenLevelText()
    {
        levelText.text = "Level " + levelNo;
        levelText.gameObject.SetActive(true);
    }
    private void CloseLevelText()
    {
        levelText.gameObject.SetActive(false);
    }

    void SwitchColor()
    {
        for(int x = 0; x<width-1; x++)
        {
            for (int y = 0; y<height-1; y++)
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

        // Group triangles by their current and previous positions
        foreach (var enemy in enemies)
        {
            if (!mergeGroups.ContainsKey(enemy.position))
            {
                mergeGroups[enemy.position] = new List<Triangle>();
            }
            mergeGroups[enemy.position].Add(enemy);

            if (enemy.previousPosition != enemy.position) // If the triangle moved, consider its previous position too
            {
                if (!mergeGroups.ContainsKey(enemy.previousPosition))
                {
                    mergeGroups[enemy.previousPosition] = new List<Triangle>();
                }
                mergeGroups[enemy.previousPosition].Add(enemy);
            }
        }

        // Merge triangles that have crossed paths or are on the same tile with the same power level and move count greater than 0
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

        // Group spotlights by their current and previous positions
        foreach (var spotlight in spotlights)
        {
            if (!mergeGroups.ContainsKey(spotlight.position))
            {
                mergeGroups[spotlight.position] = new List<SpotlightSquare>();
            }
            mergeGroups[spotlight.position].Add(spotlight);

            if (spotlight.previousPosition != spotlight.position) // If the spotlight moved, consider its previous position too
            {
                if (!mergeGroups.ContainsKey(spotlight.previousPosition))
                {
                    mergeGroups[spotlight.previousPosition] = new List<SpotlightSquare>();
                }
                mergeGroups[spotlight.previousPosition].Add(spotlight);
            }
        }

        // Merge spotlights that have crossed paths or are on the same tile with the same power level and move count greater than 0
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
                UpdateChasingTriangle();
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
        float centerX = (width * tileSize - tileSize) / 2.0f;
        float centerY = (height * tileSize - tileSize) / 2.0f;
        Camera.main.transform.position = new Vector3(centerX, centerY, Camera.main.transform.position.z);
    }

    void InitializeGrid()
    {
        GameObject parent = Instantiate(tilePrefab, new Vector2(20,20), Quaternion.identity);
        parent.transform.localScale = new Vector2(tileSize*width,tileSize*height);
        parent.GetComponent<SpriteRenderer>().sortingOrder = -20;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                /*if(y == 0 )
                {
                    grid[x, y] = Instantiate(tilePrefab, new Vector2(x * tileSize, y * tileSize+(tileSize/4)), Quaternion.identity);
                    grid[x, y].transform.localScale = new Vector3(tileSize, tileSize/2, 1);
                    grid[x, y].name = $"Tile_{x}_{y}";
                    if ((x + y) % 2 == 1) grid[x, y].GetComponent<SpriteRenderer>().color = Color.cyan;
                    grid[x,y].transform.parent = parent.transform;
                }*///Used to make the small tiles of the player for the 1D mode. Probably not necessary.

                grid[x, y] = Instantiate(tilePrefab, new Vector2(x * tileSize, y * tileSize), Quaternion.identity);
                grid[x, y].transform.localScale = new Vector3(tileSize, tileSize, 1);
                grid[x, y].name = $"Tile_{x}_{y}";
                if ((x + y) % 2 == 1) grid[x, y].GetComponent<SpriteRenderer>().color = Color.cyan;
                grid[x,y].transform.parent = parent.transform;
                
            }
        }

        /*for(int i = 0; i<46; i++)
        {
            health[i] = Instantiate(healthPrefab, new Vector2(-6, -2.5f+(i*1)), Quaternion.identity);
            health[i].transform.Rotate(0,0,90);
            health[i].transform.localScale = new Vector3(1.2f,1.2f,1);
        }*///older version of the helath bar.
    }

    void StartGame()
    {
        ChangeState(GameState.Play);
        totalTrianglesToSpawn = levelNo;
        trianglesSpawned = 0;
        //(1);
        SpawnInitialEnemies(); // Ensure enemies are spawned when the game starts
        UpdateChasingTriangle();
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

            UpdateChasingTriangle();
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
            FilterColor();
        }
    }

    private void FilterColor()
    {
        change += Time.deltaTime;
        float val = (change%360)/360;
        Color color = Color.HSVToRGB(val,0.75f,1); 
        color.a = 0.8f;
        filterSR.color = color;
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

    private void SpawnSpotlight_Heart(Vector2Int position, int powerLevel)
    {
        // Generate a random value between 0 and 1
        float randomValue = UnityEngine.Random.Range(0f, 1f);

        // Check if the random value is less than the spawn chance
        if (randomValue <= spawnChance)
        {
            GameObject spotlightObject = Instantiate(spotLightPrefab, new Vector2(position.x * tileSize, position.y * tileSize), Quaternion.identity);
            SpotlightSquare spotlight = spotlightObject.GetComponent<SpotlightSquare>();
            spotlight.Initialize(position, this, beatTimer, powerLevel);
            addSpotlight(spotlight);
        }

        if (randomValue >= 0.95f)
        {
            GameObject heartObject = Instantiate(heartPrefab, new Vector2(position.x * tileSize, position.y * tileSize), Quaternion.identity);
            hearts.Add(heartObject);
        }
    }

    public void CheckPlayerHeartCollision()
    {
        foreach (var heart in hearts)
        {
            Vector2Int heartPosition = Vector2Int.RoundToInt(new Vector2(heart.transform.position.x / tileSize, heart.transform.position.y / tileSize));

            if (player.position == heartPosition)
            {
                // Player is on the same tile as the heart
                CollectHeart(heart);
                break;
            }
        }
    }

    // Function to handle collecting the heart
    private void CollectHeart(GameObject heart)
    {
        // Add logic here to increase the player's health or score
        Debug.Log("Heart collected!");

        player.TakeDamage(-10);
        // Remove the heart from the list and destroy the heart object
        hearts.Remove(heart);
        Destroy(heart);
    }


    public void addSpotlight(SpotlightSquare spotlight)
    {
        spotlights.Add(spotlight);
    }

    Vector2Int GetOutsideSpawnPosition(HashSet<Vector2Int> occupiedPositions)
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        Vector2Int playerPos = player.position;
        //List<Vector2Int> excludedPositions = GetExcludedPositions(playerPos);

        // Generate positions just outside the grid on all sides, excluding those closest to the player
        for (int y = 0; y < height; y++)
        {
            Vector2Int abovePosition = new Vector2Int(y, width); // Above the top row

            if (!occupiedPositions.Contains(abovePosition))
                possiblePositions.Add(abovePosition);

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
            SpawnSpotlight_Heart(enemy.position, enemy.powerLevel);
            enemies.Remove(enemy);
            UpdateChasingTriangle();
        }
    }

    public void RemoveSpotlight(SpotlightSquare spotlight)
    {
        if (spotlights.Contains(spotlight))
        {
            spotlights.Remove(spotlight);
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

    public void Retry()
    {
        SceneManager.LoadScene("Game");
    }

    
    public void OpenEndScreen()
    {
        endScreen.SetActive(true);
    }

}
