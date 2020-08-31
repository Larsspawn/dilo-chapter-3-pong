using UnityEngine;
using UnityEngine.Events;

public class SideWall : MonoBehaviour
{
    public PlayerControl player;

    public UnityEvent OnScored;

    [SerializeField] private GameManager gameManager; 
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (player.Score < gameManager.maxScore && !gameManager.isGameOver)
            {
                player.IncrementScore();

                OnScored.Invoke();      // Trigger on scored event (sound fx)

                other.gameObject.SendMessage("RestartGame", SendMessageOptions.RequireReceiver);
            }

            gameManager.RestartRound();
        }
    }
}
