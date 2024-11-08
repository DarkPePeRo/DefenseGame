using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Archer : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float timer = 0;
    public float shotDelay;
    public Vector3 pos;
    public List<GameObject> FoundObject;
    public float radius = 0f;
    public LayerMask layer;
    public Collider2D[] colliders;
    public Collider2D shortEnemy;
    public GameObject shortEnemyObject;

    void Start()
    {

    }
    
    
    void Update()
    {
        pos = transform.position;
        timer += Time.deltaTime;
        if (timer > shotDelay)
        {
            if(colliders.Length > 0)
            {
                GameObject arrow = objectPool.GetObject("Arrow");
            }
            timer = 0;
        }
        colliders = Physics2D.OverlapCircleAll(transform.position, radius, layer);
        if (colliders.Length == 1) { 
            shortEnemy = colliders[0];
            shortEnemyObject = shortEnemy.gameObject;
            if (shortEnemyObject.transform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-0.6f, 0.6f, 1);
            }

        }
        if (colliders.Length > 0)
        {
            shortEnemy = colliders[colliders.Length-1];
            shortEnemyObject = shortEnemy.gameObject;
            //float short_distance = Vector3.Distance(transform.position, colliders[0].transform.position);
            //foreach (Collider2D col in colliders)
            //{
            //    float short_distance2 = Vector3.Distance(transform.position, col.transform.position);
            //    if (short_distance > short_distance2)
            //    {
            //        short_distance = short_distance2;
            //        shortEnemy = col;
            //        shortEnemyObject = shortEnemy.gameObject;
            //    }
            //}
            if (shortEnemyObject.transform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-0.6f, 0.6f, 1);
            }
            else
            {
                transform.localScale = new Vector3(0.6f, 0.6f, 1);
            }
        }
    }

    public void FindClosestTarget()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
