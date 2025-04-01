using System.Collections.Generic;
using UnityEngine;
public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;

    public List<Character> availableCharacters;
    public int selectedIndex { get; private set; } = -1;
    public Character selectedCharacter { get; set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // CharacterSelection.cs
    public void LoadAvailableCharacters()
    {
        availableCharacters = CharacterManager.Instance.GetAllCharacters();
    }
    public Character SelectCharacter(int index)
    {
        if (index < 0 || index >= availableCharacters.Count) return null;

        selectedIndex = index;
        selectedCharacter = availableCharacters[index];
        return selectedCharacter;
    }


    public Character GetSelectedCharacter()
    {
        if (selectedIndex >= 0 && selectedIndex < availableCharacters.Count)
        {
            return availableCharacters[selectedIndex];
        }
        return null;
    }
}