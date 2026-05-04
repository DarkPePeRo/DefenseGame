using UnityEngine;

public class PathMover : MonoBehaviour
{
    public MonsterDefinition def;
    public float waitFirst = 0f, waitLoop = 0f;
    public Vector2 _playerRotation = new Vector2(1, 1);
    public GameObject shadow;

    int index; float timer;
    public Vector3 dir, norm;
    MonsterHealth health;

    public Vector2 Facing => new(Mathf.Sign(norm.x), Mathf.Sign(norm.y));

    public bool IsStopped { get; private set; }

    void OnEnable()
    {
        index = 0;
        timer = 0f;
        IsStopped = false;
        shadow = FindObjectOfType<ShadowController>().gameObject;
        health = GetComponent<MonsterHealth>();
    }

    public void StopMove()
    {
        IsStopped = true;
        norm = Vector3.zero;
        dir = Vector3.zero;
    }

    void Update()
    {
        timer += Time.deltaTime;

        var target = shadow.transform;

        dir = target.position - transform.position;
        norm = dir.normalized;

        if (dir.sqrMagnitude <= 0.1f)
        {
            StopMove();
            return;
        }
        else if (health.IsDying == true)
        {
            return;
        }
        else
        {
            dir = target.position - transform.position;
            norm = dir.normalized;
            IsStopped = false;
        }
            float spd = (timer < waitFirst) ? def.moveSpeed * 0.7f : def.moveSpeed;

        if (timer < waitLoop)
            transform.Translate(norm * spd * Time.deltaTime, Space.World);
        else
            timer = 0f;

        if (def.monsterId == "WolfA")
            UpdateParamsIfNeeded();
    }

    private void UpdateParamsIfNeeded()
    {
        if (_playerRotation.x != Mathf.Sign(norm.x) || _playerRotation.y != Mathf.Sign(norm.y))
        {
            _playerRotation.x = Mathf.Sign(norm.x);
            _playerRotation.y = Mathf.Sign(norm.y);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (_playerRotation.x > 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }
}