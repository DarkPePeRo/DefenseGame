using System.Collections;
using System.Collections.Generic;
using System.Linq; // ← 반드시 추가
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResourceRewardEffectManager : MonoBehaviour
{
    public static ResourceRewardEffectManager Instance;

    public List<ResourceRewardUIInfo> rewardUIList;
    private Dictionary<ResourceType, ResourceRewardUIInfo> rewardUIMap;

    public Transform uiCanvasTransform;
    private HashSet<ResourceType> alreadyCollected = new HashSet<ResourceType>();
    void Awake()
    {
        rewardUIMap = rewardUIList.ToDictionary(x => x.type, x => x);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMultiResourceReward(Dictionary<ResourceType, int> rewardData)
    {
        alreadyCollected.Clear(); // 시작할 때 초기화

        foreach (var kvp in rewardData)
        {
            if (!rewardUIMap.TryGetValue(kvp.Key, out var uiInfo)) continue;

            int spawnCount = Mathf.Min(kvp.Value, 15);

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject icon = Instantiate(uiInfo.iconPrefab, uiCanvasTransform);
                RectTransform rt = icon.GetComponent<RectTransform>();
                rt.position = uiInfo.startPoint.position;

                Vector2 offset = UnityEngine.Random.insideUnitCircle * 100f;
                Vector3 spreadPos = rt.position + (Vector3)offset;

                rt.DOAnchorPos(spreadPos, 0.1f)
                  .SetEase(Ease.OutQuad)
                  .OnComplete(() =>
                  {
                      StartCoroutine(MoveToTarget(rt, uiInfo.targetPoint, 0.2f + i * 0.03f, kvp.Key));
                  });
            }
        }
    }

    private IEnumerator MoveToTarget(RectTransform icon, RectTransform target, float delay, ResourceType type)
    {
        yield return new WaitForSeconds(delay);

        icon.DOMove(target.position, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                Destroy(icon.gameObject);
                PlayCollectEffect(type);
            });
    }

    private void PlayCollectEffect(ResourceType type)
    {
        if (alreadyCollected.Contains(type))
            return;

        alreadyCollected.Add(type);
        switch (type)
        {
            case ResourceType.Gold:
                PlayerCurrency.Instance.AddGoldBuffered(800);
                break;
            case ResourceType.Diamond:
                PlayerCurrency.Instance.RequestAddDiamond("stage_clear");
                break;
        }
    }
}
