using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Threading;
using Unity.Mathematics;

public class BeatTimer : MonoBehaviour
{   
    public AudioMixer audioMixer;
    private GameController gameController;
    AudioSource audioSource;
    public float beatInterval = 0.6f; // Time between beats in seconds
    public float nextBeatTime;

    public event Action startBeat;
    public event Action farBeat;
    public event Action middleBeat;
    public event Action closeBeat;
    public event Action OnBeat;
    public event Action endBeat;
    public event Action CanMove;

    public SpriteRenderer backGround;

    public float timer;
    public float tolerance=0.04f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        Time.timeScale = 0.5f;
        audioSource = GetComponent<AudioSource>();
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        nextBeatTime = Time.time + beatInterval;
        gameController = GetComponent<GameController>();    
        //audioMixer.SetFloat("PitchShift", 1f);
        timer=beatInterval/2;
        Invoke("FlagFalse", 0);

        //audioMixer.SetFloat("PitchShift",0.8333f);
    }
    void Start()
    {   
        
    }

    private bool flag=true;
    private void FlagFalse()
    {
        flag = false;
    }


    private bool canMoveflag=false;
    private bool auidoFlag=true;
    public bool canPlay=true;
    public int beatCounter=0;
    public float beatOffset = 0.1f; // Adjust this value to control the beat offset (in seconds)
    private bool play=true;
    private float modTimer;
    void FixedUpdate()
    {   
        
        CheckAction();
        timer += Time.deltaTime;
        modTimer = (float)Math.Round(timer % beatInterval,2);




        

        /*timer += Time.deltaTime;
        //UpdateBackgroundColor(nextBeatTime);   

        if (timer >= nextBeatTime + beatDivider + beatOffset)
        {   
            nextBeatTime += beatInterval;
            flag = false;
            canMoveflag = true;   
        }

        if (timer >= nextBeatTime + beatOffset && flag == false)
        {
            
            flag = true;
            OnBeat?.Invoke();
            
            
            if (beatCounter == 0 && canPlay)
            {
                Invoke("PlayBack", 0);
                timer=0;
                //Invoke("PlayHand", 0);
                beatCounter++;
            }
            else if (beatCounter % 16 == 0 && canPlay)
            {
                //Invoke("PlayHand", 0f);
                beatCounter++;
            }
            beatCounter++;

            
            if (auidoFlag)
            {
                //Invoke("PlayAudio", 0.2f);
                auidoFlag = false;
            }
            
        }*/
    }

    private void PlayBack()
    {
        if(gameController)gameController.PlayBack();
    }
    /*void PlayHand()
    {
        print("playHand");
        if(gameController)gameController.PlayHand();
    }*/
    
    [SerializeField]private float beatOfset=0;
    private float previousTime;
    void CheckAction()
    {

        if(!play)
        {
            float modTimer = (float)Math.Round(timer % beatInterval, 2); // Calculated every frame

            // Check closest to the beat
            if ((modTimer <= tolerance) || (modTimer >= (beatInterval - tolerance)))
            {
                backGround.color = Color.red; // Very close to the beat
            }
            // Second closest to the beat
            else if ((modTimer <= 2 * tolerance) || (modTimer >= (beatInterval - 2 * tolerance)))
            {
                backGround.color = Color.blue; // Second level of closeness
            }
            // Third closest to the beat
            else if ((modTimer <= 3 * tolerance) || (modTimer >= (beatInterval - 3 * tolerance)))
            {
                backGround.color = Color.black; // Third level of closeness
            }
            // Fourth closest to the beat
            else if ((modTimer <= 4 * tolerance) || (modTimer >= (beatInterval - 4 * tolerance)))
            {
                backGround.color = Color.green; // Farthest in the close range
            }
            else
            {
                backGround.color = Color.white; // Far from the beat
            }
        }
        

        // Check for the beat action
        if (modTimer<=0.02f && modTimer>=0)
        {
            if (play)
            {
                PlayBack();
                play = false;
                print("here");
                
            }
            OnBeat?.Invoke();
            print("beat: " + timer);
        }


        /*else if(modTimer<=0.08f)
        {
            print("modBeat" + modTimer);
            backGround.color = Color.black;
        }
        else if(modTimer<=0.08f+beatOffset || modTimer>= 0.52f+beatOffset)
        {
            backGround.color = Color.red;
        }
        else if(modTimer<=0.12f+beatOffset || modTimer>= 0.48f+beatOffset)
        {
            backGround.color = Color.yellow;
        }
        else if(modTimer<=0.16f+beatOffset || modTimer>= 0.44f+beatOffset)
        {
            backGround.color = Color.yellow;
        }
        else backGround.color = Color.white;*/
        
        
        
    }
}
