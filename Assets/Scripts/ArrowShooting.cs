using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ArrowShooting : MonoBehaviour
{
    [SerializeField]
    public AnimationCurve curve;
    public float flightSpeed = 2f;
    public float hoverHeight = 2f;

    public GameObject target;

    public void Start()
    {
        StartCoroutine("IEFlight");
    }

    private IEnumerator IEFlight()
    {
        float duration = flightSpeed;
        float time = 0.0f;
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            float linearT = time / duration;
            float heightT = curve.Evaluate(linearT);

            float height = Mathf.Lerp(0.0f, hoverHeight, heightT);

            transform.position = Vector2.Lerp(start, end, linearT) + new Vector2(0.0f, height);

            yield return null;
        }
    }
}
