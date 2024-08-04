// File: Scripts/Managers/GameController.cs

using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public BeatTimer beatTimer;
    public GameState currentState;
    public GameObject[,] grid = new GameObject[6, 6];
    public GameObject tilePrefab; // Reference to the tile prefab
    public float tileSize = 5.0f;
    public Player player;
    public List<Triangle> enemies;
    public List<SpotlightSquare> spotlights;
    public SpectatorCrowd crowd;
    public DJ dj;
    public LevelManager levelManager;
    public UIManager uiManager;
    public AudioManager audioManager;

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
                if((x+y)%2==1) grid[x,y].GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    void StartGame()
    {
        ChangeState(GameState.Play);
        LoadLevel(1);
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
