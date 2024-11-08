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
    private static bool hasPlayedAnimation = false; // 애니메이션이 실행되었는지 여부를 추적

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

        // 애니메이션이 아직 실행되지 않았다면 실행
        
        // Animator가 null인지 확인하고, null이라면 다시 가져오기 시도
        if (bowAnimator == null)
        {
            bowAnimator = FindObjectOfType<Bow>()?.GetComponent<Animator>();
            if (bowAnimator == null)
            {
                Debug.LogError("Archer Animator not found! Please assign an Animator component to the Archer.");
                return; // Animator가 없으면 나머지 코드 실행 중단
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
        // 타겟 및 시작 위치 설정
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

    // 애니메이션을 다시 실행해야 할 때 호출하여 초기화 가능
    public static void ResetAnimationTrigger()
    {
        hasPlayedAnimation = false;
    }
}
