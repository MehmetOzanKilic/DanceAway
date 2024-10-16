using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using static SwipeController;

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
    [SerializeField]public int levelNo=1;
    [SerializeField]private float spotlightSpawnChance = 0.4f;
    [SerializeField]private float heartSpawnChance = 0.05f;    
    [SerializeField]private CrowdController crowdController;
    [SerializeField]public int width;
    [SerializeField]public int height;
    [SerializeField]public static List<int> gridBounds = new List<int>();// width lower(0)/upper(1), height lower(2)/upper(3) 
    public float tileSize = 8.0f;
    [SerializeField]public static AudioSource[] audioSources;  
    public static BeatTimer beatTimer;
    private GameState currentState;
    public static GameObject[,] grid;
    [SerializeField]private GameObject spotLightPrefab;
    [SerializeField]private GameObject heartPrefab;
    [SerializeField]private GameObject endScreen;
    [SerializeField]private GameObject filter;
    private SpriteRenderer filterSR;
    public Player player;
    [HideInInspector]public List<Triangle> enemies = new List<Triangle>();
    [HideInInspector]public List<SpotlightSquare> spotlights = new List<SpotlightSquare>();
    private List<GameObject> hearts = new List<GameObject>();
    public int totalTrianglesToSpawn;
    public int trianglesSpawned;
    private int beatCounter = 0;
    public bool isSpawningEnemies = false; // Flag to track enemy spawning
    private LevelManager levelManager;
    private EnemySpawner enemySpawner;
    private GridController gridController;
    private SwipeController swipeController;

    void Start()
    {
        // Initializing the arena grid and Centering the camera
        grid = new GameObject[width, height];
        beatTimer = GetComponent<BeatTimer>();
        audioSources = GetComponents<AudioSource>();
        levelManager = GetComponent<LevelManager>();
        levelManager.Initialize();
        enemySpawner = GetComponent<EnemySpawner>();
        enemySpawner.Initialize();  
        gridController = GetComponent<GridController>();
        gridController.Initialize();
        crowdController.Initialize(beatTimer,width,height,tileSize);
        swipeController = GameObject.Find("SwipeController").GetComponent<SwipeController>();

        CenterCamera();

        gridController.InitializeGrid();

        // Setting initial audio pitches
        for(int i = 0; i < 6; i++)
        {
            audioSources[i].Stop();
            audioSources[i].pitch = 0.8333f;
        }

        filterSR = filter.GetComponent<SpriteRenderer>();
        colorChange = UnityEngine.Random.Range(0, 360);
        endScreen.SetActive(false);
        

        beatTimer.OnBeat += HandleBeat;

        player.StartPlayer();

        gridController.ResetGridBounds();

        crowdController.GetCrowdParents();

        crowdController.ResizeCrowd();

        StartGame();
    }
    public bool canSpawn = true;
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
            if(canSpawn)enemySpawner.SpawnRemainingEnemies();
        }

        if(enemies.Count == 0 && !isSpawningEnemies)
        {
            levelManager.LoadLevel();// Load level if only there are no enemies present and none will be spawned
            gridController.ResetGridBounds();
            crowdController.ResizeCrowd();
        }

        HandleMerging();
        HandleSpotlightMerging();
        SwitchColor();
    }

    public int enemiesKilled = 0;
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
        if (gridBoundsFlag && enemiesKilled >= 5 && enemies.Count > 1)
        {
            gridController.ChangeGridBounds();
            crowdController.ResizeCrowd();
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

    public int avarage=0;// To keep track of how good the player is doing
    public void PlayHand()
    {   
        avarage=avarage/16;
        //print(avarage);
        // Plays different hands(beats) according to both the avarage of the player and the current level no.
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
            crowdController.MoreNodders(20);
        }
        if(avarage>=150)
        {
            audioSources[2].volume = 0.3f;
            audioSources[2].Play();
            crowdController.MoreNodders(50);
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
        avarage=0;
    }

    public void PlayBack()
    {
        audioSources[0].Play();
    }

    public void PlayBackground()
    {
        beatTimer.backgroundAudio=audioSources[6];
        audioSources[6].volume=0.05f;
        audioSources[6].Play();
    }

    public void LessNodders(int no)
    {
        crowdController.LessNodders(no);
    }
    public void MoreNodders(int no)
    {
        crowdController.MoreNodders(no);
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

        int healthSum=0;//how much health triangle lost
        foreach (var triangle in mergeCandidates)
        {
            healthSum+=(triangle.baseHealth*triangle.powerLevel)-triangle.health;
            if (triangle != baseTriangle)
            {
                RemoveEnemy(triangle);
                UpdateChasingTriangle();
                //var temp = triangle.powerLevel;// does not work correctly if this comment is not here
                Destroy(triangle.gameObject);
            }
        }

        baseTriangle.MergeTriangles(numberOfTriangles,healthSum);
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

    private float screenAspect;
    private float arenaWidth;
    void CenterCamera()
    {
        //Centers the camera to have same positioning ratios in different devices
        float centerX = (width * tileSize - tileSize) / 2.0f;

        float yScreenPosition = Screen.height * 0.5f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, yScreenPosition, Camera.main.nearClipPlane));

        Camera.main.transform.position = new Vector3(centerX, worldPosition.y, Camera.main.transform.position.z);
        screenAspect = (float)Screen.width / (float)Screen.height;
        arenaWidth = width*height;
    }




    public float cameraTransitionDuration = 3f;
    public IEnumerator ChangeCameraOrthoSize(float f)
    {
        float timePassed = 0;
        float initialSize = Camera.main.orthographicSize;
        float targetSize = arenaWidth / (f*screenAspect);

        while (timePassed < cameraTransitionDuration)
        {
            Camera.main.orthographicSize = Mathf.Lerp(initialSize,targetSize,timePassed/cameraTransitionDuration);
            timePassed += Time.deltaTime;
            yield return null;
        }

        //Camera.main.orthographicSize = targetSize;
    }
    public IEnumerator ChangeCameraOrthoSize(int i)
    {
        float timePassed = 0;
        float initialSize = Camera.main.orthographicSize;
        float targetSize = i;

        while (timePassed < cameraTransitionDuration)
        {
            Camera.main.orthographicSize = Mathf.Lerp(initialSize,targetSize,timePassed/cameraTransitionDuration);
            timePassed += Time.deltaTime;
            yield return null;
        }

        //Camera.main.orthographicSize = targetSize;
    }

    void StartGame()
    {
        ChangeState(GameState.Play);
        totalTrianglesToSpawn = levelNo;
        trianglesSpawned = 0;
        isSpawningEnemies = true;
        enemySpawner.SpawnRemainingEnemies(); // Ensure enemies are spawned when the game starts
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
    [SerializeField]public bool gridBoundsFlag=false;
    void Update()
    {
        if (currentState == GameState.Play)
        {
            player.HandleInput();
            //FilterColor();
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
        player.TakeHeart(10);
        // Remove the heart from the list and destroy the heart object
        hearts.Remove(heart);
        Destroy(heart);
    }


    public void addSpotlight(SpotlightSquare spotlight)
    {
        spotlights.Add(spotlight);
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
        for(int i=0; i<4; i++)
        {
            tempBounds.Add(gridBounds[i]);
        }
        return tempBounds;
    }


    //keeps making the crowd smaller if it keeps going


    /*void ResizeGrid(int newWidth, int newHeight)
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
    }*/

}
