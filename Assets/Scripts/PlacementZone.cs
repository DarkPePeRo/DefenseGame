using UnityEngine;

public class PlacementZone : MonoBehaviour
{
    private GameObject currentCharacter;
    public GameObject canvas;
    public GameObject placementzoneCanvas;
    public GameObject restartCheckUI;

    private void OnMouseDown()
    {
        GameObject selectedCharacterPrefab = CharacterSelection.Instance.selectedCharacterPrefab;

        if (selectedCharacterPrefab == null)
        {
            Debug.LogError("���õ� ĳ���� �������� �����ϴ�!");
            return;
        }

        // ���� ������ ĳ���Ͱ� �ִ� ��� ����
        if (currentCharacter != null)
        {
            Debug.Log("���� ������ ĳ���Ͱ� ����, ���� ��...");
            RemoveCharacterFromZone();
        }

        // ���ο� ĳ���͸� ���� ������ ��ġ (selectedInstance �ʱ�ȭ)
        PlaceNewCharacter(selectedCharacterPrefab);

        // ��ġ�� ĳ���� �ν��Ͻ� ��������
        GameObject selectedInstance = CharacterSelection.Instance.selectedInstance;

        // �ٸ� ������ ���õ� ĳ���Ͱ� �ִ� ��� ����
        if (selectedInstance != null)
        {
            Debug.Log("�ٸ� ������ ĳ���Ͱ� ����, ���� ��...");
            RemoveCharacterFromOtherZone(selectedInstance, (Vector2)transform.position);
        }

        // ���� �ʱ�ȭ
        FinalizeSelection();

        // UI ���� ����
        Time.timeScale = 0;
        canvas.SetActive(true);
        placementzoneCanvas.SetActive(false);
        restartCheckUI.SetActive(true);



        GameManager.Instance.DebugPlacedCharacters();
    }

    private void RemoveCharacterFromZone()
    {
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
            GameManager.Instance.PlaceCharacter(transform.position, null);
            currentCharacter = null;
        }
    }

    private void RemoveCharacterFromOtherZone(GameObject selectedInstance, Vector2 currentPlacementPosition)
    {
        // ������ �̸��� ��� ĳ���� ��ġ ��������
        var positions = GameManager.Instance.GetCharacterPositions(selectedInstance);

        foreach (var position in positions)
        {
            // ���� ��ġ ���� ��ġ�� ���ܷ� ó��
            if (position == currentPlacementPosition)
            {
                Debug.Log($"���� ��ġ ���� ��ġ�̹Ƿ� �������� �ʽ��ϴ�: {position}");
                continue;
            }

            GameObject characterToRemove = GameManager.Instance.GetPlacedCharacter(position);
            Character character = characterToRemove.GetComponent<Character>();
            CharacterManager.Instance.characters.Remove(character);
            GameManager.Instance.PlaceCharacter(position, null);
            Destroy(characterToRemove);
            Debug.Log($"�ٸ� �������� ĳ���� ���ŵ�: {position}");
        }
    }

    private void PlaceNewCharacter(GameObject selectedCharacterPrefab)
    {
        GameObject newCharacter = Instantiate(selectedCharacterPrefab, transform.position, Quaternion.identity);
        newCharacter.name = selectedCharacterPrefab.name;
        Character character = newCharacter.GetComponent<Character>();
        CharacterManager.Instance.characters.Add(character);
        currentCharacter = newCharacter;

        // GameManager�� ���ο� ĳ���� ���
        GameManager.Instance.PlaceCharacter(transform.position, currentCharacter);

        // CharacterSelection�� �ν��Ͻ� ����
        CharacterSelection.Instance.SetSelectedInstance(newCharacter);

        Debug.Log($"ĳ���� ��ġ��: {newCharacter.name} at {transform.position}");
    }

    private void FinalizeSelection()
    {
        CharacterSelection.Instance.selectedCharacterPrefab = null;
        CharacterSelection.Instance.selectedInstance = null;
        Debug.Log("���õ� ĳ���� ���� �ʱ�ȭ �Ϸ�");
    }
}
