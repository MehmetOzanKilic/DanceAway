using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cTriangle : MonoBehaviour
{
    public BeatTimer beatTimer;
    private Animator animator;

    void Start()
    {

    }
    public void Initialize(BeatTimer beatTimerRef)
    {
        beatTimer = beatTimerRef;
        beatTimer.OnBeat += Nodding;
        canNod=false;
        animator = GetComponent<Animator>();
        // Initial animation
        NotVibing();
    }

    public void NotVibing()
    {
        float random = Random.Range(0,5);
        Invoke("StartNotVibing",random);
    }

    public void StartNotVibing()
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
