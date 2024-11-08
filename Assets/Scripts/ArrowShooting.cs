using System.Collections;
using UnityEngine;

public class ArrowShooting : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float flightSpeed = 2f;
    [SerializeField] private float hoverHeight = 2f;

    private GameObject target;
    private Vector3 previousPosition;
    private MultiPrefabPool objectPool;
    private Animator bowAnimator = null;

    public Vector3 first;
    public Vector3 second;

    public float attackDistance = 0.5f;
    private static bool hasPlayedAnimation = false; // �ִϸ��̼��� ����Ǿ����� ���θ� ����

    private void Start()
    {
        objectPool = GameObject.Find("PoolManager")?.GetComponent<MultiPrefabPool>();
        if (objectPool == null)
        {
            Debug.LogError("Object Pool not found! Please assign a PoolManager with MultiPrefabPool component.");
        }
        bowAnimator = FindObjectOfType<Bow>()?.GetComponent<Animator>();
        if (bowAnimator == null)
        {
            Debug.LogError("Archer Animator not found! Please assign an Animator component to the Archer.");
        }
    }

    private void OnEnable()
    {
        SetInitialValues();

        // �ִϸ��̼��� ���� ������� �ʾҴٸ� ����
        
        // Animator�� null���� Ȯ���ϰ�, null�̶�� �ٽ� �������� �õ�
        if (bowAnimator == null)
        {
            bowAnimator = FindObjectOfType<Bow>()?.GetComponent<Animator>();
            if (bowAnimator == null)
            {
                Debug.LogError("Archer Animator not found! Please assign an Animator component to the Archer.");
                return; // Animator�� ������ ������ �ڵ� ���� �ߴ�
            }
        }
        if (!hasPlayedAnimation)
        {
            bowAnimator?.SetTrigger("isAttack");
            hasPlayedAnimation = true;
        }
        if (target != null)
        {
            StartCoroutine(IEFlight());
        }
    }

    private void SetInitialValues()
    {
        // Ÿ�� �� ���� ��ġ ����
        target = FindObjectOfType<Archer>()?.shortEnemyObject;
        transform.position = FindObjectOfType<Archer>()?.transform.position ?? transform.position;
        previousPosition = transform.position;
    }

    private IEnumerator IEFlight()
    {
        if (target == null) yield break;

        float duration = flightSpeed;
        float time = 0.0f;
        Vector3 start = transform.position;
        Vector3 end = target.transform.position + transform.up * attackDistance + transform.forward * attackDistance;

        while (time < duration)
        {
            if (target == null) yield break;

            time += Time.deltaTime;
            float linearT = time / duration;
            float heightT = curve.Evaluate(linearT);
            float height = Mathf.Lerp(0.0f, hoverHeight, heightT);

            second = transform.position;
            Vector3 minus = second - first;
            UpdateRotation(minus);
            first = second;

            transform.position = Vector2.Lerp(start, end, linearT) + new Vector2(0.0f, height);

            previousPosition = transform.position;

            yield return null;
        }

        ReturnToPool();
    }

    private void UpdateRotation(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0.0f, 0.0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == target)
        {
            objectPool.ReturnObject(other.gameObject);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        target = null;
        ResetAnimationTrigger();
        objectPool.ReturnObject(gameObject);
    }

    // �ִϸ��̼��� �ٽ� �����ؾ� �� �� ȣ���Ͽ� �ʱ�ȭ ����
    public static void ResetAnimationTrigger()
    {
        hasPlayedAnimation = false;
    }
}
