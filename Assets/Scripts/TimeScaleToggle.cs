using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScaleToggle : MonoBehaviour
{
    public TextMeshProUGUI speedText; // ����� ǥ���� UI �ؽ�Ʈ

    private float[] timeScales = { 1f, 2f, 4f };
    private int currentIndex = 0;

    public void ToggleTimeScale()
    {
        currentIndex = (currentIndex + 1) % timeScales.Length;
        Time.timeScale = timeScales[currentIndex];
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Debug.Log($"���� ���: {Time.timeScale}���");

        if (speedText != null)
        {
            speedText.text = $"{Time.timeScale}";
        }
    }
    public void RestartTime()
    {
        Time.timeScale = timeScales[currentIndex];
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
