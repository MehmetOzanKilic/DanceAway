using UnityEngine;
using System;
using UnityEngine.Audio;
using Common.Enums;

public class BeatTimer : MonoBehaviour
{   
    [Range(0,1)]
    [SerializeField]private float timeS;
    private GameController gameController;
    public float beatInterval = 0.6f; // Time between beats in seconds
    public float nextBeatTime;
    public event Action OnBeat;
    public event Action CanMove;
    public SpriteRenderer backGround;
    public float timer;
    public float tolerance=0.04f;
    public BeatState State { get; set; }
    public bool play;
    private bool levelFlag=false;
    public int beatCounter;
    private bool handPlay;

    void Awake()
    {
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        nextBeatTime = Time.time + beatInterval;
        gameController = GetComponent<GameController>();    
        timer=beatInterval/2;
        State = BeatState.OffBeat;

    }
    void Start()
    {   
        beatCounter=0;
        play=true;
        handPlay=true;
    }

    void FixedUpdate()
    {   
        if(timeS!=1)Time.timeScale=1*timeS;
        CheckAction();
        timer += Time.deltaTime;
    }

    public void LoadLevelFlag()
    {
        levelFlag = true; 
    }

    private void PlayBack()
    {
        if(gameController)gameController.PlayBack();
    }
    void PlayHand()
    {
        print("playHand");
        if(gameController)gameController.PlayHand();
    }
    private bool beatFlag=true;
    void CheckAction()
    {
        float modTimer = (float)Math.Round(timer % beatInterval, 2); // Calculated every frame

        // Check closest to the beat
        if ((modTimer <= tolerance) || (modTimer >= (beatInterval - tolerance)))
        {
            backGround.color = Color.red; // Very close to the beat
            State = BeatState.PerfectBeat;
        }
        // Second closest to the beat
        else if ((modTimer <= 2 * tolerance) || (modTimer >= (beatInterval - 2 * tolerance)))
        {
            backGround.color = Color.blue; // Second level of closeness
            State = BeatState.CloseBeat;
        }
        // Third closest to the beat
        else if ((modTimer <= 3 * tolerance) || (modTimer >= (beatInterval - 3 * tolerance)))
        {
            backGround.color = Color.green; // Third level of closeness
            State = BeatState.MiddleBeat;
        }
        // Fourth closest to the beat
        else if ((modTimer <= 4 * tolerance) || (modTimer >= (beatInterval - 4 * tolerance)))
        {
            
            if(State == BeatState.OffBeat)
            {
                if(beatCounter%16==0)
                {
                    PlayHand();
                }
                if(play)
                {
                    PlayBack();
                    play=false;
                }
            }
            backGround.color = Color.yellow; // Farthest in the close range
            State = BeatState.FarBeat;
        }
        else
        {
            backGround.color = Color.white; // Far from the beat
            State = BeatState.OffBeat;
            if(!beatFlag)beatFlag=true;
        }
        
        // Check for the beat action
        if (modTimer<=0.02f && modTimer>=0)
        {
            if(levelFlag)
            {
                gameController.LoadLevel();
                levelFlag=false;
            }
            if(beatFlag) 
            {
                OnBeat?.Invoke();
                beatCounter++;
                //print(beatCounter);
                beatFlag=false;
            }
        }       
        
    }

}
