using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;


public class God : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float timer = 0;
    public float shotDelay;
    public LayerMask layer;
    public Collider2D[] colliders;
    public Collider2D shortEnemy;
    public GameObject shortEnemyObject;
    public float range; // 마름모 범위
    public Collider2D[] candidates;

    public float tileWidth;
    public float tileHeight;

    public GameObject godDetailUI;

    void Start()
    {

    }


    void Update()
    {
        timer += Time.deltaTime;
        if (timer > shotDelay - 0.1f)
        {
            List<Collider2D> result = new List<Collider2D>();
            Vector2 center = transform.position;

            // 마름모 꼭짓점 4개 계산
            Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
            Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
            Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
            Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

            // 마름모를 폴리곤으로 정의
            Vector2[] diamond = new Vector2[] { top, right, bottom, left };

            // 넓은 후보 영역 감지 (아이소메트릭이더라도 먼저 원형으로 뽑음)
            candidates = Physics2D.OverlapCircleAll(center, range, layer);
            // 유닛 위치가 마름모 안에 있는지 검사
            if (candidates != null)
            {
                foreach (var c in candidates)
                {
                    if (!c.gameObject.activeInHierarchy)
                        continue;
                    Vector2 p = c.ClosestPoint(center);

                    if (IsPointInPolygon(p, diamond))
                    {
                        result.Add(c);
                        colliders = result.ToArray();
                    }
                }

            }
            else { colliders = new Collider2D[0]; }
            colliders = colliders.Where(c => c != null && c.gameObject.activeInHierarchy).ToArray();
        }
        if (timer > shotDelay)
        {
            if (colliders.Length > 0)
            {
                shortEnemy = colliders[colliders.Length - 1];
                shortEnemyObject = shortEnemy.gameObject;
                GameObject thunder = objectPool.GetObject("Thunder");
                if (shortEnemyObject.transform.position.x < transform.position.x)
                {
                    transform.localScale = new Vector3(-0.6f, 0.6f, 1);
                }
                else
                {
                    transform.localScale = new Vector3(0.6f, 0.6f, 1);
                }
            }
            else
            {
                shortEnemyObject = null;
            }
            timer = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 center = transform.position;

        // 마름모 꼭짓점 계산
        Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
        Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
        Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
        Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

        // 마름모 선 그리기
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(top, right);
        Gizmos.DrawLine(right, bottom);
        Gizmos.DrawLine(bottom, left);
        Gizmos.DrawLine(left, top);
    }

    bool IsPointInPolygon(Vector2 p, Vector2[] poly)
    {
        int count = poly.Length;
        bool inside = false;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
            {
                inside = !inside;
            }
        }
        return inside;
    }

}
