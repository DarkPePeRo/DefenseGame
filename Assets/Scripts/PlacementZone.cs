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
            Debug.LogError("선택된 캐릭터 프리팹이 없습니다!");
            return;
        }

        // 현재 구역에 캐릭터가 있는 경우 제거
        if (currentCharacter != null)
        {
            Debug.Log("현재 구역에 캐릭터가 있음, 제거 중...");
            RemoveCharacterFromZone();
        }

        // 새로운 캐릭터를 현재 구역에 배치 (selectedInstance 초기화)
        PlaceNewCharacter(selectedCharacterPrefab);

        // 배치된 캐릭터 인스턴스 가져오기
        GameObject selectedInstance = CharacterSelection.Instance.selectedInstance;

        // 다른 구역에 선택된 캐릭터가 있는 경우 제거
        if (selectedInstance != null)
        {
            Debug.Log("다른 구역에 캐릭터가 있음, 제거 중...");
            RemoveCharacterFromOtherZone(selectedInstance, (Vector2)transform.position);
        }

        // 참조 초기화
        FinalizeSelection();

        // UI 상태 변경
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
        // 동일한 이름의 모든 캐릭터 위치 가져오기
        var positions = GameManager.Instance.GetCharacterPositions(selectedInstance);

        foreach (var position in positions)
        {
            // 현재 배치 중인 위치는 예외로 처리
            if (position == currentPlacementPosition)
            {
                Debug.Log($"현재 배치 중인 위치이므로 제거하지 않습니다: {position}");
                continue;
            }

            GameObject characterToRemove = GameManager.Instance.GetPlacedCharacter(position);
            Character character = characterToRemove.GetComponent<Character>();
            CharacterManager.Instance.characters.Remove(character);
            GameManager.Instance.PlaceCharacter(position, null);
            Destroy(characterToRemove);
            Debug.Log($"다른 구역에서 캐릭터 제거됨: {position}");
        }
    }

    private void PlaceNewCharacter(GameObject selectedCharacterPrefab)
    {
        GameObject newCharacter = Instantiate(selectedCharacterPrefab, transform.position, Quaternion.identity);
        newCharacter.name = selectedCharacterPrefab.name;
        Character character = newCharacter.GetComponent<Character>();
        CharacterManager.Instance.characters.Add(character);
        currentCharacter = newCharacter;

        // GameManager에 새로운 캐릭터 기록
        GameManager.Instance.PlaceCharacter(transform.position, currentCharacter);

        // CharacterSelection에 인스턴스 저장
        CharacterSelection.Instance.SetSelectedInstance(newCharacter);

        Debug.Log($"캐릭터 배치됨: {newCharacter.name} at {transform.position}");
    }

    private void FinalizeSelection()
    {
        CharacterSelection.Instance.selectedCharacterPrefab = null;
        CharacterSelection.Instance.selectedInstance = null;
        Debug.Log("선택된 캐릭터 참조 초기화 완료");
    }
}
