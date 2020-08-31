using UnityEngine;
using UnityEngine.Events;

public class BallControl : MonoBehaviour
{
    [Header("Mobility")]
    public float xInitialForce;
    public float yInitialForce;

    [HideInInspector] public float targetVelocity;   // velocity for adding force to the ball
    [HideInInspector] public Vector2 lastVelocity;

    public float pushDelay = 2f;
    private float pushTimer;
    private bool isPushed;

    private Vector2 trajectoryOrigin;
    [Space]

    
    [Header("Modifiers")]
    public bool isModifiedFireBall;     // If true will instant kill collided player
    [Space]
    public PlayerControl lastHitPlayer;



    [Header("Events")]
    public UnityEvent OnBounce;
    public UnityEvent OnHitPlayer;
    public UnityEvent OnModified;
    public UnityEvent OnHitPlayerWithModifier;

    private SpriteRenderer spriteRenderer;
    [HideInInspector] public Rigidbody2D rigidBody2D;
    [SerializeField] private GameManager gameManager;
    [HideInInspector] public GameObject fxFireball;
    
    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        fxFireball = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        pushTimer = pushDelay;

        trajectoryOrigin = transform.position;

        targetVelocity = xInitialForce + yInitialForce;
    }

    private void Update()
    {
        if (isPushed == false)      // Handle PushBall on start of a round
        {
            if (pushTimer <= 0)
            {
                PushBall();

                isPushed = true;

                pushTimer = pushDelay;
            }
            else if (gameManager.isPaused || gameManager.isQuitting)
            {
                // Do Nothing / pause the pushTimer
            }
            else if (gameManager.isGameOver)
            {
                pushTimer = pushDelay;
            }
            else
                pushTimer -= Time.deltaTime;
        }

        if (isModifiedFireBall && !gameManager.isPaused)     // fireball particle fx rotation
        {
            Vector2 delta = -rigidBody2D.velocity.normalized;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            fxFireball.transform.rotation = rotation;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerControl>() != null && isModifiedFireBall)
        {   
            collision.gameObject.GetComponent<PlayerControl>().Destroy(0, this);

            OnHitPlayerWithModifier.Invoke();   // trigger the player hit with fireball event (sound fx)
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        trajectoryOrigin = transform.position;

        if (collision.gameObject.GetComponent<PlayerControl>() != null)
        {
            lastHitPlayer = collision.gameObject.GetComponent<PlayerControl>();

            OnHitPlayer.Invoke();   // trigger the player hit event (sound fx)
        }
        else
        {
            OnBounce.Invoke();      // trigger the bounce event (sound fx)
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PowerUp>() != null && lastHitPlayer != null)
        {
            PowerUp powerUp = other.gameObject.GetComponent<PowerUp>();

            lastHitPlayer.PowerUpExtend(powerUp.scaleY, powerUp.duration);

            gameManager.alreadyUsedPowerUp = true;

            powerUp.Destroy();
        }
    }

    public Vector2 TrajectoryOrigin
    {
        get { return trajectoryOrigin; }
    }

    public void ModifyFireBall(float speedModifier)
    {
        rigidBody2D.velocity *= speedModifier;
        isModifiedFireBall = true;
        fxFireball.SetActive(true);

        OnModified.Invoke();    // trigger the on fire / modified event (sound fx)
    }

    private void ResetBall()
    {
        transform.position = Vector2.zero;

        rigidBody2D.velocity = Vector2.zero;

        isModifiedFireBall = false;

        fxFireball.SetActive(false);
    }

    private void PushBall()
    {
        float yRandomInitialForce = Random.Range(-yInitialForce, yInitialForce);

        float randomDir = Random.Range(0,2);

        if (randomDir < 1.0f)
        {
            rigidBody2D.AddForce(new Vector2(-xInitialForce, yRandomInitialForce).normalized * targetVelocity);
        }
        else
        {
            rigidBody2D.AddForce(new Vector2(xInitialForce, yRandomInitialForce).normalized * targetVelocity);
        }
    }

    public void RestartGame()
    {
        ResetBall();

        isPushed = false;
    }
}
