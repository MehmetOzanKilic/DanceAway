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
    public float beatInterval = 1.0f; // Time between beats in seconds
    public float nextBeatTime;

    public event Action OnBeat;
    public event Action CanMove;

    public SpriteRenderer backGround;

    public float timer;
    public float beatDivider=2f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
    }
    void Start()
    {   
        Initialize();
        Invoke("FlagFalse", 2);
    }

    private bool flag=true;
    private void FlagFalse()
    {
        flag = false;
    }
    public void Initialize()
    {
        audioSource = GetComponent<AudioSource>();
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        nextBeatTime = Time.time + beatInterval;
        gameController = GetComponent<GameController>();    
        //audioMixer.SetFloat("PitchShift", 1f);
        timer=0f;

        //audioMixer.SetFloat("PitchShift",0.8333f);
    }


    private bool canMoveflag=false;
    private bool auidoFlag=true;
    public bool canPlay=true;
    public int beatCounter=0;
    public float beatOffset = 0.1f; // Adjust this value to control the beat offset (in seconds)

    void FixedUpdate()
    {
        timer += Time.deltaTime;

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
                Invoke("PlayHand", 0);
                beatCounter++;
            }
            else if (beatCounter % 16 == 0 && canPlay)
            {
                Invoke("PlayHand", 0f);
                beatCounter++;
            }
            else if(canPlay)
            {
                beatCounter++;
            }

            
            if (auidoFlag)
            {
                //Invoke("PlayAudio", 0.2f);
                auidoFlag = false;
            }
        }

        UpdateBackgroundColor(nextBeatTime);
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
    
    void UpdateBackgroundColor(float nextBeatTime)
    {
        float timeToNextBeat = Mathf.Abs(nextBeatTime - timer);

        if (timeToNextBeat <= beatDivider/10)
        {
            backGround.color = Color.black; // Close to the beat (red threshold)
        }

        else if (timeToNextBeat <= beatDivider/5)
        {
            backGround.color = Color.red; // Close to the beat (red threshold)
        }
        else if (timeToNextBeat <= beatDivider/1.5f)
        {
            backGround.color = Color.blue; // Close to the beat (green threshold)
        }
        else if (timeToNextBeat <= beatDivider)
        {
            backGround.color = Color.green; // Close to the beat (green threshold)
            if(canMoveflag==true){CanMove?.Invoke();canMoveflag=false;}
        }
        else
        {
            backGround.color = Color.white; // Far from the beat
        }
    }
}
