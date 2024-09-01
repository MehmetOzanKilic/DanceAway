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
    public void Initialize(BeatTimer beatTimer,float randomX,float randomY)
    {
        this.beatTimer = beatTimer;
        beatTimer.OnBeat += Nodding;
        transform.position = new Vector3(randomX, randomY, 0);
        animator = GetComponent<Animator>();
        animator.Play("cTriangle_Idle");
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
        animator.Play("cTriangle_Nod");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0).Length);

        animator.Play("cTriangle_Idle");
    }

}
