// PathMover.cs
using UnityEngine;

public class PathMover : MonoBehaviour
{
    public MonsterDefinition def;
    public float waitFirst = 0f, waitLoop = 0f;
    public Vector2 _playerRotation = new Vector2(1, 1);

    int index; float timer;
    public Vector3 dir, norm;
    public Vector2 Facing => new(Mathf.Sign(norm.x), Mathf.Sign(norm.y));

    void OnEnable() { index = 0; timer = 0f; }

    void Update()
    {

        timer += Time.deltaTime;
        var target = Waypoints.points[0];
        dir = target.position - transform.position;
        norm = dir.normalized;

        if (dir.sqrMagnitude <= 0.04f) { if (index < Waypoints.points.Length - 1) index++; return; }

        float spd = (timer < waitFirst) ? def.moveSpeed * 0.7f : def.moveSpeed;
        if (timer < waitLoop) transform.Translate(norm * spd * Time.deltaTime, Space.World);
        else timer = 0f;
        if (def.monsterId == "WolfA")
        {
            UpdateParamsIfNeeded();
        }
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
            Debug.Log("turn");

        }
    }
}
