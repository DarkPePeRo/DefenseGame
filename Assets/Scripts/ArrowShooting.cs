using System.Collections;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class ArrowShooting : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; // Inspector에서 DamageTextPrefab 할당
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float flightSpeed;
    [SerializeField] private float hoverHeight;
    private DamageUIManager damageUIManager; // DamageUIManager 참조

    public float baseDamage;
    public int damage;
    public bool isCritical;

    public GameObject target;
    private Vector3 targetdir;
    private Vector3 previousPosition;
    private MultiPrefabPool objectPool;
    public WaveSystem waveSystem;

    public float attackDistance = 0.5f;
    public float plusDistance = 0.1f;
    private static bool hasPlayedAnimation = false; // 애니메이션이 실행되었는지 여부를 추적

    private void Start()
    {
        waveSystem = FindObjectOfType<WaveSystem>();
        damageUIManager = FindObjectOfType<DamageUIManager>();
        objectPool = FindObjectOfType<MultiPrefabPool>();

        SetRandomDamage(); 
    }

    private void OnEnable()
    {
    }

    public void SetInitialValues()
    {
        if (target == null)
        {
            Debug.Log("Null Target");
            return;
        }

        if (target.tag == "Enemy")
        {
            previousPosition = transform.position;
            targetdir = target.GetComponent<DemoPlayer>().dir;
            StartCoroutine(IEFlight());
        }
        if (target.tag == "Wolf")
        {
            previousPosition = transform.position;
            targetdir = target.GetComponent<Wolf>().dir;
            StartCoroutine(IEFlight());
        }
        if(target.tag == "Boss")
        {
            previousPosition = transform.position;
            targetdir = target.GetComponent<Boss>().dir;
            StartCoroutine(IEFlight());
        }
    }

    private IEnumerator IEFlight()
    {
        if (target == null) yield break;
        float duration = flightSpeed;
        float time = 0.0f;
        Vector3 start = target.transform.position.x < transform.position.x ? transform.position + new Vector3(-0.2f, 0.2f, 0) : transform.position + new Vector3(0.2f, 0.2f, 0);
        Vector3 end = target.transform.position + targetdir * plusDistance + new Vector3(0, 0.3f, 0);


        while (time < duration)
        {
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
        if (target != null)
        {
            AttackTargetDirectly();
        }
        ReturnToPool();
    }

    private void UpdateRotation(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0.0f, 0.0f, angle);
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.gameObject == target)
    //    {
    //        AttackNearbyTargets();
    //        ReturnToPool();
    //    }
    //}

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
                targetPlayer.CurrentHP -= damage;
                damageUIManager.ShowDamageText(targetPlayer.transform.position, damage);
                Debug.Log($"Attacked {targetPlayer.name}, HP left: {targetPlayer.CurrentHP}");
            }
        }
    }
    private void SetRandomDamage()
    {
        isCritical = false;
        int minDamage = Mathf.FloorToInt(baseDamage * 0.9f); // 최소 10% 감소
        int maxDamage = Mathf.CeilToInt(baseDamage * 1.1f); // 최대 10% 증가
        damage = Random.Range(minDamage, maxDamage + 1); // 정수형 랜덤 데미지

        float criticalChance = 0.3f; // 5% 확률
        if (Random.value < criticalChance) // Random.value는 0.0 ~ 1.0 사이의 값 반환
        {
            isCritical = true;
            int minCriticalDamage = Mathf.FloorToInt(damage * 2.5f); // 치명타: 데미지 250%~300% 증가
            int maxCriticalDamage = Mathf.FloorToInt(damage * 3f);
            damage = Random.Range(minCriticalDamage, maxCriticalDamage);
            Debug.Log("Critical Hit! Damage: " + damage);
        }
    }
    private void AttackTargetDirectly()
    {
        SetRandomDamage();
        if(target == null)
        {
            Debug.Log("NullTargetnow");
        }
        // 타겟이 존재할 때 바로 피격 판정을 수행
        if (target.tag == "Enemy")
        {
            DemoPlayer targetPlayer = target.GetComponent<DemoPlayer>();
            if (targetPlayer != null)
            {
                if (targetPlayer.CurrentHP > 0)
                {
                    targetPlayer.CurrentHP -= damage;
                    if (isCritical == true)
                    {
                        damageUIManager.damageTextPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                    }
                    damageUIManager.ShowDamageText(targetPlayer.transform.position, damage);
                    damageUIManager.damageTextPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                }
            }
        }
        if (target.tag == "Wolf")
        {
            Wolf targetPlayer = target.GetComponent<Wolf>();
            if (targetPlayer != null)
            {
                if (targetPlayer.CurrentHP > 0)
                {
                    targetPlayer.CurrentHP -= damage;
                    if (isCritical == true)
                    {
                        damageUIManager.damageTextPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                    }
                    damageUIManager.ShowDamageText(targetPlayer.transform.position, damage);
                    damageUIManager.damageTextPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                }
            }
        }
        if (target.tag == "Boss")
        {
            Boss targetPlayer = target.GetComponent<Boss>();
            if (targetPlayer != null)
            {
                if (targetPlayer.CurrentHP > 0)
                {
                    targetPlayer.CurrentHP -= damage;
                    if (isCritical == true)
                    {
                        damageUIManager.damageTextPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                    }
                    damageUIManager.ShowDamageText(targetPlayer.transform.position + new Vector3(0.3f, 0.7f, 0), damage);
                    damageUIManager.damageTextPrefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                }
            }
        }
    }


}
