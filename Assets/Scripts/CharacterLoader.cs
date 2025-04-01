using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    public string prefabPath = "Characters";
    private Dictionary<string, Character> characterMap = new();

    private void Start()
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

        PlayFabCharacterProgressService.Load(saveList =>
        {
            List<CharacterSaveData> toSave = new();

            foreach (var kvp in characterMap)
            {
                var character = kvp.Value;
                string id = character.name.ToLower();

                // JSON ���� �ε�
                string statPath = Path.Combine(Application.streamingAssetsPath, $"{character.name}Stats.json");
                if (File.Exists(statPath))
                {
                    string json = File.ReadAllText(statPath);
                    CharacterLevelData levelData = JsonUtility.FromJson<CharacterLevelData>(json);
                    character.SetLevelData(levelData);
                }

                // ����� ĳ���� ã��
                var save = saveList.Find(s => s.characterId.ToLower() == id);
                character.CurrentLevel = save != null ? Mathf.Max(save.level, 1) : 1;
                character.ApplyCurrentStats();

                // ���� �� �Ǿ� �ִٸ� ���
                if (save == null)
                {
                    toSave.Add(new CharacterSaveData { characterId = character.name, level = 1 });
                }
            }

            if (toSave.Count > 0)
                PlayFabCharacterProgressService.Save(toSave);

            CharacterManager.Instance.RefreshAllCharacterUI();
            CharacterSelection.Instance.LoadAvailableCharacters();
            PlayFabCharacterPlacementService.Load(() => { });
        });
    }
}
