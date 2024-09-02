using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cTriangle : MonoBehaviour
{
    public BeatTimer beatTimer;
    private Animator animator;
    public float tRotation;

    // Start is called before the first frame update

    void Start()
    {

    }
    public void Initialize(BeatTimer beatTimerRef)
    {
        beatTimer = beatTimerRef;
        if(beatTimer == null)print("nonono");
        beatTimer.OnBeat += Nodding;
        //transform.position = new Vector3(xPos, yPos, 0);
        animator = GetComponent<Animator>();
        float random = Random.Range(0,5);
        Invoke("StartNotVibing",random);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartNotVibing()
    {
        animator.Play("cTriangle_notVibing");
    }

    public bool canNod=false;
    private void Nodding()
    {
        if(canNod)StartCoroutine(Nod());
    }

    private IEnumerator Nod()
    {   
        animator.Play("cTriangle_Nod");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0).Length);

        animator.Play("cTriangle_Idle");
    }

}
