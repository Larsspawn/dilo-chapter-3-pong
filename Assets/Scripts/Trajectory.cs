using UnityEngine;

public class Trajectory : MonoBehaviour
{
    public BallControl ball;
    private CircleCollider2D ballCollider;
    private Rigidbody2D ballRigidbody;
 
    public GameObject ballAtCollision;

    [Space]

    public LayerMask layerMask;

    Vector2 offsetHitPoint = new Vector2();

    [HideInInspector]
    public bool drawBallAtCollision = false;
    
    private void Start()
    {
        ballRigidbody = ball.GetComponent<Rigidbody2D>();
        ballCollider = ball.GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        RaycastHit2D[] circleCastHit2DArray = Physics2D.CircleCastAll(
            ballRigidbody.position, 
            ballCollider.radius,
            ballRigidbody.velocity.normalized,
            Mathf.Infinity,
            layerMask
        );

        foreach (RaycastHit2D circleCastHit2D in circleCastHit2DArray)
        {
            if (circleCastHit2D.collider != null &&
                circleCastHit2D.collider.GetComponent<BallControl>() == null)
            {
                Vector2 hitPoint = circleCastHit2D.point;
 
                Vector2 hitNormal = circleCastHit2D.normal;

                offsetHitPoint = hitPoint + hitNormal * ballCollider.radius;

                DottedLine.DottedLine.Instance.DrawDottedLine(ball.transform.position, offsetHitPoint);

                if (circleCastHit2D.collider.GetComponent<SideWall>() == null)
                {
                    Vector2 inVector = (offsetHitPoint - ball.TrajectoryOrigin).normalized;

                    Vector2 outVector = Vector2.Reflect(inVector, hitNormal);

                    float outDot = Vector2.Dot(outVector, hitNormal);
                    if (outDot > -1.0f && outDot < 1.0)
                    {
                        DottedLine.DottedLine.Instance.DrawDottedLine(
                            offsetHitPoint,
                            offsetHitPoint + outVector * 10.0f
                        );

                        drawBallAtCollision = true;
                    }
                }

                break;
            }
        }

        if (drawBallAtCollision)
        {
            // Gambar bola "bayangan" di prediksi titik tumbukan
            ballAtCollision.transform.position = offsetHitPoint;
            ballAtCollision.SetActive(true);
        }
        else
        {
            // Sembunyikan bola "bayangan"
            ballAtCollision.SetActive(false);
        }
    }
}
