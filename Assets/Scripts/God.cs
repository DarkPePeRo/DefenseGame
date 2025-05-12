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
    public Vector3 pos;
    public List<GameObject> FoundObject;
    public float radius = 0f;
    public LayerMask layer;
    public Collider2D[] colliders;
    public Collider2D shortEnemy;
    public GameObject shortEnemyObject;
    public float range = 3f; // ������ ����
    public Collider2D[] candidates;

    public float width;
    public float height;
    public float tileWidth = 5.12f;
    public float tileHeight = 2.56f;

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
            List<Collider2D> result = new List<Collider2D>();
            Vector2 center = transform.position;

            // ������ ������ 4�� ���
            Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
            Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
            Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
            Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

            // ������ ���������� ����
            Vector2[] diamond = new Vector2[] { top, right, bottom, left };

            // ���� �ĺ� ���� ���� (�̼Ҹ�Ʈ���̴��� ���� �������� ����)
            candidates = Physics2D.OverlapCircleAll(center, range, layer);
            // ���� ��ġ�� ������ �ȿ� �ִ��� �˻�
            if (candidates != null)
            {
                foreach (var c in candidates)
                {
                    if (!c.gameObject.activeInHierarchy)
                        continue;
                    Vector2 p = c.bounds.center;

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

        // ������ ������ ���
        Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
        Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
        Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
        Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

        // ������ �� �׸���
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(top, right);
        Gizmos.DrawLine(right, bottom);
        Gizmos.DrawLine(bottom, left);
        Gizmos.DrawLine(left, top);
    }


    Vector2 WorldToIsoOffset(Vector2 delta)
    {
        float isoX = (delta.x / tileWidth + delta.y / tileHeight) * 0.5f;
        float isoY = (delta.y / tileHeight - delta.x / tileWidth) * 0.5f;
        return new Vector2(isoX, isoY);
    }
    Vector2 IsoOffsetToWorld(int x, int y)
    {
        return new Vector2(
            (x - y) * tileWidth * 0.5f,
            (x + y) * tileHeight * 0.5f
        );
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
