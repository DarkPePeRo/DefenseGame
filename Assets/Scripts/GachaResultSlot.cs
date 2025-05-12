// GachaResultSlot.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GachaResultSlot : MonoBehaviour
{
    public Image icon;
    public GameObject shineOverlay;
    public TMP_Text gradeText;

    public Transform effectAnchor; // 슬롯 중앙 위치
    public GameObject rareEffectPrefab; // 외부에 있는 월드 파티클 프리팹

    public void SetupShineOnly()
    {
        icon.gameObject.SetActive(false);
        shineOverlay.SetActive(true);
        gradeText.text = "";
    }

    public void Reveal(GachaItem item)
    {
        icon.sprite = item.icon;
        icon.gameObject.SetActive(true);
        shineOverlay.SetActive(false);

        gradeText.text = GradeLabel(item.grade);

        if (item.grade >= 3 && rareEffectPrefab != null && effectAnchor != null)
        {
            GameObject fx = Instantiate(rareEffectPrefab, effectAnchor.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        if (item.grade >= 3)
        {
            transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 5, 0.5f);
        }
    }

    private string GradeLabel(int grade)
    {
        switch (grade)
        {
            case 3: return "S";
            case 4: return "SS";
            case 5: return "SSS";
            default: return "";
        }
    }
}
