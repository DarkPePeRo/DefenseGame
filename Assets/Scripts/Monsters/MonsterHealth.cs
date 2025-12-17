// MonsterHealth.cs
using UnityEngine;

public interface IDamageable { void TakeDamage(float dmg); }

public class MonsterHealth : MonoBehaviour, IDamageable
{
    public MonsterDefinition def;
    public float maxHP;
    public float currentHP;

    public event System.Action<MonsterHealth> OnDied;
    public event System.Action<float, float> OnHpChanged; // curr, max

    [SerializeField] private WaveSystem wave; // 인스펙터 주입

    public void Start()
    {
        wave = GameObject.Find("WaveSystem")?.GetComponent<WaveSystem>();
    }
    void OnEnable()
    {
        float mul = wave != null ? wave.GetHealthMultiplier() : 1f;
        maxHP = (def ? def.baseHP : 100f) * mul;
        currentHP = maxHP;
        OnHpChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(float dmg)
    {
        currentHP = Mathf.Max(0, currentHP - dmg);
        OnHpChanged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0) Die();
    }

    void Die() => OnDied?.Invoke(this);
}
