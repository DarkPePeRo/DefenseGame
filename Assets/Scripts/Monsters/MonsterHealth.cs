using UnityEngine;

public interface IDamageable { void TakeDamage(float dmg); }

public class MonsterHealth : MonoBehaviour, IDamageable
{
    public MonsterDefinition def;
    public float maxHP;
    public float currentHP;

    public event System.Action<MonsterHealth> OnDied;
    public event System.Action<float, float> OnHpChanged;

    [SerializeField] private WaveSystem wave;
    [SerializeField] private MonsterAnimator anim;

    PathMover mover;
    Collider2D[] cols; 
    public bool IsDying { get; private set; }
    public bool IsTargetable => !IsDying && currentHP > 0;

    public void Start()
    {
        wave = GameObject.Find("WaveSystem")?.GetComponent<WaveSystem>();
        anim = GetComponent<MonsterAnimator>();

        // Ãß°¡
        mover = GetComponent<PathMover>();
        cols = GetComponentsInChildren<Collider2D>(true);
    }

    void OnEnable()
    {
        IsDying = false;

        float mul = wave != null ? wave.currentWave : 1f;

        maxHP = (def ? def.baseHP : 100f) * Mathf.Pow(1.18f, mul);

        currentHP = maxHP;
        OnHpChanged?.Invoke(currentHP, maxHP);

        SetColliders(true);
    }

    public void TakeDamage(float dmg)
    {
        if (IsDying) return;

        currentHP = Mathf.Max(0, currentHP - dmg);
        OnHpChanged?.Invoke(currentHP, maxHP);

        anim.PlayHit();

        if (currentHP <= 0)
        {
            IsDying = true;

            mover?.StopMove();

            SetColliders(false);

            anim.PlayDie(Die, 1f);
        }
    }

    void SetColliders(bool enabled)
    {
        if (cols == null) return;
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = enabled;
    }

    void Die()
    {
        OnDied?.Invoke(this);
    }
}