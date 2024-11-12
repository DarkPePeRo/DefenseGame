using System.Collections;
using UnityEngine;
using TMPro;

public class ArrowShooting : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; // Inspector���� DamageTextPrefab �Ҵ�
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float flightSpeed = 2f;
    [SerializeField] private float hoverHeight = 2f;
    private DamageUIManager damageUIManager; // DamageUIManager ����

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
        // Ÿ�� �� ���� ��ġ ����
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
                targetPlayer.HP -= damage;
                damageUIManager.ShowDamageText(targetPlayer.transform.position, damage);
                Debug.Log($"Attacked {targetPlayer.name}, HP left: {targetPlayer.HP}");
            }
        }
    }
    private void SetRandomDamage()
    {
        int minDamage = Mathf.FloorToInt(baseDamage * 0.9f); // �ּ� 10% ����
        int maxDamage = Mathf.CeilToInt(baseDamage * 1.1f); // �ִ� 10% ����
        damage = Random.Range(minDamage, maxDamage + 1); // ������ ���� ������
    }

}
