using System.Collections;
using UnityEngine;
using TMPro;

public class ArrowShooting : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; // Inspector에서 DamageTextPrefab 할당
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float flightSpeed = 2f;
    [SerializeField] private float hoverHeight = 2f;
    private DamageUIManager damageUIManager; // DamageUIManager 참조

    public int baseDamage;
    public int damage;

    public GameObject target;
    private Vector3 targetdir;
    private Vector3 previousPosition;
    private MultiPrefabPool objectPool;
    private Animator bowAnimator = null;
    public Spawn spawn;
    public Archer archer;

    public float attackDistance = 0.5f;
    public float plusDistance = 0.1f;
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
        archer = FindObjectOfType<Archer>();
        if (archer == null)
        {
            Debug.LogError("Archer not found!");
        }
        damageUIManager = FindObjectOfType<DamageUIManager>();
        spawn = GameObject.Find("Spawn").GetComponent<Spawn>();

        SetRandomDamage();
    }

    private void OnEnable()
    {
        SetInitialValues();
    }

    private void SetInitialValues()
    {
        if (archer == null)
        {
            archer = FindObjectOfType<Archer>();
        }
        if (bowAnimator == null)
        {
            bowAnimator = FindObjectOfType<Bow>()?.GetComponent<Animator>();
        }
        if (!hasPlayedAnimation)
        {
            bowAnimator.SetTrigger("isAttack");
            hasPlayedAnimation = true;
        }
        // 타겟 및 시작 위치 설정
        target = archer.shortEnemyObject;
        if (target == null)
        {
            Debug.Log("null Target");
            target = archer.shortEnemyObject;
        }
        transform.position = archer.transform.position;
        previousPosition = transform.position;

        if (target != null)
        {
            targetdir = target.GetComponent<DemoPlayer>().dir;
            StartCoroutine(IEFlight());
        }
    }

    private IEnumerator IEFlight()
    {
        if (target == null) yield break;

        float duration = flightSpeed;
        float time = 0.0f;
        Vector3 start = transform.position;
        Vector3 end = target.transform.position + targetdir * plusDistance;

        Debug.Log(end);

        while (time < duration)
        {
            if (target == null) yield break;

            time += Time.deltaTime;
            float linearT = time / duration;
            float heightT = curve.Evaluate(linearT);
            float height = Mathf.Lerp(0.0f, hoverHeight, heightT);

            Vector3 currentPosition = Vector2.Lerp(start, end, linearT) + new Vector2(0.0f, height);
            UpdateRotation(currentPosition - previousPosition);
            transform.position = currentPosition;
            previousPosition = currentPosition;

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
        if (other.gameObject == target )
        {
            AttackNearbyTargets();
            ReturnToPool();
        }
    }
    public void GetAttacked()
    {
        target.GetComponent<DemoPlayer>().HP -= damage;
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
    private void AttackNearbyTargets()
    {
        // 데미지 텍스트를 띄울 때마다 새로운 랜덤 데미지 설정
        SetRandomDamage();

        // 공격 범위 내에 있는 모든 Collider2D 객체를 가져옴
        Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(transform.position, attackDistance, LayerMask.GetMask("Enemy"));

        foreach (var targetCollider in nearbyTargets)
        {
            // 각 객체에 대해 데미지를 적용
            DemoPlayer targetPlayer = targetCollider.GetComponent<DemoPlayer>();
            if (targetPlayer != null)
            {
                targetPlayer.HP -= damage;
                damageUIManager.ShowDamageText(targetPlayer.transform.position, damage);
                Debug.Log($"Attacked {targetPlayer.name}, HP left: {targetPlayer.HP}");
            }
        }
    }
    private void SetRandomDamage()
    {
        int minDamage = Mathf.FloorToInt(baseDamage * 0.9f); // 최소 10% 감소
        int maxDamage = Mathf.CeilToInt(baseDamage * 1.1f); // 최대 10% 증가
        damage = Random.Range(minDamage, maxDamage + 1); // 정수형 랜덤 데미지
    }

}
