using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject shopUI;
    private CanvasGroup shopCanvasGroup;
    public GameObject rankingUI;
    private CanvasGroup rankingCanvasGroup;
    public GameObject characterUI;
    private CanvasGroup characterCanvasGroup;
    public GameObject characterDetailUI;
    private CanvasGroup characterDetailCanvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        shopCanvasGroup = shopUI.GetComponent<CanvasGroup>();
        rankingCanvasGroup = rankingUI.GetComponent<CanvasGroup>();
        characterCanvasGroup = characterUI.GetComponent<CanvasGroup>();
        characterDetailCanvasGroup = characterDetailUI.GetComponent<CanvasGroup>();
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
    public void OnCharacterDetail()
    {
        Debug.Log("Shop");
        StartCoroutine(FadeInCharacterDetailUI());

    }
    public void OffCharacterDetail()
    {
        StartCoroutine(FadeOutCharacterDetailUI());
    }

    private IEnumerator FadeInShopUI()
    {
        if (shopCanvasGroup != null)
        {
            shopUI.SetActive(true); // UI Ȱ��ȭ
            float duration = 0.2f; // ���̵� �� ���� �ð�
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
            float duration = 0.2f; // ���̵� �ƿ� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                shopCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            shopUI.SetActive(false); // UI ��Ȱ��ȭ
        }
    }
    private IEnumerator FadeInRankingUI()
    {
        if (rankingCanvasGroup != null)
        {
            rankingUI.SetActive(true); // UI Ȱ��ȭ
            float duration = 0.2f; // ���̵� �� ���� �ð�
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
            float duration = 0.2f; // ���̵� �ƿ� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rankingCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            rankingUI.SetActive(false); // UI ��Ȱ��ȭ
        }
    }
    private IEnumerator FadeInCharacterUI()
    {
        if (characterCanvasGroup != null)
        {
            characterUI.SetActive(true); // UI Ȱ��ȭ
            float duration = 0.2f; // ���̵� �� ���� �ð�
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
            float duration = 0.2f; // ���̵� �ƿ� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            characterUI.SetActive(false); // UI ��Ȱ��ȭ
        }
    }
    private IEnumerator FadeInCharacterDetailUI()
    {
        if (characterDetailCanvasGroup != null)
        {
            characterDetailUI.SetActive(true); // UI Ȱ��ȭ
            float duration = 0.2f; // ���̵� �� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterDetailCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutCharacterDetailUI()
    {
        if (characterDetailCanvasGroup != null)
        {
            float duration = 0.2f; // ���̵� �ƿ� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterDetailCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            characterDetailUI.SetActive(false); // UI ��Ȱ��ȭ
        }
    }
}
