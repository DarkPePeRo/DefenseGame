using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    public float speed;
    public float timer;
    public float waitingTimeF;
    public float waitingTimeL;
    public float MaxHP;
    public float CurrentHP;

    public MultiPrefabPool objectPool;
    public PlayerCurrency currency;
    private MonsterStat monsterStat;
    public Spawn spawn;
    public MonsterStatsLoader monsterStatsLoader;
    public WaveSystem waveSystem;

    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");

    public Vector2 _playerRotation = new Vector2(1, 1);
    public Animator _animator;
    private bool _initialized;

    public Vector3 dir;
    public Vector3 normalizedDir;
    private Transform target;
    private int wavepointIndex = 0; //maximum 5
    private void Awake()
    {
        objectPool = GameObject.Find("PoolManager")?.GetComponent<MultiPrefabPool>();
        currency = GameObject.Find("CurrencyManager")?.GetComponent<PlayerCurrency>();
        spawn = GameObject.Find("Spawn")?.GetComponent<Spawn>();
        monsterStatsLoader = GameObject.Find("StatLoader")?.GetComponent<MonsterStatsLoader>();
        _animator = GetComponent<Animator>();
        waveSystem = GameObject.Find("WaveSystem")?.GetComponent<WaveSystem>();
    }
    void Start()
    {
        _initialized = true;
        target = Waypoints.points[0]; // Enemy의 target으로 WayPoint로 지정 
        Initialize("SkeletonBoss", monsterStatsLoader);
        // 웨이브별 체력과 골드 설정 적용
        float healthMultiplier = waveSystem.GetHealthMultiplier();
        MaxHP = (monsterStat != null ? monsterStat.hp : MaxHP) * healthMultiplier; // 웨이브별 체력 배수 적용
        CurrentHP = MaxHP;
    }

    private void OnEnable()
    {
        wavepointIndex = 0;
        target = Waypoints.points[0];
        timer = 0;
        _animator.SetFloat(Horizontal, 1);
        _animator.SetFloat(Vertical, 1);
        Initialize("SkeletonBoss", monsterStatsLoader);
        // 웨이브별 체력과 골드 설정 적용
        float healthMultiplier = waveSystem.GetHealthMultiplier();
        MaxHP = (monsterStat != null ? monsterStat.hp : MaxHP) * healthMultiplier; // 웨이브별 체력 배수 적용
        CurrentHP = MaxHP;

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
        if (CurrentHP > 0)
        {
        }
        else
        {
            Destroy(gameObject);
            currency.AddGoldBuffered(waveSystem.GetGoldReward());
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
    public void Initialize(string monsterName, MonsterStatsLoader statsLoader)
    {
        // JSON을 다시 읽지 않고 캐시된 데이터에서 스탯을 가져옴
        monsterStat = statsLoader.GetMonsterStatByName(monsterName);
        if (monsterStat != null)
        {
            Debug.Log($"Initialized Monster: {monsterStat.name} - HP: {MaxHP}, Attack: {monsterStat.attack}, Defense: {monsterStat.defense}");
        }
        else
        {
            Debug.LogError("Monster stat not found for " + monsterName);
        }
    }
    public void SetStats(MonsterStat stat)
    {
        monsterStat = stat;
        MaxHP = monsterStat.hp * waveSystem.GetHealthMultiplier(); // HP를 설정
        Debug.Log($"Monster stats set: {monsterStat.name} - HP: {MaxHP}, Attack: {monsterStat.attack}, Defense: {monsterStat.defense}");
    }
}
