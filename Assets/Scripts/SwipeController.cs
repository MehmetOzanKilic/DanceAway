using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    [SerializeField]private Player player;
    private Vector2 startTouchPosition, endTouchPosition;
    private bool isSwipe;
    
    [SerializeField] private float minSwipeDistance = 50f; // Minimum swipe distance in pixels
    private RectTransform swipeAreaRectTransform;

    void Start()
    {
        // Get the RectTransform of the UI element where you want to detect swipes
        swipeAreaRectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Check if the touch started within the swipe area (UI RectTransform)
                    if (RectTransformUtility.RectangleContainsScreenPoint(swipeAreaRectTransform, touch.position, Camera.main))
                    {
                        startTouchPosition = touch.position;
                        isSwipe = true;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isSwipe)
                    {
                        endTouchPosition = touch.position;
                        if (Vector2.Distance(startTouchPosition, endTouchPosition) >= minSwipeDistance)
                        {
                            DetectSwipeDirection();
                        }
                        isSwipe = false; // Reset the swipe flag
                    }
                    break;

                case TouchPhase.Ended:
                    
                    break;
            }
        }
    }

    private void DetectSwipeDirection()
    {
        Vector2 swipeDirection = endTouchPosition - startTouchPosition;
        float x = swipeDirection.x;
        float y = swipeDirection.y;

        float angleInDegrees = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        if(20 <= Mathf.Abs(angleInDegrees) && Mathf.Abs(angleInDegrees) <= 70)
        {
            if(angleInDegrees > 0)
            {
                if (isSwipe)
                {
                    player.Move(Vector2Int.right);
                    player.Move(Vector2Int.up);
                    player.transform.eulerAngles = new Vector3(0,0,0);
                }
            }
            else
            {
                if (isSwipe)
                {
                    player.Move(Vector2Int.right);
                    player.Move(Vector2Int.down);
                    player.transform.eulerAngles = new Vector3(0,0,180);
                }
            }
        }
        else if(110 <= Mathf.Abs(angleInDegrees) && Mathf.Abs(angleInDegrees) <= 160)
        {
            if(angleInDegrees > 0)
            {
                if (isSwipe)
                {
                    player.Move(Vector2Int.left);
                    player.Move(Vector2Int.up);
                    player.transform.eulerAngles = new Vector3(0,0,0);
                }
            }
            else
            {
                if (isSwipe)
                {
                    player.Move(Vector2Int.left);
                    player.Move(Vector2Int.down);
                    player.transform.eulerAngles = new Vector3(0,0,180);
                }
            }
        }

        else
        {
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                if (x > 0)
                {
                    //Debug.Log("Swipe Right");
                    if (isSwipe)
                    {
                        player.Move(Vector2Int.right);
                        player.transform.eulerAngles = new Vector3(0,0,270);
                    }
                }
                else
                {
                    //Debug.Log("Swipe Left");
                    if (isSwipe)
                    {
                        player.Move(Vector2Int.left);
                        player.transform.eulerAngles = new Vector3(0,0,90);
                    }
                }
            }
            else
            {
                if (y > 0)
                {
                    //Debug.Log("Swipe Up");
                    if (isSwipe)
                    {
                        player.Move(Vector2Int.up);
                        player.transform.eulerAngles = new Vector3(0,0,0);
                    }
                }
                else
                {
                    //Debug.Log("Swipe Down");
                    if (isSwipe)
                    {
                        player.Move(Vector2Int.down);
                        player.transform.eulerAngles = new Vector3(0,0,180);
                    }
                }
            }
        }
    }
}
