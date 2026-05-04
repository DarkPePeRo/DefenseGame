using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gumro : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject damageTextPrefab;
    private DamageUIManager damageUIManager;

    [Header("Damage")]
    public float baseDamage;
    public float damage;
    public float attackSpeed;
    [SerializeField] private float criticalRate = 20f;
    [SerializeField] private float criticalDamage = 200f;
    [SerializeField] private float criticalScale = 1.25f;
    [SerializeField] private float normalScale = 0.5f;

    [Header("Position")]
    public float plusDistance = 0.1f;
    public ShadowController shadow;
    public SpriteRenderer sprite;

    [Header("Area Attack")]
    [SerializeField] private LayerMask targetLayer;   // Enemy, Boss Layer 지정
    [SerializeField] private float rangePadding = 0f; // 범위 조금 키우고 싶으면 0.2~0.5
    [SerializeField] private bool showDebugRange = true;

    [SerializeField] private Animator animator;

    [SerializeField] private float hitDelay = 0.07f;
    [SerializeField] private float returnDelay = 0.22f;
    [SerializeField] private string slashAnimName = "GumroSlash";

    private bool isCritical;
    [Header("Critical Effect")]
    [SerializeField] private float criticalHitStop = 0.035f;

    public GameObject target;
    private Vector3 targetdir;
    private MultiPrefabPool objectPool;

    public GodStatManage godStatManage;

    private Coroutine attackCoroutine;

    void Start()
    {
        CacheRefs();
    }

    private void OnEnable()
    {
        CacheRefs();
        SetInitialValues();
    }

    private void CacheRefs()
    {
        if (objectPool == null)
            objectPool = GameObject.Find("PoolManager")?.GetComponent<MultiPrefabPool>();

        if (objectPool == null)
            Debug.LogError("Object Pool not found! Please assign a PoolManager with MultiPrefabPool component.");

        if (damageUIManager == null)
            damageUIManager = FindObjectOfType<DamageUIManager>();

        if (shadow == null)
            shadow = FindObjectOfType<ShadowController>();

        if (godStatManage == null)
            godStatManage = FindObjectOfType<GodStatManage>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (godStatManage != null)
            baseDamage = godStatManage.attackPower;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void SetInitialValues()
    {
        if (shadow == null)
        {
            Debug.Log("ShadowController 없음");
            ReturnToPoolSafe();
            return;
        }

        if (shadow.shortEnemyObject != null)
        {
            target = shadow.shortEnemyObject;
        }

        if (target == null)
        {
            Debug.Log("Null Target");
            ReturnToPoolSafe();
            return;
        }

        Vector3 attackerPos = shadow.transform.position;
        Vector3 targetPos = target.transform.position;

        Vector3 dir = targetPos - attackerPos;
        dir.z = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector3.right;

        if (target.CompareTag("Enemy"))
        {
            SetRandomDamage();

            transform.position = targetPos - dir * 0.1f + new Vector3(0f, 0.5f, 0f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            if (isCritical)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            StartAttackCoroutine();
        }
        else if (target.CompareTag("Boss"))
        {
            SetRandomDamage();

            transform.position = targetPos - dir * 0.1f + new Vector3(0f, 1.5f, 0f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            if (isCritical)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            StartAttackCoroutine();
        }
        else
        {
            ReturnToPoolSafe();
        }
    }

    private void StartAttackCoroutine()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        attackCoroutine = StartCoroutine(ThunderAttack());
    }

    private IEnumerator ThunderAttack()
    {
        if (animator != null)
        {
            animator.Play(slashAnimName, 0, 0f);
        }

        yield return new WaitForSeconds(hitDelay);

        AttackBySpriteRange();

        if (isCritical)
        {
            yield return StartCoroutine(HitStop(criticalHitStop));
        }

        float remainTime = Mathf.Max(0f, returnDelay - hitDelay);
        yield return new WaitForSeconds(remainTime);

        ReturnToPoolSafe();
    }

    private IEnumerator HitStop(float duration)
    {
        float originalTimeScale = Time.timeScale;

        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
    }
    private void AttackBySpriteRange()
    {
        if (sprite == null)
        {
            Debug.LogWarning("SpriteRenderer 없음");
            return;
        }

        Bounds bounds = sprite.bounds;

        Vector2 center = bounds.center;
        Vector2 size = bounds.size;

        size.x += rangePadding;
        size.y += rangePadding;

        float angle = transform.eulerAngles.z;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            size,
            angle,
            targetLayer
        );

        if (hits == null || hits.Length == 0)
            return;

        HashSet<MonsterHealth> damagedTargets = new HashSet<MonsterHealth>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            if (!hit.CompareTag("Enemy") && !hit.CompareTag("Boss"))
                continue;

            MonsterHealth monsterHealth = hit.GetComponent<MonsterHealth>();

            if (monsterHealth == null)
                monsterHealth = hit.GetComponentInParent<MonsterHealth>();

            if (monsterHealth == null)
                continue;

            if (damagedTargets.Contains(monsterHealth))
                continue;

            if (monsterHealth.currentHP <= 0)
                continue;

            damagedTargets.Add(monsterHealth);

            monsterHealth.TakeDamage(damage);

            if (damageUIManager != null)
            {
                damageUIManager.ShowDamageText(
                    monsterHealth.transform.position + new Vector3(0.3f, 0.5f, 0),
                    damage
                );
            }
        }
    }

    private void SetRandomDamage()
    {
        isCritical = Random.Range(0f, 100f) < criticalRate;

        float randomMultiplier = Random.Range(0.9f, 1.1f);

        damage = baseDamage * randomMultiplier;

        if (isCritical)
        {
            damage *= criticalDamage / 100f;
        }

        damage *= 100000f;
    }

    private void ReturnToPoolSafe()
    {
        target = null;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (objectPool != null)
            objectPool.ReturnObject(gameObject);
        else
            gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugRange) return;

        SpriteRenderer sr = sprite != null ? sprite : GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Bounds bounds = sr.bounds;

        Gizmos.color = Color.magenta;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            bounds.center,
            Quaternion.Euler(0, 0, transform.eulerAngles.z),
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(
                bounds.size.x + rangePadding,
                bounds.size.y + rangePadding,
                0
            )
        );

        Gizmos.matrix = oldMatrix;
    }
#endif
}