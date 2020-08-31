using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    public PlayerControl player1;
    private Rigidbody2D player1Rigidbody;
    [Space]
    public PlayerControl player2; 
    private Rigidbody2D player2Rigidbody;

    [Space]



    [Header("Ball")]
    public BallControl ball;
    private Rigidbody2D ballRigidbody;
    private CircleCollider2D ballCollider;
    [Space]
    public Trajectory trajectory;

    [Space]



    [Header("Modifiers")]   // All modifiers are overrided by these matched variables
    public float powerUpSpawnDelay = 5.0f;
    public float powerUpStayDuration = 10.0f;
    public float powerUpDuration = 15.0f;
    public float scaleYMultiplier = 2.0f;
    [HideInInspector] public bool alreadyUsedPowerUp;   // Check if true then extend the spawn delay
    [Space][Space]
    public float ballModifierSpawnDelay = 10.0f;
    public float ballModifierDuration = 7.0f;
    public float ballSpeedModifier = 2.0f;
    [Space]
    public GameObject pfPowerUp;
    public GameObject pfBallModifier;

    [Space]


    [Header("GUI")]
    public Text player1Text;
    public Text player2Text;
    [Space]
    public GameObject uiGameOver;
    public GameObject uiMenu;
    public GameObject uiFadeOut;
    public Text winnerText;
    [Space]
    public Font fontGeneral;
    public Texture2D texNormal;
    public Texture2D texActive;
    public Texture2D texHover;
    private GUIStyle guiButtonStyle;

    [Space]

    [Header("Events")]
    public UnityEvent OnButtonPushed;
    public UnityEvent OnGameOver;
    public UnityEvent OnGameQuit;


    [Space]

    [Header("Others")]
    public int maxScore;
    public bool isGameOver;     // true if game is over
    public bool isPaused;       // true if paused
    public bool isQuitting;     // true on quit
    public bool isLoadingScene; // true on scene changes

    private bool isDebugWindowShown = false;

    private void Awake()
    {
        player1Rigidbody = player1.GetComponent<Rigidbody2D>();
        player2Rigidbody = player2.GetComponent<Rigidbody2D>();
        ballRigidbody = ball.GetComponent<Rigidbody2D>();
        ballCollider = ball.GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        StartCoroutine(CR_SpawnPowerUp());
        StartCoroutine(CR_SpawnModifier());

        uiMenu.SetActive(false);

        uiGameOver.SetActive(false);
        isGameOver = false;

        guiButtonStyle = new GUIStyle();

        uiFadeOut.SetActive(false);

        ball.RestartGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnButtonPushed.Invoke();

            isPaused = !isPaused;

            if (isPaused)
            {
                ball.lastVelocity = ballRigidbody.velocity;
                ballRigidbody.velocity = Vector2.zero;      // Stop the ball movement on pause

                uiMenu.SetActive(true);     // Show the menu on pause

                Time.timeScale = 0f;    // Stop all time related calculations including movements and animations
            }
            else
            {
                Time.timeScale = 1f;

                if (!ball.isModifiedFireBall)   // Add force after unpause to continue, double the speed for fireball
                {
                    ballRigidbody.AddForce(ball.lastVelocity.normalized * ball.targetVelocity);
                }
                else
                {
                    ballRigidbody.AddForce(ball.lastVelocity.normalized * ball.targetVelocity * ballSpeedModifier);
                }

                uiMenu.SetActive(false);    // Hide the menu on resume
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.R))
        {
            ResetGame();     // Restart Game shortcut : Lshift + R
        }

        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Delete)) && !isQuitting && !isLoadingScene)
        {
            Time.timeScale = 1.0f;

            ballRigidbody.velocity = Vector2.zero;

            StartCoroutine(QuitGame());     // Quit Game shortcut : Lshift + Del

            isQuitting = true;
        }

        /*

        // For Debug Purposes : Add 1 score to player 1 #cheating
        if (Input.GetKeyDown(KeyCode.Z))
        {
            player1.IncrementScore();

            RestartRound();
        }

        // For Debug Purposes : Add 1 score to player 2 #cheating
        if (Input.GetKeyDown(KeyCode.X))
        {
            player2.IncrementScore();

            RestartRound();
        }

        // For Debug Purposes : Modify ball to fireball #cheating
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ball.ModifyFireBall(2f);
        }
        */
    }

    public IEnumerator CR_SpawnPowerUp()
    {
        while (true)
        {
            float randDelay = Random.Range(powerUpSpawnDelay, powerUpSpawnDelay * 2f);

            if (alreadyUsedPowerUp)
                yield return new WaitForSeconds(randDelay * 3f);
            else
                yield return new WaitForSeconds(randDelay);

            if (ballRigidbody.velocity.magnitude > 0)
            {
                alreadyUsedPowerUp = false;

                float randY = Random.Range(-7, 7);
                float randX = Random.Range(0, 2);
                
                GameObject powerUpInstance = Instantiate(pfPowerUp);

                if (powerUpInstance != null)   // avoid getting error "Missing Reference ERROR"
                {
                    powerUpInstance.SetActive(true);

                    if (randX < 1f)
                        powerUpInstance.transform.position = new Vector2(-3, randY);
                    else
                        powerUpInstance.transform.position = new Vector2(3, randY);

                    PowerUp powerUp = powerUpInstance.GetComponent<PowerUp>();
                    powerUp.duration = powerUpDuration;
                    powerUp.scaleY = scaleYMultiplier;

                    Destroy(powerUpInstance, powerUpStayDuration);
                }

                // Debug.Log("Spawned a Extend Power-up");
            }
            else
                yield return new WaitForSeconds(randDelay / 2f);
        }
    }

    public IEnumerator CR_SpawnModifier()
    {
        while (true)
        {
            float randDelay = Random.Range(ballModifierSpawnDelay, ballModifierSpawnDelay * 2f);

            yield return new WaitForSeconds(randDelay);

            if (ballRigidbody.velocity.magnitude > 0)
            {
                float randY = Random.Range(-7, 7);
                
                GameObject modifierInstance = Instantiate(pfBallModifier) as GameObject;

                if (modifierInstance != null)   // avoid getting error "Missing Reference ERROR"
                {
                    modifierInstance.SetActive(true);
                    modifierInstance.transform.position = new Vector2(0, randY);
                    modifierInstance.GetComponent<BallModifier>().speedModifier = ballSpeedModifier;

                    Destroy(modifierInstance, ballModifierDuration);
                }
                // Debug.Log("Spawned a Fireball Modifier");
            }
            else
                yield return new WaitForSeconds(randDelay / 2f);
        }
    }

    public void UpdateGUI() // Update UI called each player score changes
    {
        player1Text.text = player1.Score.ToString();
        player2Text.text = player2.Score.ToString();
    }

    public void CheckWinner()
    {
        if (player1.Score >= maxScore)
        {
            ball.SendMessage("ResetBall", SendMessageOptions.RequireReceiver);

            uiGameOver.SetActive(true);

            winnerText.text = "PLAYER ONE WINS";

            Debug.Log("Player 1 win");

            OnGameOver.Invoke();

            isGameOver = true;
        }
        else if (player2.Score >= maxScore)
        {
            ball.SendMessage("ResetBall", SendMessageOptions.RequireReceiver);

            uiGameOver.SetActive(true);

            winnerText.text = "PLAYER TWO WINS";

            Debug.Log("Player 2 win");

            OnGameOver.Invoke();

            isGameOver = true;
        }
    }

    public void LoadMainMenu()
    {
        if (!isLoadingScene && !isQuitting)
        {
            Time.timeScale = 1.0f;

            uiFadeOut.SetActive(true);

            StartCoroutine(CR_LoadMainMenu());

            isLoadingScene = true;
        }
    }

    public IEnumerator CR_LoadMainMenu()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(0);
    }

    public void RestartRound()
    {
        player1.gameObject.SetActive(true);
        player2.gameObject.SetActive(true);

        // Clear all player power-ups
        player1.RemovePowerUp();
        player2.RemovePowerUp();

        player1.transform.position = new Vector2(player1.transform.position.x, 0);
        player2.transform.position = new Vector2(player2.transform.position.x, 0);

        // Destroy all remaining modifiers
        GameObject[] remainingModifiers = GameObject.FindGameObjectsWithTag("Modifier");
        foreach(GameObject modifier in remainingModifiers)
        {
            Destroy(modifier);
        }

        trajectory.drawBallAtCollision = false;
        trajectory.ballAtCollision.SetActive(false);
        
        // Debug.Log("Game Restarted");
    }

    public void ResetGame()
    {
        isGameOver = false;
    
        // Reset both score
        player1.ResetScore();
        player2.ResetScore();

        // Clear all player power-ups
        player1.RemovePowerUp();
        player2.RemovePowerUp();

        // Reset racket/player's y axis pos
        player1.transform.position = new Vector2(player1.transform.position.x, 0);
        player2.transform.position = new Vector2(player2.transform.position.x, 0);

        ball.SendMessage("RestartGame", SendMessageOptions.RequireReceiver);

        // Destroy all remaining modifiers
        GameObject[] remainingModifiers = GameObject.FindGameObjectsWithTag("Modifier");
        foreach(GameObject modifier in remainingModifiers)
        {
            Destroy(modifier);
        }

        uiGameOver.SetActive(false);
    }

    public IEnumerator QuitGame()
    {
        OnGameQuit.Invoke();

        uiFadeOut.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        Debug.Log("Quit");

        Application.Quit();
    }

    private void OnGUI()
    {
        if (!isQuitting && !isLoadingScene)    // Draw GUI is it's not on quitting state
        {
            Color defaultGuiColor = GUI.color;
            GUI.skin.font = fontGeneral;

            // Set the font family/face and font size
            guiButtonStyle.font = fontGeneral;
            guiButtonStyle.fontSize = 32;
            guiButtonStyle.alignment = TextAnchor.MiddleCenter;

            // Set the guistyle on normal state
            guiButtonStyle.normal.background = texNormal;
            guiButtonStyle.normal.textColor = new Color32(180,180,180,255);
            
            // Set the guistyle on active state
            guiButtonStyle.active.background = texActive;
            guiButtonStyle.active.textColor = new Color32(150,150,150,255);

            // Set the guistyle on hover state
            guiButtonStyle.hover.background = texHover;
            guiButtonStyle.hover.textColor = new Color32(220,220,220,255);

            // GUI.Label(new Rect(Screen.width / 2 - 150 - 12, 20, 100, 100), "" + player1.Score);
            // GUI.Label(new Rect(Screen.width / 2 - 150 + 12, 20, 100, 100), "" + player2.Score);

            if (GUI.Button(new Rect(Screen.width / 2 - 60, 35, 120, 53), "RESTART", guiButtonStyle))
            {
                OnButtonPushed.Invoke();

                isGameOver = false;

                ResetGame();
            }

            /*
            if (player1.Score == maxScore)
            {
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 10, 2000, 1000), "PLAYER ONE WINS");

                ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            }
            else if (player2.Score == maxScore)
            {
                GUI.Label(new Rect(Screen.width / 2 + 30, Screen.height / 2 - 10, 2000, 1000), "PLAYER TWO WINS");
    
                ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            }
            */

            if (GUI.Button(new Rect(Screen.width/2 - 65, Screen.height - 100, 130, 72), "TOGGLE\nDEBUG INFO", guiButtonStyle))
            {
                OnButtonPushed.Invoke();

                isDebugWindowShown = !isDebugWindowShown;
            }

            if (isDebugWindowShown)
            {
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.yellow;     // Change background color on debug

                float ballMass = ballRigidbody.mass;
                Vector2 ballVelocity = ballRigidbody.velocity;
                float ballSpeed = ballRigidbody.velocity.magnitude;
                Vector2 ballMomentum = ballMass * ballVelocity; 
                float ballFriction = ballCollider.friction;
    
                float impulsePlayer1X = player1.LastContactPoint.normalImpulse;
                float impulsePlayer1Y = player1.LastContactPoint.tangentImpulse;
                float impulsePlayer2X = player2.LastContactPoint.normalImpulse;
                float impulsePlayer2Y = player2.LastContactPoint.tangentImpulse;
    
                string textSoloOrMulti;     // For Debug : gamemode "solo" or "multi"
                if (Settings.isSolo)
                    textSoloOrMulti = "Solo";
                else
                    textSoloOrMulti = "Multi";

                string textPlayer1PowerUp;      // For Debug : player 1 power-up timer text
                if (player1.isOnPowerUp)
                    textPlayer1PowerUp = player1.powerUpTimer.ToString("F1");
                else
                    textPlayer1PowerUp = "No Power-up";

                string textPlayer2PowerUp;      // For Debug : player 2 power-up timer text
                if (player2.isOnPowerUp)
                    textPlayer2PowerUp = player2.powerUpTimer.ToString("F1");
                else
                    textPlayer2PowerUp = "No Power-up";

                string textBallOnFire;
                if (ball.isModifiedFireBall)
                    textBallOnFire = "Yes";
                else
                    textBallOnFire = "No";

                // Debug text
                string debugText =
                    "Gamemode = " + textSoloOrMulti + "\n" +
                    "Player 1 power-up = " + textPlayer1PowerUp + "\n" +
                    "Player 2 power-up = " + textPlayer2PowerUp + "\n\n" +

                    "Ball is on FIRE = " + textBallOnFire + "\n" +  
                    "Ball velocity = " + ballVelocity + "\n" +
                    "Ball speed = " + ballSpeed + "\n" +
                    "Ball mass = " + ballMass + "\n" +
                    "Ball momentum = " + ballMomentum + "\n" +
                    "Ball friction = " + ballFriction + "\n" +
                    "Last impulse from player 1 = (" + impulsePlayer1X.ToString("F2") + ", " + impulsePlayer1Y.ToString("F2") + ")\n" +
                    "Last impulse from player 2 = (" + impulsePlayer2X.ToString("F2") + ", " + impulsePlayer2Y.ToString("F2") + ")\n";

                // Show text GUI
                GUIStyle guiDebugStyle = new GUIStyle(GUI.skin.textArea);
                guiDebugStyle.fontSize = 24;
                guiDebugStyle.alignment = TextAnchor.UpperCenter;
                GUI.TextArea(new Rect(Screen.width/2 - 200, Screen.height - 320, 400, 190), debugText, guiDebugStyle);
            
                GUI.backgroundColor = oldColor;     // Change background color back to normal

                trajectory.enabled = !trajectory.enabled;
                trajectory.ballAtCollision.SetActive(true);
            }
            else
            {
                trajectory.ballAtCollision.SetActive(false);
            }

            GUI.color = defaultGuiColor;
        }
    }
}