using UnityEngine;
using System;
using UnityEngine.Audio;
using Common.Enums;

public class BeatTimer : MonoBehaviour
{
    [Range(0,1)]
    [SerializeField] private float timeS; // To control time scale for debugging
    [SerializeField] public float beatInterval = 0.6f; // Time between beats in seconds
    private GameController gameController;
    public event Action OnBeat;
    private SpriteRenderer backGround;
    private double timer;
    private float tolerance;
    public BeatState State { get; set; }
    public bool play;
    public int beatCounter;

    public AudioSource backgroundAudio;

    void Awake()
    {
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        gameController = GetComponent<GameController>();
        tolerance = beatInterval/10;
    }

    void Start()
    {
        Invoke("StartAfterDelay", beatInterval);
        gameController.PlayBackground();
        timer = 0;
    }

    private void StartAfterDelay()
    {
        beatCounter = -1;
        play = true;
        State = BeatState.OffBeat;
    }

    void Update()
    {
        timer = (double)AudioSettings.dspTime; // Use dspTime for accurate timing
    }

    void FixedUpdate()
    {
        if (timeS != 1) Time.timeScale = 1 * timeS;
        CheckAction();
    }

    private void PlayBack()
    {   
        if (gameController) gameController.PlayBack();
    }

    void PlayHand()
    {
        if (gameController) gameController.PlayHand();
        gameController.avarage = 0;
    }

    private bool beatFlag = true;

    void CheckAction()
    {
        double modTimer = (double)(float)Math.Round(timer % beatInterval, 2);
        UpdateBeatState(modTimer);

        if (modTimer <= tolerance && beatFlag)
        {
            OnBeat?.Invoke();
            beatCounter++;
            beatFlag = false; // Ensure the beat is only triggered once per interval
        }

    }

    private void UpdateBeatState(double modTimer)
    {
        // Adjust beat state color based on proximity to the beat
        if (modTimer <= tolerance || (modTimer >= (beatInterval - tolerance)))
        {
            backGround.color = Color.red;
            State = BeatState.PerfectBeat;
        }
        else if (modTimer <= 2 * tolerance || (modTimer >= (beatInterval - 2 * tolerance)))
        {
            backGround.color = Color.blue;
            State = BeatState.CloseBeat;
        }
        else if (modTimer <= 3 * tolerance || (modTimer >= (beatInterval - 3 * tolerance)))
        {
            backGround.color = Color.green;
            State = BeatState.MiddleBeat;
        }
        else if (modTimer <= 4 * tolerance || (modTimer >= (beatInterval - 4 * tolerance)))
        {
            if(State == BeatState.OffBeat)
            {
                if(beatCounter%16==0)
                {
                    if(play)
                    {
                        PlayBack();
                        play=false;
                    }
                    PlayHand();
                }
            }
            backGround.color = Color.yellow;
            State = BeatState.FarBeat;
        }
        else
        {
            backGround.color = Color.black;
            State = BeatState.OffBeat;
            if(!beatFlag)beatFlag=true;
        }
    }
}
