using UnityEngine;

public class DemoPlayer : MonoBehaviour
{
    public float speed;
    public float timer;
    public float waitingTimeF;
    public float waitingTimeL;
    public float HP = 100;

    public MultiPrefabPool objectPool;
    public PlayerCurrency currency;

    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");

    public Vector2 _playerRotation = new Vector2(1, 1);
    public Animator _animator;
    private bool _initialized;

    public Vector3 dir;
    public Vector3 normalizedDir;
    private Transform target;
    private int wavepointIndex = 0; //maximum 5

    void Start()
    {
        objectPool = GameObject.Find("PoolManager").GetComponent<MultiPrefabPool>();
        if (objectPool == null)
        {
            Debug.LogError("Object Pool not found! Please assign a PoolManager with MultiPrefabPool component.");
        }

        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator not found!");
        }
        currency = GameObject.Find("CurrencyManager").GetComponent<PlayerCurrency>();

        _initialized = true;
        target = Waypoints.points[0]; // Enemy의 target으로 WayPoint로 지정 

        _animator.SetFloat(Horizontal, 1);
        _animator.SetFloat(Vertical, 1);
    }

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
        wavepointIndex = 0;
        target = Waypoints.points[0];
        timer = 0;
        HP = 100;
        _animator.SetFloat(Horizontal, 1);
        _animator.SetFloat(Vertical, 1);
    }

    void Update()
    {
        if (!_initialized)
            return;

        timer += Time.deltaTime;

        dir = (target.position - transform.position);
        float distanceSquared = dir.sqrMagnitude; // 두 벡터 거리의 제곱 계산
        normalizedDir = dir.normalized;

        if (distanceSquared <= 0.04f) // 0.2f * 0.2f == 0.04f
        {
            GetNextWayPoint();
        }

        float currentSpeed = (timer < waitingTimeF) ? speed * 0.7f : speed;
        
        if (timer < waitingTimeL)
        {
            transform.Translate(normalizedDir * currentSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            timer = 0;
        }
        _animator.SetBool("Walk", true);
        UpdateParamsIfNeeded();
        PlayerDir();
        if(HP <= 0)
        {
            objectPool.ReturnObject(gameObject);
            currency.AddCurrency(currency.gold, 100);
        }
    }

    private void UpdateParamsIfNeeded()
    {
        if (_playerRotation.x != Mathf.Sign(normalizedDir.x) || _playerRotation.y != Mathf.Sign(normalizedDir.y))
        {
            _playerRotation.x = Mathf.Sign(normalizedDir.x);
            _playerRotation.y = Mathf.Sign(normalizedDir.y);
            _animator.SetFloat("Horizontal", _playerRotation.x);
            _animator.SetFloat("Vertical", _playerRotation.y);
        }
    }

    private void GetNextWayPoint()
    {
        if (wavepointIndex >= Waypoints.points.Length - 1)
        {
            return;
        }
        wavepointIndex++;
        target = Waypoints.points[wavepointIndex];
    }

    private void PlayerDir()
    {
        _playerRotation.x = normalizedDir.x > 0 ? 1 : -1;
        _playerRotation.y = normalizedDir.y > 0 ? 1 : -1;
    }
}
