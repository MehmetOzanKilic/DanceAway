using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Common.Enums;

public class Player : MonoBehaviour
{
    public int score;
    private int maxHealth=100;
    public int health; // Player starting health
    public Vector2Int position;
    private GameController gameController; // Reference to GameController
    private BeatTimer beatTimer;
    private Animator animator;
    private Rigidbody2D rb;
    public BeatState State { get; set; }
    private float tileSize;
    public LayerMask spotlightLayer;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        tileSize = gameController.tileSize;
        beatTimer = GameObject.Find("GameController").GetComponent<BeatTimer>();
        position = new Vector2Int(((gameController.width+1)/2)-1,0); // Starting position at the bottom-middle tile
        transform.position = new Vector2(position.x * tileSize, position.y * tileSize);
        animator = GetComponent<Animator>();
        animator.Play("idle");
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;

    }

    void FixedUpdate()
    {

    }


    private Vector2Int newPosition;
    private int moveCount=0;  
    [SerializeField]private Text scoreText;

    public int moveAllowed=1;
    private int mult;
    public void Move(Vector2Int direction)
    {
        State = beatTimer.State;

        bool validMove=true;

        if (State == BeatState.OffBeat)
        {
            validMove=false; // Ignore movement if not allowed
        }

        // Only count moves if there are enemies.
        if(gameController.enemies.Count > 0)moveCount++;

        if(validMove)newPosition = position + direction;
        // Check if the new position is within the grid bounds
        if (newPosition.x >= 0 && newPosition.x < gameController.width && newPosition.y >= 0 && newPosition.y < gameController.height)
        {
            int scoreIncrement = 0;
            CheckForSpotlightCollision();
            if(gameController.enemies.Count > 0)
            {
                if (State == BeatState.PerfectBeat)
                {
                    scoreIncrement = 200; // Black threshold
                }
                else if (State == BeatState.CloseBeat)
                {
                    scoreIncrement = 150; // Red threshold
                }
                else if (State == BeatState.MiddleBeat)
                {
                    scoreIncrement = 100; // Red threshold
                }
                else if (State == BeatState.FarBeat)
                {
                    scoreIncrement = 50; // Red threshold
                }
                else if (State == BeatState.OffBeat)
                {
                    scoreIncrement=-200;
                    gameController.LessNodders();
                    StartCoroutine(WrongMove(direction));
                }
                else
                {
                    scoreIncrement = 0;
                }

            }
            // Update player's position
            
            if(validMove)
            {
                position = newPosition;
                rb.AddForce(direction*(int)(200*tileSize));
            }//???????? 

            HitStrongestTriangle(scoreIncrement*mult);


            // giving negative score without the spotlight multiplier
            if(validMove)score += scoreIncrement*mult;
            else score += scoreIncrement;

            scoreText.text = mult.ToString();
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
            // Instead of using GetComponent, use the SpotlightCache to get the spotlight reference
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
        rb.AddForce(direction*(int)(200*tileSize));

        yield return new WaitForSeconds(beatTimer.beatInterval/4);

        rb.AddForce(-direction*(int)(200*tileSize));
    }

    private IEnumerator ResetAnimation(string animationName)
    {
        // Get the runtime animator controller
        if(!takingDamage)animator.Play(animationName);

        // Wait until the animation has finished playing
        yield return new WaitForSeconds(0.2f);

        // Optionally, reset the animation to the default state
        if(!takingDamage)animator.Play("idle"); // Replace "Idle" with the name of your default animation state
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
        var strongestTriangle = gameController.enemies
            .FirstOrDefault(t => t.powerLevel == maxPower);

        // If a triangle is found, deal damage to it
        if (strongestTriangle != null)
        {
            strongestTriangle.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // Handle player death (e.g., end game, restart level)
            Debug.Log("Player has died");
            gameController.OpenEndScreen();
        }

        StartCoroutine(DamageTaken());
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
}
