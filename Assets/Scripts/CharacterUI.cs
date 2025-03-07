using UnityEngine;
using System.Collections;

public class CharacterUI : MonoBehaviour
{
    public GameObject canvas;
    public GameObject placementzoneCanvas;
    public GameObject characterUI;
    public GameObject[] characterDetailUI;
    public CanvasGroup[] characterDetailCanvasGroup;
    public CharacterManager characterManager;


    public void Start()
    {
        // CanvasGroup 배열 크기를 characterDetailUI 배열 크기와 동일하게 초기화
        characterDetailCanvasGroup = new CanvasGroup[characterDetailUI.Length];

        for (int i = 0; i < characterDetailUI.Length; i++)
        {
            if (characterDetailUI[i] != null)
            {
                // CanvasGroup 컴포넌트 가져오기
                characterDetailCanvasGroup[i] = characterDetailUI[i].GetComponent<CanvasGroup>();

                // CanvasGroup이 없는 경우 경고 메시지 출력
                if (characterDetailCanvasGroup[i] == null)
                {
                    Debug.LogWarning($"GameObject '{characterDetailUI[i].name}'에 CanvasGroup이 없습니다. CanvasGroup을 추가해주세요.");
                }
            }
            else
            {
                Debug.LogError($"characterDetailUI[{i}]가 null입니다. 배열에 올바른 GameObject를 할당했는지 확인하세요.");
            }
        }
    }


    public void OnCharacterButtonClick(int characterIndex)
    {
        canvas.SetActive(false);
        CharacterSelection.Instance.SelectCharacter(characterIndex);
        placementzoneCanvas.SetActive(true);
        characterUI.SetActive(false);
        characterDetailUI[characterIndex].SetActive(false);
    }

    public void OnCharacterDetail(int i)
    {
        StartCoroutine(FadeInCharacterDetailUI(i));

    }
    public void OffCharacterDetail(int i)
    {
        StartCoroutine(FadeOutCharacterDetailUI(i));
    }
    private IEnumerator FadeInCharacterDetailUI(int i)
    {
        if (characterDetailCanvasGroup != null)
        {
            characterDetailUI[i].SetActive(true); // UI 활성화
            characterManager.GetCharacterInfo(CharacterSelection.Instance.availableCharacters[i].name);
            float duration = 0.2f; // 페이드 인 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterDetailCanvasGroup[i].alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutCharacterDetailUI(int i)
    {
        if (characterDetailCanvasGroup != null)
        {
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterDetailCanvasGroup[i].alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            characterDetailUI[i].SetActive(false); // UI 비활성화
        }
    }
}
