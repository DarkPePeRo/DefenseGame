using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MonsterHealth))]
public class HealthBar : MonoBehaviour
{
    public MonsterDefinition def;

    [Header("Normal 몬스터용")]
    public GameObject hpBarPrefab;

    Slider slider;
    MonsterHealth hp;

    void Awake() { hp = GetComponent<MonsterHealth>(); }

    void OnEnable()
    {
        if (def != null && def.monsterId == "BossA")
        {
            if (BossHpHud.Instance != null)
                BossHpHud.Instance.Bind(hp);

            return;
        }

        if (slider == null && hpBarPrefab != null)
        {
            var inst = Instantiate(hpBarPrefab, transform);
            inst.transform.SetAsFirstSibling();
            if(def.monsterId == "WolfA")
            {
                inst.transform.localPosition = new Vector3(0, 4f, 0);
            }
            else
            {
                inst.transform.localPosition = new Vector3(0, 0.66f, 0);
            }
                slider = inst.GetComponentInChildren<Slider>(true);
            if (slider) slider.gameObject.SetActive(false);
            Debug.Log("slider");
        }

        hp.OnHpChanged += OnHpChanged;
        OnHpChanged(hp.currentHP, hp.maxHP);
    }

    void OnDisable()
    {
        if (hp != null)
            hp.OnHpChanged -= OnHpChanged;

        if (def != null && def.monsterId == "BossA")
        {
            if (BossHpHud.Instance != null)
                BossHpHud.Instance.Unbind();
        }
    }

    void OnHpChanged(float curr, float max)
    {
        if (!slider) return;

        slider.value = max <= 0 ? 0 : curr / max;
        slider.gameObject.SetActive(curr < max);
    }
}