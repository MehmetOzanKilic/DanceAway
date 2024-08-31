using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cTriangle : MonoBehaviour
{
    public BeatTimer beatTimer;
    public float tRotation;
    // Start is called before the first frame update

    void Start()
    {

    }
    public void Initialize(BeatTimer beatTimer,float randomX,float randomY)
    {
        this.beatTimer = beatTimer;
        beatTimer.OnBeat += Nodding;
        transform.position = new Vector3(randomX, randomY, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Nodding()
    {
        StartCoroutine(Nod());
    }

    private IEnumerator Nod()
    {   
        transform.eulerAngles = new Vector3(0f, tRotation,-15f);

        yield return new WaitForSeconds(beatTimer.beatInterval/2);

        transform.eulerAngles = new Vector3(0f, tRotation, 0f);
    }

}
