using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class God : MonoBehaviour
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

    public float width;
    public float height;

    public GameObject godDetailUI;

    void Start()
    {

    }


    void Update()
    {
        pos = transform.position;
        timer += Time.deltaTime;
        if (timer > shotDelay - 0.1f)
        {
            colliders = Physics2D.OverlapAreaAll(
            new Vector2(transform.position.x - width / 2, transform.position.y - height / 2),
            new Vector2(transform.position.x + width / 2, transform.position.y + height / 2),
            layer);
        }
        if (timer > shotDelay)
        {
            if (colliders.Length > 0)
            {
                shortEnemy = colliders[colliders.Length - 1];
                shortEnemyObject = shortEnemy.gameObject;
                GameObject thunder = objectPool.GetObject("Thunder");
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
            timer = 0;
        }
    }

    public void FindClosestTarget()
    {

    }

    private void OnDrawGizmos()
    {
        // 사각형의 네 꼭짓점 계산
        Vector2 bottomLeft = new Vector2(transform.position.x - width / 2, transform.position.y - height / 2);
        Vector2 bottomRight = new Vector2(transform.position.x + width / 2, transform.position.y - height / 2);
        Vector2 topLeft = new Vector2(transform.position.x - width / 2, transform.position.y + height / 2);
        Vector2 topRight = new Vector2(transform.position.x + width / 2, transform.position.y + height / 2);

        // 기즈모 색상 설정
        Gizmos.color = Color.red;

        // 사각형 그리기 (네 변을 선으로 연결)
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    private void OnMouseDown()
    {
        
    }
}
