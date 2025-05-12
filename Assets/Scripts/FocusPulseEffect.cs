using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FocusPulseEffect : MonoBehaviour
{
    public Image[] circles;
    public float delayBetween = 0.2f;
    public float pulseDuration = 1f;
    public float maxScale = 2f;

    private System.Action<FocusPulseEffect> onFinishedCallback;

    public void Play(System.Action<FocusPulseEffect> onFinish)
    {
        onFinishedCallback = onFinish;

        for (int i = 0; i < circles.Length; i++)
        {
            int index = i;
            Image circle = circles[index];

            circle.transform.localScale = Vector3.zero;
            circle.color = new Color(1, 1, 1, 1);

            DOVirtual.DelayedCall(delayBetween * index, () =>
            {
                AnimateCircle(circle, index == circles.Length - 1);
            });
        }
    }

    void AnimateCircle(Image circle, bool isLast)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(circle.transform.DOScale(maxScale, pulseDuration).SetEase(Ease.OutQuad));
        seq.Join(circle.DOFade(0f, pulseDuration));

        if (isLast)
        {
            seq.OnComplete(() =>
            {
                onFinishedCallback?.Invoke(this);
            });
        }
    }
}
