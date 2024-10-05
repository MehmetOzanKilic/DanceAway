using UnityEngine;
using System;
using UnityEngine.Audio;
using Common.Enums;

public class BeatTimer : MonoBehaviour
{   
    [Range(0,1)]
    [SerializeField]private float timeS; // To controle timescale for debugging
    [SerializeField]public float beatInterval = 0.6f; // Time between beats in seconds
    private GameController gameController;
    public event Action OnBeat;
    public SpriteRenderer backGround;
    public float timer;
    public float tolerance=0.04f;
    public BeatState State { get; set; }
    public bool play;
    public int beatCounter;

    void Awake()
    {
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        gameController = GetComponent<GameController>();    
        timer=0;
        State = BeatState.OffBeat;

    }
    void Start()
    {   
        Invoke("StartAfterDelay",beatInterval);
    }

    private void StartAfterDelay()
    {
        beatCounter=-1;
        play=true;
    }

    void FixedUpdate()
    {   
        if(timeS!=1)Time.timeScale=1*timeS;
        CheckAction();
        timer += Time.deltaTime;
    }

    private void PlayBack()
    {
        if(gameController)gameController.PlayBack();
    }
    void PlayHand()
    {
        if(gameController)gameController.PlayHand();
        gameController.avarage=0;
    }
    private bool beatFlag=true;
    void CheckAction()
    {
        float modTimer = (float)Math.Round(timer % beatInterval, 2); // Calculated every frame

        // Check closest to the beat
        if ((modTimer <= tolerance) || (modTimer >= (beatInterval - tolerance)))
        {
            backGround.color = Color.red;
            State = BeatState.PerfectBeat;
        }
        // Second closest to the beat
        else if ((modTimer <= 2 * tolerance) || (modTimer >= (beatInterval - 2 * tolerance)))
        {
            backGround.color = Color.blue;
            State = BeatState.CloseBeat;
        }
        // Third closest to the beat
        else if ((modTimer <= 3 * tolerance) || (modTimer >= (beatInterval - 3 * tolerance)))
        {
            backGround.color = Color.green;
            State = BeatState.MiddleBeat;
        }
        // Fourth closest to the beat
        else if ((modTimer <= 4 * tolerance) || (modTimer >= (beatInterval - 4 * tolerance)))
        {
            // PlayBack/Hand gets called at the start of the beat to make the music fit better.
            if(State == BeatState.OffBeat)
            {
                if(beatCounter%16==0)
                {
                    print("inside%16");
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
            backGround.color = Color.black; // Far from the beat
            State = BeatState.OffBeat;
            if(!beatFlag)beatFlag=true;
        }
        
        // Check for the beat action
        if (modTimer<=0.02f && modTimer>=0)
        {
            if(beatFlag)// to ensure beat happens only once per beatInterval 
            {
                OnBeat?.Invoke();
                beatCounter++;
                //print(beatCounter);
                beatFlag=false;
            }
        }       
        
    }

}
