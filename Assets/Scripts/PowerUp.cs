using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float duration;
    public float scaleY;

    public GameObject fxDestroy;

    public void Destroy()
    {
        // Instantiate(fxDestroy);

        Destroy(gameObject);
    }
}
