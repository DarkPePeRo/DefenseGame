using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Archer : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float timer = 0;
    public float shotDelay;
    public Vector3 pos;
    public float radius = 0f;
    public LayerMask layer;
    public Collider2D[] colliders;
    public Collider2D shortEnemy;
    public GameObject shortEnemyObject;

    public Character character;

    void Start()
    {
        objectPool = GameObject.FindObjectOfType<MultiPrefabPool>();
        if (objectPool == null)
        {
            Debug.LogError("MultiPrefabPool not found! Please ensure a PoolManager exists in the scene.");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > shotDelay - 0.1f)
        {
            colliders = Physics2D.OverlapCircleAll(transform.position, radius, layer);
        }
        if (timer > shotDelay)
        {
            if (colliders.Length > 0)
            {
                shortEnemy = colliders[colliders.Length - 1];
                shortEnemyObject = shortEnemy.gameObject;
                ShootArrow(shortEnemyObject);
                if (shortEnemyObject.transform.position.x < transform.position.x)
                {
                    transform.localScale = new Vector3(-0.6f, 0.6f, 1);
                }
                else
                {
                    transform.localScale = new Vector3(0.6f, 0.6f, 1);
                }
            }
            timer = 0;
        }
    }
    private void ShootArrow(GameObject target)
    {
        if (objectPool == null)
        {
            Debug.LogError("Object pool is not initialized. Arrow cannot be spawned.");
            return;
        }

        GameObject arrow = objectPool.GetObject("Arrow");

        if (arrow != null)
        {
            arrow.transform.position = transform.position;

            ArrowShooting arrowScript = arrow.GetComponent<ArrowShooting>();
            if (arrowScript != null)
            {
                arrowScript.target = target;
                arrowScript.SetInitialValues(); // 화살 초기화
                arrowScript.baseDamage = character.GetCurrentAttackPower();
            }
            else
            {
                Debug.LogError("ArrowShooting script not found on the arrow object.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}