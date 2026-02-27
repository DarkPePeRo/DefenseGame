using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using TMPro;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public CharacterManager characterManager;
    public GameObject shopUI;
    private CanvasGroup shopCanvasGroup;
    public GameObject rankingUI;
    private CanvasGroup rankingCanvasGroup;
    public GameObject characterUI;
    private CanvasGroup characterCanvasGroup;
    public GameObject godUI;
    private CanvasGroup godCanvasGroup;
    public GameObject towerUI;
    private CanvasGroup towerCanvasGroup;
    public GameObject[] towerDetailUI;
    private CanvasGroup[] towerDetailCanvasGroup;
    public GameObject[] towerUpgradeUI;
    private CanvasGroup[] towerUpgradeCanvasGroup;
    public Sprite[] sprites;
    public GameObject BossUI;
    public GameObject Blocker;
    private Button blockerButton;
    public GameObject spoilsUI;
    private CanvasGroup spoilsCanvasGroup;
    public GameObject spoilsBtn;
    bool isBossClicked = false;

    // Start is called before the first frame update
    void Start()
    {
        shopCanvasGroup = shopUI.GetComponent<CanvasGroup>();
        rankingCanvasGroup = rankingUI.GetComponent<CanvasGroup>();
        characterCanvasGroup = characterUI.GetComponent<CanvasGroup>();
        godCanvasGroup = godUI.GetComponent<CanvasGroup>();
        towerCanvasGroup = towerUI.GetComponent<CanvasGroup>();
        spoilsCanvasGroup = spoilsUI.GetComponent<CanvasGroup>();
        // UI 배열 크기만큼 CanvasGroup 배열 생성
        towerDetailCanvasGroup = new CanvasGroup[towerDetailUI.Length];
        towerUpgradeCanvasGroup = new CanvasGroup[towerUpgradeUI.Length];

        for (int i = 0; i < towerDetailUI.Length; i++)
        {
            towerDetailCanvasGroup[i] = towerDetailUI[i].GetComponent<CanvasGroup>();
        }
        for (int i = 0; i < towerUpgradeUI.Length; i++)
        {
            towerUpgradeCanvasGroup[i] = towerUpgradeUI[i].GetComponent<CanvasGroup>();
        }
        blockerButton = Blocker.GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void BossUIColor()
    {
        if (isBossClicked) {
            isBossClicked = !isBossClicked;
            BossUI.GetComponent<Image>().sprite = sprites[0];
            BossUI.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            BossUI.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
        }
        else
        {
            isBossClicked = !isBossClicked;
            BossUI.GetComponent<Image>().sprite = sprites[1];
            BossUI.GetComponent<Image>().color = new Color(171/255f, 171/255f, 171/255f, 1);
            BossUI.GetComponent<RectTransform>().localScale = new Vector3(0.9f, 0.9f, 1);

        }
    }
    public void OnShop()
    {
        StartCoroutine(FadeInShopUI());

    }
    public void OffShop()
    {
        StartCoroutine(FadeOutShopUI());
    }
    public void OnRanking()
    {
        Debug.Log("Shop");
        StartCoroutine(FadeInRankingUI());

    }
    public void OffRanking()
    {
        StartCoroutine(FadeOutRankingUI());
    }
    public void OnCharacter()
    {
        Debug.Log("Shop");
        StartCoroutine(FadeInCharacterUI());

    }
    public void OffCharacter()
    {
        StartCoroutine(FadeOutCharacterUI());
    }
    public void OnGod()
    {
        Debug.Log("Shop");
        StartCoroutine(FadeInGodUI());
        StatUpgradeUI.Instance.UpdateUI();

    }
    public void OffGod()
    {
        StartCoroutine(FadeOutGodUI());
    }
    public void OnTower()
    {
        StartCoroutine(FadeInTowerUI());

    }
    public void OffTower()
    {
        StartCoroutine(FadeOutTowerUI());
    }
    public void OnTowerDetail(string name)
    {
        StartCoroutine(FadeInTowerDetailUI(name));

    }
    public void OffTowerDetail(string name)
    {
        StartCoroutine(FadeOutTowerDetailUI(name));
    }

    public void OnTowerUpgrade(string name)
    {
        StartCoroutine(FadeInTowerUpgradeUI(name));

    }
    public void OffTowerUpgrade(string name)
    {
        StartCoroutine(FadeOutTowerUpgradeUI(name));
    }
    public void HomeButton()
    {
        shopUI.SetActive(false);
        rankingUI.SetActive(false);
        characterUI.SetActive(false);
        godUI.SetActive(false);
        towerUI.SetActive(false);
    }

    public void OnSpoils()
    {
        StartCoroutine(FadeInSpoilsUI());
    }

    public void OffSpoils()
    {
        StartCoroutine(FadeOutSpoilsUI());
    }

    private IEnumerator FadeInShopUI()
    {
        if (shopCanvasGroup != null)
        {
            shopUI.SetActive(true); // UI 활성화
            float duration = 0.2f; // 페이드 인 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                shopCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutShopUI()
    {
        if (shopCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                shopCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            shopUI.SetActive(false); // UI 비활성화
        }
    }
    private IEnumerator FadeInRankingUI()
    {
        if (rankingCanvasGroup != null)
        {
            rankingUI.SetActive(true); // UI 활성화
            float duration = 0.2f; // 페이드 인 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rankingCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutRankingUI()
    {
        if (rankingCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rankingCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            rankingUI.SetActive(false); // UI 비활성화
        }
    }
    private IEnumerator FadeInCharacterUI()
    {
        if (characterCanvasGroup != null)
        {
            characterUI.SetActive(true); // UI 활성화
            float duration = 0.2f; // 페이드 인 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutCharacterUI()
    {
        if (characterCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            characterUI.SetActive(false); // UI 비활성화
        }
    }
    private IEnumerator FadeInGodUI()
    {
        if (godCanvasGroup != null)
        {
            godUI.SetActive(true); // UI 활성화
            float duration = 0.2f; // 페이드 인 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                godCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutGodUI()
    {
        if (godCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                godCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            godUI.SetActive(false); // UI 비활성화
        }
    }
    private IEnumerator FadeInTowerUI()
    {
        if (towerCanvasGroup != null)
        {
            towerUI.SetActive(true); // UI 활성화
            float duration = 0.2f; // 페이드 인 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                towerCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutTowerUI()
    {
        if (towerCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                towerCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            towerUI.SetActive(false); // UI 비활성화
        }
    }
    private IEnumerator FadeInTowerDetailUI(string name)
    {
        int a = 0;
        switch (name)
        {
            case "house":
                a = 0;
                break;
            case "mill":
                a = 1;
                break;
            
        }
        if (towerDetailCanvasGroup[a] != null)
        {
            towerDetailUI[a].SetActive(true);
            Blocker.SetActive(true);

            blockerButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            blockerButton.onClick.AddListener(() => OffTowerDetail(name));

            float duration = 0.3f;
            float elapsed = 0f;

            RectTransform rect = towerDetailUI[a].GetComponent<RectTransform>();
            Vector2 targetPos = Vector2.zero; // 원래 위치
            Vector2 startPos = targetPos + new Vector2(100f, 0f); // 오른쪽에서 등장

            rect.anchoredPosition = startPos;
            towerDetailCanvasGroup[a].alpha = 0f;

            // 부드러운 이동 + 튕김 효과 수동 구현
            AnimationCurve slideCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.8f, 1.1f),
                new Keyframe(1f, 1f)
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float curvedT = slideCurve.Evaluate(t);

                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, curvedT);
                towerDetailCanvasGroup[a].alpha = t;

                yield return null;
            }

            // 보정 (정확한 위치/알파 설정)
            rect.anchoredPosition = targetPos;
            towerDetailCanvasGroup[a].alpha = 1f;
        }
    }

    private IEnumerator FadeOutTowerDetailUI(string name)
    {
        int a = 0;
        switch (name)
        {
            case "house":
                a = 0;
                break;
            case "mill":
                a = 1;
                break;

        }
        if (towerDetailCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                towerDetailCanvasGroup[a].alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            towerDetailUI[a].SetActive(false); // UI 비활성화
            Blocker.SetActive(false);
            blockerButton.onClick.RemoveAllListeners();
        }
    }
    private IEnumerator FadeInSpoilsUI()
    {
        if (spoilsCanvasGroup != null)
        {
            spoilsUI.SetActive(true); // UI 활성화
            spoilsBtn.SetActive(false); // 버튼 비활성화
            float duration = 0.3f;
            float elapsed = 0f;

            RectTransform rect = spoilsUI.GetComponent<RectTransform>(); 
            Vector2 targetPos = rect.anchoredPosition; // 현재 위치를 기준으로
            Vector2 startPos = targetPos + new Vector2(100f, 0f); // 오른쪽에서 등장

            rect.anchoredPosition = startPos;
            spoilsCanvasGroup.alpha = 0f;

            // 부드러운 이동 + 튕김 효과 수동 구현
            AnimationCurve slideCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.8f, 1.1f),
                new Keyframe(1f, 1f)
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float curvedT = slideCurve.Evaluate(t);

                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, curvedT);
                spoilsCanvasGroup.alpha = t;

                yield return null;
            }

            // 보정 (정확한 위치/알파 설정)
            rect.anchoredPosition = targetPos;
            spoilsCanvasGroup.alpha = 1f;
        }
    }
    private IEnumerator FadeInTowerUpgradeUI(string name)
    {
        int a = 0;
        switch (name)
        {
            case "house":
                a = 0;
                break;
            case "mill":
                a = 1;
                break;

        }
        if (towerUpgradeCanvasGroup[a] != null)
        {
            towerUpgradeUI[a].SetActive(true);
            Blocker.SetActive(true);

            blockerButton.onClick.AddListener(() => OffTowerUpgrade(name));

            float duration = 0.3f;
            float elapsed = 0f;

            RectTransform rect = towerUpgradeUI[a].GetComponent<RectTransform>();
            Vector2 targetPos = Vector2.zero; // 원래 위치
            Vector2 startPos = targetPos + new Vector2(100f, 0f); // 오른쪽에서 등장

            rect.anchoredPosition = startPos;
            towerUpgradeCanvasGroup[a].alpha = 0f;

            // 부드러운 이동 + 튕김 효과 수동 구현
            AnimationCurve slideCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.8f, 1.1f),
                new Keyframe(1f, 1f)
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float curvedT = slideCurve.Evaluate(t);

                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, curvedT);
                towerUpgradeCanvasGroup[a].alpha = t;

                yield return null;
            }

            // 보정 (정확한 위치/알파 설정)
            rect.anchoredPosition = targetPos;
            towerUpgradeCanvasGroup[a].alpha = 1f;
        }
    }
    IEnumerator FadeOutTowerUpgradeUI(string name)
    {
        int a = 0;
        switch (name)
        {
            case "house":
                a = 0;
                break;
            case "mill":
                a = 1;
                break;

        }
        if (towerUpgradeCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            towerDetailUI[a].SetActive(false); // UI 비활성화
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                towerUpgradeCanvasGroup[a].alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            towerUpgradeUI[a].SetActive(false);
            Blocker.SetActive(false);
            blockerButton.onClick.RemoveAllListeners();
        }
    }
    private IEnumerator FadeOutSpoilsUI()
    {
        if (spoilsCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                spoilsCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            spoilsUI.SetActive(false); // UI 비활성화
            spoilsBtn.SetActive(true); // 버튼 활성화
        }
    }
}
