using System.Collections;
using UnityEngine;
using TMPro;

public class ArrowShooting : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; // Inspector���� DamageTextPrefab �Ҵ�
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float flightSpeed;
    [SerializeField] private float hoverHeight;
    private DamageUIManager damageUIManager; // DamageUIManager ����

    public float baseDamage;
    public int damage;

    public GameObject target;
    private Vector3 targetdir;
    private Vector3 previousPosition;
    private MultiPrefabPool objectPool;
    private Animator bowAnimator = null;
    public Spawn spawn;
    public Archer archer;
    public WaveSystem waveSystem;

    public float attackDistance = 0.5f;
    public float plusDistance = 0.1f;
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
        archer = FindObjectOfType<Archer>();
        if (archer == null)
        {
            Debug.LogError("Archer not found!");
        }
        waveSystem = FindObjectOfType<WaveSystem>();
        damageUIManager = FindObjectOfType<DamageUIManager>();
        spawn = GameObject.Find("Spawn").GetComponent<Spawn>();

        SetRandomDamage(); 
        bowAnimator.SetBool("isAttack", false);
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
            hasPlayedAnimation = true;
        }
        target = archer.shortEnemyObject;

        if (target == null)
        {
            Debug.Log("Null Target");
            return;
        }

        if (target.tag == "Enemy")
        {
            transform.position = archer.transform.position;
            previousPosition = transform.position;
            targetdir = target.GetComponent<DemoPlayer>().dir;
            StartCoroutine(IEFlight());
        }
        if(target.tag == "Boss")
        {
            transform.position = archer.transform.position;
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
            bowAnimator.SetBool("isAttack", true);
            float linearT = time / duration;
            float heightT = curve.Evaluate(linearT);
            float height = Mathf.Lerp(0.0f, hoverHeight, heightT);

            Vector3 currentPosition = Vector2.Lerp(start, end, linearT) + new Vector2(0.0f, height);
            UpdateRotation(currentPosition - previousPosition);
            transform.position = currentPosition;
            previousPosition = currentPosition;

            yield return null;
        }
        bowAnimator.SetBool("isAttack", false); 
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

    // �ִϸ��̼��� �ٽ� �����ؾ� �� �� ȣ���Ͽ� �ʱ�ȭ ����
    public static void ResetAnimationTrigger()
    {
        hasPlayedAnimation = false;
    }
    private void AttackNearbyTargets()
    {
        // ������ �ؽ�Ʈ�� ��� ������ ���ο� ���� ������ ����
        SetRandomDamage();

        // ���� ���� ���� �ִ� ��� Collider2D ��ü�� ������
        Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(transform.position, attackDistance, LayerMask.GetMask("Enemy"));

        foreach (var targetCollider in nearbyTargets)
        {
            // �� ��ü�� ���� �������� ����
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
        int minDamage = Mathf.FloorToInt(baseDamage * 0.9f * waveSystem.GetArrowDamageMultiplier()); // �ּ� 10% ����
        int maxDamage = Mathf.CeilToInt(baseDamage * 1.1f * waveSystem.GetArrowDamageMultiplier()); // �ִ� 10% ����
        damage = Random.Range(minDamage, maxDamage + 1); // ������ ���� ������
    }
    private void AttackTargetDirectly()
    {
        SetRandomDamage();
        if(target == null)
        {
            Debug.Log("NullTargetnow");
        }
        // Ÿ���� ������ �� �ٷ� �ǰ� ������ ����
        if (target.tag == "Enemy")
        {
            DemoPlayer targetPlayer = target.GetComponent<DemoPlayer>();
            if (targetPlayer != null)
            {
                if (targetPlayer.CurrentHP > 0)
                {
                    targetPlayer.CurrentHP -= damage;
                    damageUIManager.ShowDamageText(targetPlayer.transform.position, damage);
                    Debug.Log($"Attacked {targetPlayer.name}, HP left: {targetPlayer.CurrentHP}");
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
                    damageUIManager.ShowDamageText(targetPlayer.transform.position + new Vector3(0.3f, 0.5f, 0), damage);
                    Debug.Log($"Attacked {targetPlayer.name}, HP left: {targetPlayer.CurrentHP}");
                }
            }
        }
    }
}
