using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Common.Enums;
using UnityEditor.Animations;

public class Player : MonoBehaviour
{
    [SerializeField]private Text scoreText;
    [SerializeField]private Text beatStateText;
    public int score;
    private int maxHealth=100;
    public int health; // Player starting health
    public Vector2Int position;
    private GameController gameController; // Reference to GameController
    private BeatTimer beatTimer;
    private Animator animator;
    private Animator beatStateAnimator;
    private Rigidbody2D rb;
    public BeatState State { get; set; }
    private float tileSize;
    public LayerMask spotlightLayer;
    private Vector2Int currentDirection;
    //[SerializeField]private List<int> gridBounds = new List<int>();// width lower(0)/upper(1), height lower(2)/upper(3)
    [SerializeField]private List<int> gridBoundsPlayer = new List<int>();// width lower(0)/upper(1), height lower(2)/upper(3)
    void Start()
    {

    }
    public void StartPlayer()
    {
        // GameController and BeatTimer scripts
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        beatTimer = GameObject.Find("GameController").GetComponent<BeatTimer>();
        // Setting the initial bounds for movement.
        tileSize = gameController.tileSize;
        gridBoundsPlayer = gameController.ReturnGridbounds();
        //Nasıl deep coy olmadan kopşyalıyıcam
        // Starting position at the bottom-middle tile
        position = new Vector2Int(3,2);
        transform.position = new Vector2(position.x * tileSize, position.y * tileSize);
        // Starting animation
        animator = GetComponent<Animator>();
        beatStateAnimator = beatStateText.GetComponent<Animator>();
        animator.Play("idle");

        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;

    }

    void FixedUpdate()
    {

    }

    private int moveCount=0;  
    private int mult;
    public void Move(Vector2Int direction)
    {
        Vector2Int newPosition;
        State = beatTimer.State;
        currentDirection = direction;// To use later if the player walks into a triangle.
        crowdPushFlag = true;

        bool validMove=true;

        if (State == BeatState.OffBeat)
        {
            validMove=false; // Ignore movement if in OffBeat
        }

        // Update newPosition if move is valid
        if(validMove)newPosition = position + direction;
        else newPosition = position;

        // Check if the new position is within the grid bounds
        if (newPosition.x >= gridBoundsPlayer[0] && newPosition.x < gridBoundsPlayer[1] && newPosition.y >= gridBoundsPlayer[2] && newPosition.y < gridBoundsPlayer[3])
        {
            int scoreIncrement = 0;
            CheckForSpotlightCollision();// Find out how much mult is.
            if(gameController.enemies.Count > 0)
            {
                moveCount++;// Only count moves if there are enemies.
                if (State == BeatState.PerfectBeat)
                {
                    scoreIncrement = 200; // Perfect score threshold
                    beatStateText.text = "S";
                }
                else if (State == BeatState.CloseBeat)
                {
                    scoreIncrement = 150; // Close score threshold
                    beatStateText.text = "A";
                }
                else if (State == BeatState.MiddleBeat)
                {
                    scoreIncrement = 100; // Middle score threshold
                    beatStateText.text = "B";
                }
                else if (State == BeatState.FarBeat)
                {
                    scoreIncrement = 50; // Far score threshold
                    beatStateText.text = "C";
                }
                else if (State == BeatState.OffBeat)
                {
                    // Moving during off beat is punishing.
                    scoreIncrement=-400;// Make a large score deduction.
                    beatStateText.text = "F";
                    gameController.LessNodders(50);// Decrease the number of cTriangles Nodding.
                    StartCoroutine(WrongMove(direction));
                }
                else
                {
                    scoreIncrement = 0;
                    beatStateText.text = "WTF";
                }

            }

            // Update player's position and move player.           
            if(validMove)
            {
                position = newPosition;
                rb.AddForce(direction*(int)(200*tileSize));
            }
            
            //Only one triangle with the highest powerLevel gets hit.
            HitStrongestTriangle(scoreIncrement*mult);

            // Giving negative score without the spotlight multiplier
            if(validMove)score += scoreIncrement*mult;
            else score += scoreIncrement;

            // Updating score and avarage
            scoreText.text = position.ToString();
            beatStateAnimator.Play("BeatStateText",-1,0f);
            gameController.avarage = score/((moveCount%16)+1);
        }

        else
        {
            StartCoroutine(WrongMove(direction));
        }

        StartCoroutine(ResetAnimation("Player_Moving"));
    }

    private void CheckForSpotlightCollision()
    {
        // Check for nearby colliders in the spotlight layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f, spotlightLayer);

        // mult start with one in each check
        mult = 1;

        foreach (Collider2D collider in colliders)
        {
            SpotlightSquare spotlight;

            if (SpotlightSquare.cachedSpotlights.TryGetValue(collider.gameObject, out spotlight) && spotlight != null)
            {
                // mult gets multiplied if there is a spotlight overlapping with the player
                mult *= spotlight.powerLevel;
            }
        }
    }

    private IEnumerator WrongMove(Vector2 direction)
    {
        //Making the player move back and forth for a wrong move
        rb.AddForce(direction*(int)(200*tileSize));

        yield return new WaitForSeconds(beatTimer.beatInterval/4);

        rb.AddForce(-direction*(int)(200*tileSize));
    }

    private IEnumerator ResetAnimation(string animationName)
    {
        if(!takingDamage)animator.Play(animationName);

        yield return new WaitForSeconds(0.2f);

        if(!takingDamage)animator.Play("idle");
    }

    private void HitStrongestTriangle(int damage)
    {
        if (gameController.enemies.Count == 0)
        {
            return; // No enemies to hit
        }

        // Calculate the maximum power level among all triangles
        int maxPower = gameController.enemies.Max(t => t.powerLevel);

        // Find the first triangle with the maximum power level
        var strongestTriangle = gameController.enemies.FirstOrDefault(t => t.powerLevel == maxPower);

        // If a triangle is found, deal damage to it
        if (strongestTriangle != null)
        {
            strongestTriangle.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        if(!takingDamage)
        {
            health -= damage;
            Move(-currentDirection);
            if (health <= 0)
            {
                // Handle player death
                Debug.Log("Player has died");
                gameController.OpenEndScreen();
            }

            StartCoroutine(DamageTaken());
        }
    }

    private bool takingDamage = false;
    private IEnumerator DamageTaken()
    {
        animator.Play("Player_Damage");
        takingDamage = true;

        yield return new WaitForSeconds(beatTimer.beatInterval);

        animator.Play("idle");
        takingDamage=false;
    }

    private bool crowdPushFlag=true;
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Triangle"))
        {
            Triangle triangle;
            if(Triangle.cachedTriangles.TryGetValue(other.gameObject, out triangle))
            {
                TakeDamage(triangle.powerLevel);
                print("damageTaken: "+triangle.powerLevel);
            }
        }

        if(other.CompareTag("Heart"))
        {
            gameController.CollectHeart(other.gameObject);
        }

    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.eulerAngles = new Vector3(0,0,0);
            Move(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.eulerAngles = new Vector3(0,0,180);
            Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.eulerAngles = new Vector3(0,0,90);
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.eulerAngles = new Vector3(0,0,270);
            Move(Vector2Int.right);
        }
    }

    public void ChangeGridBounds()
    {
        gridBoundsPlayer = gameController.ReturnGridbounds();
    }
}
