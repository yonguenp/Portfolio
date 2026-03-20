using UnityEngine;

public class WorldScroller2D : MonoBehaviour
{
    public float speed = 6f;
    public float resetX = -20f;
    public float loopLength = 40f;

void Update()
    {
        transform.position += Vector3.left * (speed * Time.deltaTime);

        if (transform.position.x <= resetX)
        {
            var p = transform.position;
            p.x += loopLength;
            transform.position = p;
        }
    }
}
