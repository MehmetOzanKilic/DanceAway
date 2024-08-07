using UnityEngine;
using System;
using System.Threading;
using Unity.Mathematics;

public class BeatTimer : MonoBehaviour
{
    public float beatInterval = 1.0f; // Time between beats in seconds
    public float nextBeatTime;

    public event Action OnBeat;
    public event Action CanMove;

    public SpriteRenderer backGround;

    public float timer;
    public float beatDivider=2f;

    void Start()
    {
        backGround = GameObject.Find("BackGround").GetComponent<SpriteRenderer>();
        nextBeatTime = Time.time + beatInterval;
        timer=0f;
    }

    private bool flag=false;
    private bool canMoveflag=false;
    void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= nextBeatTime+beatDivider)
        {   
            nextBeatTime += beatInterval;
            flag=false;
            canMoveflag=true;   
        }
        if (timer >= nextBeatTime && flag==false)
        {   
            flag=true;
            OnBeat?.Invoke();
        }

        UpdateBackgroundColor(nextBeatTime);
    }
    
    void UpdateBackgroundColor(float nextBeatTime)
    {
        float timeToNextBeat = Mathf.Abs(nextBeatTime - timer);

        if (timeToNextBeat <= beatDivider/3)
        {
            backGround.color = Color.black; // Close to the beat (red threshold)
        }

        else if (timeToNextBeat <= beatDivider*2/3)
        {
            backGround.color = Color.red; // Close to the beat (red threshold)
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
