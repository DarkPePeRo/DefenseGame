using UnityEngine;

public class WindmillRotate : MonoBehaviour
{
    public float rotationSpeed = 120f; // 초당 회전 각도

    void Update()
    {
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }
}
