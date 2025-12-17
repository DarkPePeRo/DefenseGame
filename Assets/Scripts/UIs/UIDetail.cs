using DG.Tweening;
using UnityEngine;

public class UIDetail : MonoBehaviour
{
    public RectTransform uiPanel; // 움직일 UI
    public float offscreenX = 1000f; // 시작 위치 (오른쪽 화면 밖)
    public float duration = 0.6f;

    public void OnDetail()
    {
        // 초기 위치를 오른쪽 밖으로 이동
        Vector2 startPos = uiPanel.anchoredPosition;
        startPos.x = offscreenX;
        uiPanel.anchoredPosition = startPos;

        // 튕김 애니메이션으로 슬라이드 인
        uiPanel.DOAnchorPosX(0, duration)
            .SetEase(Ease.OutBack); // ← 이게 “툭 튕기는” Ease
    }
}
