using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterLoader : MonoBehaviour
{
    public string prefabPath = "Characters";
    private Dictionary<string, Character> characterMap = new();

    private void Start()
    {
        StartCoroutine(LoadCharacters());
    }

    private IEnumerator LoadCharacters()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(prefabPath);
        foreach (var prefab in prefabs)
        {
            GameObject instance = Instantiate(prefab);
            instance.name = prefab.name;
            instance.SetActive(false);

            Character character = instance.GetComponent<Character>();
            if (character == null) continue;

            character.name = prefab.name;
            character.gameObject.name = prefab.name;

            CharacterManager.Instance.RegisterCharacter(character);
            characterMap[character.name.ToLower()] = character;
        }

        bool isDone = false;
        List<CharacterSaveData> saveList = new();

        PlayFabCharacterProgressService.Load(result => {
            saveList = result;
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);

        List<CharacterSaveData> toSave = new();

        foreach (var kvp in characterMap)
        {
            var character = kvp.Value;
            string id = character.name.ToLower();

            // JSON 스탯 로드
            string statPath = Path.Combine(Application.streamingAssetsPath, $"{character.name}Stats.json");
#if UNITY_ANDROID && !UNITY_EDITOR
            string uri = statPath;
#else
            string uri = "file://" + statPath;
#endif
            UnityWebRequest request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                CharacterLevelData levelData = JsonUtility.FromJson<CharacterLevelData>(json);
                character.SetLevelData(levelData);
            }
            else
            {
                Debug.LogError($"[{character.name}] 스탯 JSON 로딩 실패: {request.error}");
            }

            // 저장된 캐릭터 찾기
            var save = saveList.Find(s => s.characterId.ToLower() == id);
            character.CurrentLevel = save != null ? Mathf.Max(save.level, 1) : 1;
            character.ApplyCurrentStats();

            if (save == null)
            {
                toSave.Add(new CharacterSaveData { characterId = character.name, level = 1 });
            }
            else
            {
                toSave.Add(new CharacterSaveData { characterId = character.name, level = character.CurrentLevel });
            }
        }

        if (toSave.Count > 0)
            PlayFabCharacterProgressService.Save(toSave);

        CharacterManager.Instance.RefreshAllCharacterUI();
        CharacterSelection.Instance.LoadAvailableCharacters();
        PlayFabCharacterPlacementService.Load(() => { });
    }
}