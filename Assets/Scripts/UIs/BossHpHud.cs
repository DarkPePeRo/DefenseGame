// BossHpHud.cs (Canvas 하위 고정 UI에 부착)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHpHud : MonoBehaviour
{
    public static BossHpHud Instance { get; private set; }

    [Header("UI Refs")]
    public Slider slider;
    public TextMeshProUGUI hpPercentText;

    MonsterHealth _target;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Bind(MonsterHealth target)
    {
        Unbind();

        _target = target;
        if (_target == null) { Hide(); return; }

        _target.OnHpChanged += OnHpChanged;
        gameObject.SetActive(true);
        OnHpChanged(_target.currentHP, _target.maxHP);
    }

    public void Unbind()
    {
        if (_target != null)
            _target.OnHpChanged -= OnHpChanged;

        _target = null;
        Hide();
    }

    void Hide() => gameObject.SetActive(false);

    void OnHpChanged(float curr, float max)
    {
        float ratio = max <= 0 ? 0 : curr / max;
        if (slider) slider.value = ratio;

        if (hpPercentText)
        {
            int percent = Mathf.FloorToInt(ratio * 100f);
            hpPercentText.text = percent + "%";
        }
    }
}