using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Thunder : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; // Inspector���� DamageTextPrefab �Ҵ�
    private DamageUIManager damageUIManager; // DamageUIManager ����

    public float baseDamage;
    public int damage;
    public float attackSpeed;
    public float plusDistance = 0.1f;
    public God god;
    public SpriteRenderer sprite;

    public GameObject target;
    private Vector3 targetdir;
    private Vector3 previousPosition;
    private MultiPrefabPool objectPool;
    public Spawn spawn;
    public WaveSystem waveSystem;
    void Start()
    {
        objectPool = GameObject.Find("PoolManager")?.GetComponent<MultiPrefabPool>();
        if (objectPool == null)
        {
            Debug.LogError("Object Pool not found! Please assign a PoolManager with MultiPrefabPool component.");
        }
        waveSystem = FindObjectOfType<WaveSystem>();
        damageUIManager = FindObjectOfType<DamageUIManager>();
        spawn = GameObject.Find("Spawn").GetComponent<Spawn>();
        god = FindObjectOfType<God>();

        SetRandomDamage();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetInitialValues();
    }

    private void SetInitialValues()
    {
        if (sprite == null) 
        { 
            sprite = GetComponent<SpriteRenderer>();
        }
        if (god == null)
        {
            god = FindObjectOfType<God>();
        }
        target = god.shortEnemyObject;

        if (target == null)
        {
            Debug.Log("Null Target");
            return;
        }

        if (target.tag == "Enemy")
        {
            previousPosition = transform.position;
            targetdir = target.GetComponent<DemoPlayer>().dir;
            transform.position = target.transform.position + targetdir * plusDistance + new Vector3(0, 0.8f, 0);
            StartCoroutine(ThunderAttack());
        }
        if (target.tag == "Boss")
        {
            previousPosition = transform.position;
            targetdir = target.GetComponent<Boss>().dir;
            transform.position = target.transform.position + targetdir * plusDistance + new Vector3(0, 1.5f, 0);
            StartCoroutine(ThunderAttack());
        }

    }
    private IEnumerator ThunderAttack()
    {
        if (target == null) yield break;
        float duration = attackSpeed;
        float time = 0.0f;


        while (time < duration)
        {
            time += Time.deltaTime; 

            yield return null;
        }
        if (target != null)
        {
            AttackTargetDirectly();
        }
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        target = null;
        objectPool.ReturnObject(gameObject);
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
        if (target == null)
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
    // Update is called once per frame
    void Update()
    {
        
    }
}