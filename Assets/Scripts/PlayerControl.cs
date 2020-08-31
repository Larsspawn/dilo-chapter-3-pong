using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerControl : MonoBehaviour
{
    [Header("Key Input")]
    public KeyCode upButton1 = KeyCode.W;
    public KeyCode downButton1 = KeyCode.S;
    [Space]
    public KeyCode upButton2 = KeyCode.UpArrow;
    public KeyCode downButton2 = KeyCode.DownArrow;
    [Space]

    [Header("Mobility")]
    public float speed = 10.0f;
    public float yBoundary = 9.0f;

    private Rigidbody2D rigidbody2D;

    private ContactPoint2D lastContactPoint;


    // Power-up
    private float currentScaleY;
    private float targetScaleY;
    private float scaleY;

    [HideInInspector] public float powerUpTimer;
    [HideInInspector] public bool isOnPowerUp;
    [HideInInspector] public float powerUpTransitionTimer;

    // Events
    [Space]

    public UnityEvent OnPowerUp;
    public UnityEvent OnPowerUpRunOut;

    [Space]
    
    public bool isSecondPlayer;
    [SerializeField] private GameManager gameManager;

    private int score;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();

        targetScaleY = 1f;
    }

    private void Update()
    {
        if (gameManager.isGameOver || gameManager.isPaused || gameManager.isQuitting)   // Ignore codes below on gameover or paused
        {
            rigidbody2D.velocity = Vector2.zero;    // Stop the player movement

            return;
        }

        InputHandler();
            
        // Limit racket position on Y axis
        Vector3 position = transform.position;

        if (position.y > yBoundary)
        {
            position.y = yBoundary;
        }
        else if (position.y < -yBoundary)
        {
            position.y = -yBoundary;
        }

        transform.position = position;

        HandlePowerUp();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.name.Equals("Ball"))
        {
            lastContactPoint = collision.GetContact(0);
        }
    }

    public void InputHandler()
    {
        Vector2 velocity = rigidbody2D.velocity;

        if (Settings.isSolo)    // if the player choose solo player gameplay from MainMenu
        {
            if (Input.GetKey(upButton1) || Input.GetKey(upButton2))
                velocity.y = speed;
            else if (Input.GetKey(downButton1) || Input.GetKey(downButton2))
                velocity.y = -speed;
            else
                velocity.y = 0.0f;
        }
        else       // if the player choose multi player gameplay from MainMenu
        { 
            if (!isSecondPlayer)        // First player input : WASD Keys
            {
                if (Input.GetKey(upButton1))
                    velocity.y = speed;
                else if (Input.GetKey(downButton1))
                    velocity.y = -speed;
                else
                    velocity.y = 0.0f;    
            }
            else    // Second player input : Arrow Keys
            {
                if (Input.GetKey(upButton2))
                    velocity.y = speed;
                else if (Input.GetKey(downButton2))
                    velocity.y = -speed;
                else
                    velocity.y = 0.0f;
            }
        }

        rigidbody2D.velocity = velocity;
    }

    public void HandlePowerUp()
    {
        if (isOnPowerUp)
        {
            if (powerUpTimer <= 0)  // When the power-up duration is up
            {
                isOnPowerUp = false;

                currentScaleY = transform.localScale.y;     // variables for lerp back to normal size
                targetScaleY = 1f;

                powerUpTransitionTimer = 0;

                OnPowerUpRunOut.Invoke();
            }
            else if (gameManager.isPaused)
            {
                // Do Nothing / pause the pushTimer
            }
            else
            {
                if (powerUpTransitionTimer < 1)
                {
                    powerUpTransitionTimer += Time.deltaTime * 1.5f;

                    // Lerp from current size to target size 
                    scaleY = Mathf.Lerp(currentScaleY, targetScaleY, powerUpTransitionTimer);

                    transform.localScale = new Vector2(
                        transform.localScale.x, scaleY
                    );
                }
                    
                powerUpTimer -= Time.deltaTime;     // Decreasing power-up duration overtime
            }
        }
        else
        {
            if (powerUpTransitionTimer < 1)     // Lerp back to normal size when not on power-up
            {
                powerUpTransitionTimer += Time.deltaTime  * 1.5f;

                scaleY = Mathf.Lerp(currentScaleY, targetScaleY, powerUpTransitionTimer);

                transform.localScale = new Vector2(
                    transform.localScale.x, scaleY
                );
            } 
        }
    }

    public void RemovePowerUp()     // remove power-up
    {
        powerUpTransitionTimer = 0f;
        powerUpTimer = 0f;
        isOnPowerUp = false;
        currentScaleY = 1f;
        targetScaleY = 1f;
    }

    public void PowerUpExtend(float scaleYMultiplier, float duration)
    {
        isOnPowerUp = true;     // Start the power-up size lerping
        powerUpTimer = duration;    // Reset the timer to duration

        // Variables for lerping from current y scale to target y scale
        currentScaleY = transform.localScale.y;    
        targetScaleY = transform.localScale.y * scaleYMultiplier;

        powerUpTransitionTimer = 0; // reset the t for lerp

        OnPowerUp.Invoke();
    }

    public void IncrementScore()
    {
        score++;

        gameManager.CheckWinner();

        gameManager.UpdateGUI();
    }

    public void ResetScore()
    {
        score = 0;

        gameManager.UpdateGUI();
    }

    public ContactPoint2D LastContactPoint
    {
        get { return lastContactPoint; }
    }

    public int Score
    {
        get { return score; }
    }

    public void Destroy(float delay, BallControl ball)
    {
        if (isSecondPlayer)     // if player 2 destroyed then player 1 scored
            gameManager.player1.IncrementScore();
        else
            gameManager.player2.IncrementScore();

        if (Score < gameManager.maxScore && !gameManager.isGameOver)
        {
            gameManager.ball.gameObject.SendMessage("RestartGame", SendMessageOptions.RequireReceiver);
        }

        gameObject.SetActive(false);    // fake destroy

        gameManager.RestartRound();
    }
}
