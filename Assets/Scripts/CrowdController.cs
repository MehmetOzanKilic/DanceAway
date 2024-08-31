using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdController : MonoBehaviour
{
    [SerializeField]private GameObject trianglePrefab;
    [SerializeField]private int crowdNumber;
    public BeatTimer beatTimer;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void Initialize(BeatTimer beatTimer)
    {
        SpawnCrowd(beatTimer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [SerializeField]private float distribution;
    private void SpawnCrowd(BeatTimer beatTimer)
    {
        for (int i = 0; i<crowdNumber ; i++)
        {
            float randomY = (int)(Random.Range(-8/distribution, 50/distribution)*distribution);
            if(i<crowdNumber/2)
            {
                float randomX = (int)(Random.Range(-6/distribution,-30/distribution)*distribution);
                GameObject triangle = Instantiate(trianglePrefab,new Vector3(randomX, randomY,0), Quaternion.identity);
                triangle.GetComponent<cTriangle>().Initialize(beatTimer,randomX,randomY);
                triangle.GetComponent<cTriangle>().tRotation = 0;
                triangle.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (int)(1000-randomX);
            }
            else if(i>=crowdNumber/2)
            {
                float randomX = (int)(Random.Range(46/distribution, 70/distribution)*distribution);
                GameObject triangle = Instantiate(trianglePrefab,new Vector3(randomX, randomY,0), Quaternion.identity);
                triangle.transform.eulerAngles = new Vector3(0,180,0); 
                triangle.GetComponent<cTriangle>().Initialize(beatTimer,randomX,randomY);
                triangle.GetComponent<cTriangle>().tRotation = 180;
                triangle.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (int)(1000+randomX);
            }
        }
    }
}
