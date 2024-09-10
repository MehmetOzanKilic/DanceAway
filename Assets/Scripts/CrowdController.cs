using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdController : MonoBehaviour
{
    [SerializeField]private GameObject trianglePrefab;
    [SerializeField]private int xOffset;
    [SerializeField]private int yOffset;
    private List<GameObject> crowd;
    void Start()
    {
        
    }
    // Initialized after GameController
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
        // Determines how wide and tall the crowd will be
        int xLenght = (int)(width*tileSize)/4;
        int yLenght = (int)(height*tileSize) + 5;

        // Spawns columns until the desired width is reached
        for (int i = 0; i < xLenght/xOffset; i++)
        {   
            // Fills each column with triangles from top to bottom
            for (int j = 0; j < yLenght/yOffset; j++)
            {
                // Position of individual left triangles according to the offsets
                float xPos = -(i*xOffset)-(tileSize*0.75f);
                float yPos = (j*yOffset)-(tileSize*0.75f);
                if(i%2==1)yPos+=yOffset/2;
                GameObject triangle = Instantiate(trianglePrefab,new Vector3( xPos, yPos, 0), Quaternion.identity);
                triangle.GetComponent<SpriteRenderer>().sortingOrder = (int)(1000-xPos);
                crowd.Add(triangle);

                // Position of individual right triangles according to the offsets
                xPos = (i*xOffset)+((width-1)*tileSize) + (tileSize*0.75f);
                yPos = (j*yOffset)-(tileSize*0.75f);
                if(i%2==1)yPos+=yOffset/2;
                triangle = Instantiate(trianglePrefab,new Vector3( xPos, yPos, 0), Quaternion.identity);
                triangle.transform.eulerAngles = new Vector3(0,180,0);
                triangle.GetComponent<SpriteRenderer>().sortingOrder = (int)(1000+xPos);
                crowd.Add(triangle);
            }
        }

        SelectInitialNodders();

        // Initiaizing each cTriangle
        foreach(GameObject triangle in crowd)
        {
            triangle.GetComponent<cTriangle>().Initialize(beatTimer);
        }
    }

    private List<cTriangle> nodders;
    private List<GameObject> notNodders;
    [Range(0,1)]
    [SerializeField]private float initialNodderPer;
    private void SelectInitialNodders()
    {
        nodders = new List<cTriangle>();
        notNodders = new List<GameObject>(crowd);
        
        // The number of initial nodders according to the crowd number
        int initialNoddernum = (int)(notNodders.Count*initialNodderPer);
        // Randomly activating nodding for some cTriangles.
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

    // Method to randomly activate more cTriangles to nod
    public void MoreNodders(int more)
    {
        //print("more nodders:" + more);
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
    // Method to randomly deactivate some cTriangles to not nod
    public void LessNodders(int less)
    {
        //print("less nodders:" + less);
        if(less>nodders.Count)less=nodders.Count;
        for (int i = 0; i < less; i++)
        {
            int random = Random.Range(0, nodders.Count);
            cTriangle temp = nodders[random];
            temp.canNod = false;
            temp.NotVibing();
            notNodders.Add(temp.gameObject);
            nodders.RemoveAt(random);
        }
    }


}
