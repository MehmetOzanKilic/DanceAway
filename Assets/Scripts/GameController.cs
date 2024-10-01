using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [HideInInspector]public BeatState State;
    [SerializeField]private int levelNo=1;
    [SerializeField]private float spotlightSpawnChance = 0.4f;
    [SerializeField]private float heartSpawnChance = 0.05f;    
    [SerializeField]private CrowdController crowdController;
    [SerializeField]public int width;
    [SerializeField]public int height;
    [SerializeField]public List<int> gridBounds = new List<int>();// width lower(0)/upper(1), height lower(2)/upper(3) 
    public float tileSize = 8.0f;
    [SerializeField]private int levelLoad5Wait=1;
    [SerializeField]private AudioSource[] audioSources;  
    [SerializeField]private BeatTimer beatTimer;
    private GameState currentState;
    private GameObject[,] grid;
    [SerializeField]private GameObject tilePrefab; // Reference to the tile prefab
    [SerializeField]private GameObject trianglePrefab; // Reference to the triangle prefab
    [SerializeField]private GameObject spotLightPrefab;
    [SerializeField]private GameObject heartPrefab;
    [SerializeField]private GameObject endScreen;
    [SerializeField]private GameObject filter;
    private SpriteRenderer filterSR;
    public Player player;
    [HideInInspector]public List<Triangle> enemies = new List<Triangle>();
    [HideInInspector]public List<SpotlightSquare> spotlights = new List<SpotlightSquare>();
    private List<GameObject> hearts = new List<GameObject>();
    private int totalTrianglesToSpawn;
    private int trianglesSpawned;
    private int beatCounter = 0;
    private bool isSpawningEnemies = false; // Flag to track enemy spawning

    void Start()
    {
        // Initializing the arena grid and Centering the camera
        grid = new GameObject[width, height];
        gridBounds.Add(0);
        gridBounds.Add(width);
        gridBounds.Add(0);
        gridBounds.Add(height);
        CenterCamera();
        InitializeGrid();
        player.StartPlayer();
        // Setting initial audio pitches
        audioSources = GetComponents<AudioSource>();
        for(int i = 0; i < 6; i++)
        {
            audioSources[i].Stop();
            audioSources[i].pitch = 0.8333f;
        }

        levelText.gameObject.SetActive(false);
        filterSR = filter.GetComponent<SpriteRenderer>();
        colorChange = UnityEngine.Random.Range(0, 360);
        endScreen.SetActive(false);
        crowdController.Initialize(beatTimer,width,height,tileSize);

        beatTimer.OnBeat += HandleBeat;

        ChangeGridBounds();

        getCrowdParents();

        resizeCrowd();

        StartGame();
    }
    private bool canSpawn = true;
    void HandleBeat()// Handles all the checks happening once per beat
    {

        beatCounter++;

        StartCoroutine(HandleBeatCoroutine());
        
        foreach (var spotlight in spotlights)
        {
            if(beatCounter%2==0)spotlight.Move();// Spotlights move once per 2 beats since their speed is halved
        }

        if (isSpawningEnemies)
        {
            if(canSpawn)SpawnRemainingEnemies();
        }

        if(enemies.Count == 0 && !isSpawningEnemies)
        {
            LoadLevel();// Load level if only there are no enemies present and none will be spawned
            ChangeGridBounds();
        }

        HandleMerging();
        HandleSpotlightMerging();
        SwitchColor();
    }

    IEnumerator HandleBeatCoroutine()
    {
        List<Triangle> trianglesToRemove = new List<Triangle>();

        // Iterate over a copy of the list to avoid modifying the collection during iteration
        var enemiesCopy = new List<Triangle>(enemies);

        foreach (var enemy in enemiesCopy)
        {
            if (enemy != null)
            {
                enemy.Move(); // Make enemies move with the beat

                // Check if this enemy should be removed (health check or any other condition)
                if (enemy.health <= 0)
                {
                    trianglesToRemove.Add(enemy);
                }
            }
            yield return null;
        }

        // Now safely remove the destroyed triangles
        foreach (var enemy in trianglesToRemove)
        {
            RemoveEnemy(enemy); // Your existing method to remove enemies
        }

        // After all enemies have moved, handle grid bounds change if needed
        if (gridBoundsFlag)
        {
            ChangeGridBounds();
            gridBoundsFlag = false;
        }
    }


    // Updates the triangle that is chaing the player according to the powe level of the triangle
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

    public int avarage;// To keep track of how good the player is doing
    public void PlayHand()
    {   
        // Plays different hands(beats) according to both the avarage of the player and the current level no.
        print("avarage: " + avarage);
        if(avarage<100)
        {
            audioSources[1].Stop();
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

    public void LessNodders(int no)
    {
        crowdController.LessNodders(no);
    }

    private bool flag5=true;
    public void LoadLevel()
    {
        levelNo++;
        if((levelNo%5)==0 && flag5)
        {   
            flag5 = false;
            StartCoroutine(Load5());
            flag5 = true;
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
        yield return new WaitForSeconds(levelLoad5Wait);
        ChangePitch();
        CloseLevelText();
        StartMusic();
        yield return new WaitForSeconds(levelLoad5Wait/2);
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
        // Different beat intervals and pitches are set according to the level no.
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

    void SwitchColor()// To chage the arena grid's color randomly each beat
    {
        for(int x = 0; x<width; x++)
        {
            for (int y = 0; y<height; y++)
            {
                grid[x,y].GetComponent<SpriteRenderer>().color =  GetRandomColor();
            }
        }
    }

    private Color GetRandomColor()
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        float g = UnityEngine.Random.Range(0f, 1f);
        float b = UnityEngine.Random.Range(0f, 1f);
        return new Color(r, g, b);
    }

    void HandleMerging()// Maybe change it so that it works with colliders instead?????????
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


    void HandleSpotlightMerging()// Maybe change it so that it works with colliders instead?????????
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
        //Centers the camera to have same positioning ratios in different devices
        float centerX = (width * tileSize - tileSize) / 2.0f;

        float yScreenPosition = Screen.height * 0.35f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, yScreenPosition, Camera.main.nearClipPlane));

        Camera.main.transform.position = new Vector3(centerX, worldPosition.y, Camera.main.transform.position.z);
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float arenaWidth = width*height;
        Camera.main.orthographicSize = arenaWidth / (1.2f * screenAspect);
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
                grid[x, y] = Instantiate(tilePrefab, new Vector2(x * tileSize, y * tileSize), Quaternion.identity);
                grid[x, y].transform.localScale = new Vector3(tileSize, tileSize, 1);
                grid[x, y].name = $"Tile_{x}_{y}";
                if ((x + y) % 2 == 1) grid[x, y].GetComponent<SpriteRenderer>().color = Color.cyan;
                grid[x,y].transform.parent = parent.transform;
            }
        }
    }

    void ResizeGrid(int newWidth, int newHeight)
    {
        // Calculate the grid center based on the original width and height
        float centerX = (width * tileSize) / 2.0f;
        float centerY = (height * tileSize) / 2.0f;

        // Calculate the boundaries of the new grid dimensions
        float newCenterX = (newWidth * tileSize) / 2.0f;
        float newCenterY = (newHeight * tileSize) / 2.0f;

        // Iterate through the grid to activate/deactivate tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate the current tile's position relative to the center
                float tileXPos = (x * tileSize) + tileSize / 2.0f;
                float tileYPos = (y * tileSize) + tileSize / 2.0f;

                // Determine if this tile is within the new width and height bounds
                bool isWithinBoundsX = Mathf.Abs(tileXPos - centerX) <= newCenterX;
                bool isWithinBoundsY = Mathf.Abs(tileYPos - centerY) <= newCenterY;

                if (isWithinBoundsX && isWithinBoundsY)
                {
                    // Activate the tile
                    grid[x, y].SetActive(true);
                }
                else
                {
                    // Deactivate the tile
                    grid[x, y].SetActive(false);
                }
            }
        }

        width = newWidth;
        height = newHeight;
    }

    private int maxX;
    private int maxY;
    private int minX;
    private int minY;

    private void ChangeGridBounds()
    {
        print("hwy");
        if (gridBounds.Count < 4)
        {
            Debug.LogError("GridBounds list does not contain enough elements!");
            return;
        }

        if(enemies.Count==0)
        {
            gridBounds[0] = 0;
            gridBounds[1] = 7; 
            gridBounds[2] = 0; 
            gridBounds[3] = 7;
        }

        else
        {
            maxX=Math.Max(enemies.Max(enemy => enemy.position.x),player.position.x);
            maxY=Math.Max(enemies.Max(enemy => enemy.position.y),player.position.y);
            minX=Math.Min(enemies.Min(enemy => enemy.position.x),player.position.x);
            minY=Math.Min(enemies.Min(enemy => enemy.position.y),player.position.y);

            if(maxX<width-2 && minX>1 && maxY<height-2 && minY>1)
            {
                //??????? Her şeyin 3 e 3 den büyük olduğuna emin olmak lazım
                gridBounds[0] = 2;
                gridBounds[1] = 5; 
                gridBounds[2] = 2; 
                gridBounds[3] = 5;
            }
            else if(maxX<width-1 && minX>0 && maxY<height-1 && minY>0)
            {
                //??????? Her şeyin 3 e 3 den büyük olduğuna emin olmak lazım
                gridBounds[0] = 1;
                gridBounds[1] = 6; 
                gridBounds[2] = 1; 
                gridBounds[3] = 6;
            }
        }

    }

    void StartGame()
    {
        ChangeState(GameState.Play);
        totalTrianglesToSpawn = levelNo;
        trianglesSpawned = 0;
        isSpawningEnemies = true;
        SpawnRemainingEnemies(); // Ensure enemies are spawned when the game starts
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
        Time.timeScale = 0;
    }

    void ResumeGame()
    {
        ChangeState(GameState.Play);
        Time.timeScale = 1;
    }

    void EndGame()
    {
        ChangeState(GameState.End);
    }

    [SerializeField]private bool resizeFlag=false;
    [SerializeField]private bool gridBoundsFlag=false;
    void Update()
    {
        if (currentState == GameState.Play)
        {
            player.HandleInput();
            FilterColor();
        }
        if(resizeFlag)
        {    
            ResizeGrid(5,5);
            resizeFlag=false;
        }
    }

    private float colorChange;
    private void FilterColor()
    {
        colorChange += Time.deltaTime;
        float val = (colorChange%360)/360;
        Color color = Color.HSVToRGB(val,0.75f,1); 
        color.a = 0.8f;
        filterSR.color = color;
    }

    private int nameCounter = 0;
    void SpawnEnemies(int spawnCount)
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

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
        if (randomValue <= spotlightSpawnChance)
        {
            GameObject spotlightObject = Instantiate(spotLightPrefab, new Vector2(position.x * tileSize, position.y * tileSize), Quaternion.identity);
            SpotlightSquare spotlight = spotlightObject.GetComponent<SpotlightSquare>();
            spotlight.Initialize(position, this, beatTimer, powerLevel);
            addSpotlight(spotlight);
        }

        if (randomValue >= 1-heartSpawnChance)
        {
            GameObject heartObject = Instantiate(heartPrefab, new Vector2(position.x * tileSize, position.y * tileSize), Quaternion.identity);
            hearts.Add(heartObject);
        }
    }

    // Function to handle collecting the heart
    public void CollectHeart(GameObject heart)
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

        // Generate positions outside the grid, excluding the bottom side
        for (int y = 1; y < gridBounds[3]; y++)
        {
            if (playerPos.x != gridBounds[0])
            {
                Vector2Int leftPosition = new Vector2Int(gridBounds[0]-1, y); // Left side
                if (!occupiedPositions.Contains(leftPosition))
                    possiblePositions.Add(leftPosition);
            }

            if (playerPos.x != gridBounds[1] - 1)
            {
                Vector2Int rightPosition = new Vector2Int(gridBounds[1], y); // Right side
                if (!occupiedPositions.Contains(rightPosition))
                    possiblePositions.Add(rightPosition);
            }
        }

        for (int x = 0; x < gridBounds[1]; x++)
        {
            if (playerPos.y != gridBounds[3] - 1)
            {
                Vector2Int topPosition = new Vector2Int(x, gridBounds[3]); // Top side
                if (!occupiedPositions.Contains(topPosition))
                    possiblePositions.Add(topPosition);
            }
        }

        if (possiblePositions.Count == 0)
        {
            return Vector2Int.zero; // No valid position found
        }

        int randIndex = UnityEngine.Random.Range(0, possiblePositions.Count);
        return possiblePositions[randIndex];
    }

    public void RemoveEnemy(Triangle enemy)
    {
        if (enemies.Contains(enemy))
        {
            SpawnSpotlight_Heart(enemy.position, enemy.powerLevel);
            enemies.Remove(enemy);
            UpdateChasingTriangle();
            gridBoundsFlag=true;
        }
    }

    public void RemoveSpotlight(SpotlightSquare spotlight)
    {
        if (spotlights.Contains(spotlight))
        {
            spotlights.Remove(spotlight);
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene("Game");
    }

    
    public void OpenEndScreen()
    {
        endScreen.SetActive(true);
    }

    public List<int> ReturnGridbounds()
    {
        List<int> tempBounds = new List<int>();
        print("herehere");
        for(int i=0; i<4; i++)
        {
            print("herein");
            tempBounds.Add(gridBounds[i]);
        }
        return tempBounds;
    }

    private GameObject leftCrowd;
    private GameObject rightCrowd;
    private Vector2 leftCrowdPos;
    private Vector2 rightCrowdPos;
    public void getCrowdParents()
    {
        leftCrowd = crowdController.crowdParentLeft;
        rightCrowd = crowdController.crowdParentRight;

        leftCrowdPos = leftCrowd.transform.position;
        rightCrowdPos = rightCrowd.transform.position;
    }

    private void resizeCrowd()
    {
        if(gridBounds[1] == width)
        {
            leftCrowd.transform.position = leftCrowdPos;
            rightCrowd.transform.position = rightCrowdPos;
        }
        else
        {
            leftCrowd.transform.position = leftCrowd.transform.position + new Vector3(tileSize*gridBounds[0], 0,0);
            print(leftCrowd.transform.position);
            rightCrowd.transform.position = rightCrowd.transform.position - new Vector3(tileSize*(width-gridBounds[1]), 0,0);
            print(rightCrowd.transform.position);
        }
    }

}
