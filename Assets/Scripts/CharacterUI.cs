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
        // CanvasGroup �迭 ũ�⸦ characterDetailUI �迭 ũ��� �����ϰ� �ʱ�ȭ
        characterDetailCanvasGroup = new CanvasGroup[characterDetailUI.Length];

        for (int i = 0; i < characterDetailUI.Length; i++)
        {
            if (characterDetailUI[i] != null)
            {
                // CanvasGroup ������Ʈ ��������
                characterDetailCanvasGroup[i] = characterDetailUI[i].GetComponent<CanvasGroup>();

                // CanvasGroup�� ���� ��� ��� �޽��� ���
                if (characterDetailCanvasGroup[i] == null)
                {
                    Debug.LogWarning($"GameObject '{characterDetailUI[i].name}'�� CanvasGroup�� �����ϴ�. CanvasGroup�� �߰����ּ���.");
                }
            }
            else
            {
                Debug.LogError($"characterDetailUI[{i}]�� null�Դϴ�. �迭�� �ùٸ� GameObject�� �Ҵ��ߴ��� Ȯ���ϼ���.");
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
            characterDetailUI[i].SetActive(true); // UI Ȱ��ȭ
            characterManager.GetCharacterInfo(CharacterSelection.Instance.availableCharacters[i].name);
            float duration = 0.2f; // ���̵� �� ���� �ð�
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
            float duration = 0.2f; // ���̵� �ƿ� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                characterDetailCanvasGroup[i].alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            characterDetailUI[i].SetActive(false); // UI ��Ȱ��ȭ
        }
    }
}
