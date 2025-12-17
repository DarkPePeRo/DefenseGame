// HealthBar.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MonsterHealth))]
public class HealthBar : MonoBehaviour
{
    public GameObject hpBarPrefab;
    Slider slider;

    MonsterHealth hp;

    void Awake() { hp = GetComponent<MonsterHealth>(); }

    void OnEnable()
    {
        if (slider == null && hpBarPrefab != null)
        {
            var inst = Instantiate(hpBarPrefab, transform);
            inst.transform.localPosition = new Vector3(0, 0.66f, 0);
            slider = inst.GetComponentInChildren<Slider>();
            slider.gameObject.SetActive(false);
        }
        hp.OnHpChanged += OnHpChanged;
        OnHpChanged(hp.currentHP, hp.maxHP);
    }
    void OnDisable() { hp.OnHpChanged -= OnHpChanged; }

    void OnHpChanged(float curr, float max)
    {
        if (!slider) return;
        slider.value = max <= 0 ? 0 : curr / max;
        slider.gameObject.SetActive(curr < max);
    }
}
