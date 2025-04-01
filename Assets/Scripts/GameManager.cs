using PlayFab;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform[] characterSlots = new Transform[4]; // ���� ��ġ
    public GameObject[] placedCharacters = new GameObject[4]; // ��ġ�� ĳ����

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void PlaceCharacterAtSlot(int slotIndex, GameObject character)
    {
        if (slotIndex < 0 || slotIndex >= placedCharacters.Length) return;

        // �ٸ� ���Կ� �̹� �� ĳ���Ͱ� �ִٸ� ����
        for (int i = 0; i < placedCharacters.Length; i++)
        {
            if (i != slotIndex && placedCharacters[i] == character)
            {
                placedCharacters[i].SetActive(false);
                placedCharacters[i] = null;
            }
        }

        if (placedCharacters[slotIndex] != null)
        {
            placedCharacters[slotIndex].SetActive(false);
        }

        character.transform.position = characterSlots[slotIndex].position;
        character.SetActive(true);
        placedCharacters[slotIndex] = character;
    }


    public void ClearPlacedCharacters()
    {
        for (int i = 0; i < placedCharacters.Length; i++)
        {
            if (placedCharacters[i] != null)
            {
                Destroy(placedCharacters[i]);
                placedCharacters[i] = null;
            }
        }
    }
    public void SaveAll()
    {
        Debug.Log("[GameManager] SaveAll ȣ���");

        if (LocalCharacterCache.cachedProgress.Count > 0)
            PlayFabCharacterProgressService.Save(LocalCharacterCache.cachedProgress);

        if (LocalCharacterCache.cachedPlacements.Count > 0)
        {
            var data = new PlacementSaveData { placements = LocalCharacterCache.cachedPlacements.ToArray() };
            string json = JsonUtility.ToJson(data);

            PlayFabClientAPI.UpdateUserData(new PlayFab.ClientModels.UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { "PlacedCharacters", json } }
            },
            result => Debug.Log("��ġ ���� �Ϸ�"),
            error => Debug.LogError("��ġ ���� ����: " + error.GenerateErrorReport()));
        }

        LocalCharacterCache.Clear();
    }
}
