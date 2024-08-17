using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]private GameObject filter;
    private SpriteRenderer filterSR;
    public int waitSeconds=1;
    AudioSource[] audioSources;  
    public BeatTimer beatTimer;
    public GameState currentState;
    public GameObject[,] grid = new GameObject[6, 6];
    public GameObject tilePrefab; // Reference to the tile prefab
    public float tileSize = 8.0f;
    private int beatCounter = 0;
    private float change;
    void Start()
    {
        CenterCamera();
        InitializeGrid();
        StartGame();
        audioSources = GetComponents<AudioSource>();
        for(int i = 0; i < 6; i++)
        {
            audioSources[i].Stop();
            audioSources[i].pitch = 0.8333f;
        }
        filterSR = filter.GetComponent<SpriteRenderer>();
        change = Random.Range(0, 360);

        

        beatTimer.OnBeat += HandleBeat;
    }

    private bool canSpawn = true;
    void HandleBeat()
    {
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

        FilterColor();
    }

    private void FilterColor()
    {
        change += Time.deltaTime;
        float val = (change%360)/360;
        Color color = Color.HSVToRGB(val,0.75f,1); 
        color.a = 0.8f;
        filterSR.color = color;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    [SerializeField]private GameObject howToPlay;
    public void OpenHow()
    {
        howToPlay.SetActive(true);
    }

    public void CloseHow()
    {
        howToPlay.SetActive(false);
    }
}
