using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdController : MonoBehaviour
{
    [SerializeField]private GameObject trianglePrefab;
    [SerializeField]private int crowdNumber;
    public BeatTimer beatTimer;
    [SerializeField]private float distribution;
    [SerializeField]private int xOffset;
    [SerializeField]private int yOffset;

    private int width;
    private int height;
    private float tileSize;
    private List<GameObject> crowd;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(BeatTimer beatTimer,int width,int height,float tileSize)
    {
        crowd = new List<GameObject>();
        SpawnCrowd(beatTimer,width,height,tileSize);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnCrowd(BeatTimer beatTimer,int width,int height, float tileSize)
    {
        int xLenght = (int)(width*tileSize)/4;
        int yLenght = (int)(height*tileSize) + 5;

        for (int i = 0; i < xLenght/xOffset; i++)
        {
            for (int j = 0; j < yLenght/yOffset; j++)
            {
                float xPos = -(i*xOffset)-(tileSize*0.75f);
                float yPos = (j*yOffset)-(tileSize*0.75f);
                if(i%2==1)yPos+=yOffset/2;
                GameObject triangle = Instantiate(trianglePrefab,new Vector3( xPos, yPos, 0), Quaternion.identity);
                //triangle.GetComponent<cTriangle>().tRotation = 0;
                triangle.GetComponent<SpriteRenderer>().sortingOrder = (int)(1000-xPos);
                crowd.Add(triangle);

                xPos = (i*xOffset)+((width-1)*tileSize) + (tileSize*0.75f);
                yPos = (j*yOffset)-(tileSize*0.75f);
                if(i%2==1)yPos+=yOffset/2;
                triangle = Instantiate(trianglePrefab,new Vector3( xPos, yPos, 0), Quaternion.identity);
                //triangle.GetComponent<cTriangle>().tRotation = 180;
                triangle.transform.eulerAngles = new Vector3(0,180,0);
                triangle.GetComponent<SpriteRenderer>().sortingOrder = (int)(1000+xPos);
                crowd.Add(triangle);
            }
        }

        SelectInitialNodders();

        foreach(GameObject triangle in crowd)
        {
            triangle.GetComponent<cTriangle>().Initialize(beatTimer);
        }
        /*for (int i = 0; i<crowdNumber ; i++)
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
        }*/
    }

    private List<cTriangle> nodders;
    private List<GameObject> notNodders;
    [Range(0,1)]
    [SerializeField]private float initialNodderPer;
    private void SelectInitialNodders()
    {
        nodders = new List<cTriangle>();
        notNodders = new List<GameObject>(crowd);
        
        int initialNoddernum = (int)(notNodders.Count*initialNodderPer);
        print("initialNoddernum:" + initialNoddernum);  
        for(int i = 0; i<initialNoddernum; i++)
        {
            int random = Random.Range(0, notNodders.Count);
            cTriangle temp = notNodders[random].GetComponent<cTriangle>();
            nodders.Add(temp);
            notNodders.RemoveAt(random);   
        }

        foreach(cTriangle nodder in nodders)
        {
            nodder.canNod = true;
        }

    }

    public void MoreNodders(int more)
    {
        print("more nodders:" + more);
        if(more>notNodders.Count)more=notNodders.Count;
        for(int i = 0; i<more; i++)
        {
            int random = Random.Range(0, notNodders.Count);
            cTriangle temp = notNodders[random].GetComponent<cTriangle>();
            nodders.Add(temp);
            notNodders.RemoveAt(random);  
        }

        for(int i = nodders.Count-1; i>nodders.Count-more-1; i--)
        {
            nodders[i].canNod = true;
        }
    }

    public void LessNodders(int less)
    {
        print("less nodders:" + less);
        if(less>nodders.Count)less=nodders.Count;
        for (int i = 0; i < less; i++)
        {
            int random = Random.Range(0, nodders.Count);
            cTriangle temp = nodders[random];
            temp.canNod = false;
            notNodders.Add(temp.gameObject);
            nodders.RemoveAt(random);
        }
    }


}
