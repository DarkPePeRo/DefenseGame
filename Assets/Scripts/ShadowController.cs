using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    Animator anim;

    static readonly int Attack = Animator.StringToHash("Attack");
    static readonly int Hor = Animator.StringToHash("Horizontal");
    static readonly int Ver = Animator.StringToHash("Vertical");
    static readonly int State = Animator.StringToHash("State");

    [Header("Attack")]
    public MultiPrefabPool objectPool;
    public float shotDelay = 0.5f;
    public float attackDistance = 0.8f;

    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float stopDistance = 0.65f;

    [Header("Target Search")]
    public LayerMask layer;
    public float range = 10f;
    public float tileWidth;
    public float tileHeight;
    public GameObject centerObject;

    [Header("Debug")]
    public Collider2D[] colliders;
    public Collider2D[] candidates;
    public Collider2D shortEnemy;
    public GameObject shortEnemyObject;

    private bool canBattle = false;
    private float attackTimer = 0f;

    private Collider2D currentTarget;
    private MonsterHealth currentTargetHealth;

    private Coroutine attackRoutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.SetInteger(State, 2);
    }

    private void Update()
    {
        if (!canBattle)
            return;

        UpdateTarget();

        if (currentTarget == null)
        {
            SetIdleDirection();
            return;
        }

        shortEnemy = currentTarget;
        shortEnemyObject = currentTarget.gameObject;

        Vector3 targetPos = currentTarget.transform.position;
        float distance = Vector2.Distance(transform.position, targetPos);

        FaceTarget(targetPos);

        if (distance > stopDistance)
        {
            MoveToTarget(targetPos);
            attackTimer = 0f;
            return;
        }

        TryAttack();
    }

    public void SetBattleEnabled(bool enabled)
    {
        canBattle = enabled;

        if (!enabled)
        {
            ClearCurrentTarget();
            attackTimer = 0f;

            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
        }
    }

    private void UpdateTarget()
    {
        if (!IsTargetStillValid(currentTarget))
        {
            ClearCurrentTarget();
        }

        if (currentTarget == null)
        {
            AcquireTarget();
        }
    }

    private void MoveToTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.z = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        Vector3 moveDir = dir.normalized;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        SetMoveAnimation(moveDir);
    }

    private void TryAttack()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < shotDelay)
            return;

        attackTimer = 0f;

        if (!IsTargetStillValid(currentTarget))
        {
            ClearCurrentTarget();
            return;
        }

        PlayAttack();
    }

    private void PlayAttack()
    {
        if (objectPool != null)
        {
            GameObject gumro = objectPool.GetObject("Gumro");

            if (gumro != null)
            {
                gumro.transform.position = transform.position;
            }
        }

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine(0.2f));
    }

    private void FaceTarget(Vector3 targetPos)
    {
        if (targetPos.x < transform.position.x)
            transform.localScale = new Vector3(-0.6f, 0.6f, 1f);
        else
            transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    private void SetMoveAnimation(Vector3 moveDir)
    {
        float x = Mathf.Abs(moveDir.x) < 0.01f ? 0f : Mathf.Sign(moveDir.x);
        float y = Mathf.Abs(moveDir.y) < 0.01f ? 0f : Mathf.Sign(moveDir.y);

        anim.SetFloat(Hor, x);
        anim.SetFloat(Ver, y);
    }

    private void SetIdleDirection()
    {
        // 필요하면 대기 방향 고정
        // anim.SetFloat(Hor, 0);
        // anim.SetFloat(Ver, -1);
    }

    private void AcquireTarget()
    {
        if (centerObject == null)
            return;

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

                MonsterHealth mh = c.GetComponentInParent<MonsterHealth>();
                if (mh == null)
                    continue;

                if (!mh.IsTargetable)
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

        if (colliders.Length <= 0)
        {
            ClearCurrentTarget();
            return;
        }

        currentTarget = GetNearestTarget(colliders);
        currentTargetHealth = currentTarget.GetComponentInParent<MonsterHealth>();

        shortEnemy = currentTarget;
        shortEnemyObject = currentTarget.gameObject;
    }

    private Collider2D GetNearestTarget(Collider2D[] targets)
    {
        Collider2D nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D target in targets)
        {
            if (target == null)
                continue;

            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = target;
            }
        }

        return nearest;
    }

    private bool IsTargetStillValid(Collider2D target)
    {
        if (target == null)
            return false;

        if (!target.gameObject.activeInHierarchy)
            return false;

        MonsterHealth mh = target.GetComponentInParent<MonsterHealth>();
        if (mh == null)
            return false;

        if (!mh.IsTargetable)
            return false;

        if (!IsInsideDiamond(target))
            return false;

        return true;
    }

    private bool IsInsideDiamond(Collider2D target)
    {
        if (centerObject == null)
            return false;

        Vector2 center = centerObject.transform.position;

        Vector2 top = center + new Vector2(0, tileHeight * 0.5f);
        Vector2 right = center + new Vector2(tileWidth * 0.5f, 0);
        Vector2 bottom = center - new Vector2(0, tileHeight * 0.5f);
        Vector2 left = center - new Vector2(tileWidth * 0.5f, 0);

        Vector2[] diamond = new Vector2[] { top, right, bottom, left };

        Vector2 p = target.ClosestPoint(center);
        return IsPointInPolygon(p, diamond);
    }

    private void ClearCurrentTarget()
    {
        currentTarget = null;
        currentTargetHealth = null;
        shortEnemy = null;
        shortEnemyObject = null;
    }

    private bool IsPointInPolygon(Vector2 p, Vector2[] poly)
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

    private IEnumerator AttackRoutine(float duration)
    {
        anim.ResetTrigger(Attack);
        anim.SetTrigger(Attack);

        yield return new WaitForSeconds(duration);

        attackRoutine = null;
    }

    private void OnDrawGizmos()
    {
        if (centerObject == null)
            return;

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
}