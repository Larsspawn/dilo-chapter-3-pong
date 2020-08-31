using UnityEngine;

public class BallModifier : MonoBehaviour
{
    public float speedModifier;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            BallControl ball = other.gameObject.GetComponent<BallControl>();

            ball.ModifyFireBall(speedModifier);     // Call the modify function to set to fireball
        }
    }
}
