using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using TMPro;

public class ButtonManager : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        shopCanvasGroup = shopUI.GetComponent<CanvasGroup>();
        rankingCanvasGroup = rankingUI.GetComponent<CanvasGroup>();
        characterCanvasGroup = characterUI.GetComponent<CanvasGroup>();
        godCanvasGroup = godUI.GetComponent<CanvasGroup>();
        towerCanvasGroup = towerUI.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnShop()
    {
        Debug.Log("Shop");
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

    }
    public void OffGod()
    {
        StartCoroutine(FadeOutGodUI());
    }
    public void OnTower()
    {
        Debug.Log("Shop");
        StartCoroutine(FadeInTowerUI());

    }
    public void OffTower()
    {
        StartCoroutine(FadeOutTowerUI());
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
}
