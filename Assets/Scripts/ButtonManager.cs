using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject shopUI;
    private CanvasGroup shopCanvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        shopCanvasGroup = shopUI.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnShop()
    {
        Debug.Log("Shop");
        StartCoroutine("FadeInShopUI");

    }
    public void OffShop()
    {
        StartCoroutine("FadeOutShopUI");
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
}