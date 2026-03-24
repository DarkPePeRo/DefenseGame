using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class ShadowController : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float timer = 0;
    public float shotDelay;
    public LayerMask layer;

    public Collider2D[] colliders;
    public Collider2D shortEnemy;
    public GameObject shortEnemyObject;
    public Vector3 targetdir;
    public float plusDistance = 0.4f;

    public float range;
    public float detectionRadius;
    public Collider2D[] candidates;
    public Collider2D[] targets;

    public float tileWidth;
    public float tileHeight;

    public GameObject centerObject;

    private Collider2D currentTarget;
    private MonsterHealth currentTargetHealth;

    void Start()
    {

    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!IsTargetStillValid(currentTarget))
        {
            ClearCurrentTarget();
        }

        if (currentTarget == null && timer > shotDelay - 0.1f)
        {
            AcquireTarget();
        }

        if (timer > shotDelay)
        {
            if (!IsTargetStillValid(currentTarget))
            {
                ClearCurrentTarget();
            }

            if (currentTarget != null)
            {
                shortEnemy = currentTarget;
                shortEnemyObject = currentTarget.gameObject;

                Vector3 targetPos = shortEnemyObject.transform.position;
                float distance = Vector2.Distance(transform.position, targetPos);

                // 일정 거리 밖일 때만 이동
                if (distance > detectionRadius)
                {
                    PathMover mover = shortEnemy.GetComponent<PathMover>();
                    if (mover != null)
                    {
                        targetdir = mover.dir.normalized;
                        transform.position = targetPos + (Vector3)(targetdir * plusDistance);
                    }
                }

                // 방향 전환은 이동 여부와 별개로 처리
                if (targetPos.x < transform.position.x)
                    transform.localScale = new Vector3(-0.6f, 0.6f, 1);
                else
                    transform.localScale = new Vector3(0.6f, 0.6f, 1);

                GameObject thunder = objectPool.GetObject("Thunder");
            }
            else
            {
                shortEnemy = null;
                shortEnemyObject = null;
            }

            timer = 0;
        }
    }

    void AcquireTarget()
    {
        List<Collider2D> result = new List<Collider2D>();
        Vector2 center = centerObject.transform.position;

        Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
        Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
        Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
        Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

        Vector2[] diamond = new Vector2[] { top, right, bottom, left };

        candidates = Physics2D.OverlapCircleAll(center, range, layer);

        if (candidates != null)
        {
            foreach (var c in candidates)
            {
                if (c == null || !c.gameObject.activeInHierarchy)
                    continue;

                var mh = c.GetComponentInParent<MonsterHealth>();
                if (mh != null && !mh.IsTargetable)
                    continue;

                Vector2 p = c.ClosestPoint(center);

                if (IsPointInPolygon(p, diamond))
                {
                    result.Add(c);
                }
            }
        }

        colliders = result
            .Where(c => c != null && c.gameObject.activeInHierarchy)
            .ToArray();

        if (colliders.Length > 0)
        {
            currentTarget = colliders[colliders.Length - 1];
            currentTargetHealth = currentTarget.GetComponentInParent<MonsterHealth>();

            shortEnemy = currentTarget;
            shortEnemyObject = currentTarget.gameObject;
        }
        else
        {
            ClearCurrentTarget();
        }
    }

    bool IsTargetStillValid(Collider2D target)
    {
        if (target == null)
            return false;

        if (!target.gameObject.activeInHierarchy)
            return false;

        var mh = target.GetComponentInParent<MonsterHealth>();
        if (mh == null)
            return false;

        if (!mh.IsTargetable)
            return false;
        if (!IsInsideDiamond(target))
            return false;

        return true;
    }

    bool IsInsideDiamond(Collider2D target)
    {
        Vector2 center = centerObject.transform.position;

        Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
        Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
        Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
        Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

        Vector2[] diamond = new Vector2[] { top, right, bottom, left };

        Vector2 p = target.ClosestPoint(center);
        return IsPointInPolygon(p, diamond);
    }

    void ClearCurrentTarget()
    {
        currentTarget = null;
        currentTargetHealth = null;
        shortEnemy = null;
        shortEnemyObject = null;
    }

    private void OnDrawGizmos()
    {
        Vector2 center = centerObject.transform.position;

        Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
        Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
        Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
        Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

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