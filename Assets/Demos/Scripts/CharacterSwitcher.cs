using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField, Tooltip("Index of the character to show on enable.")]
    private int startingCharacterIndex = 0;

    [SerializeField, Tooltip("List of character data (head & body transforms).")]
    private List<CharacterData> characters = new();

    private int currentCharacterIndex;

    [ButtonGroup("Navigation", 0), Tooltip("Show the previous character in the list.")]
    public void Previous() => SwitchCharacter(currentCharacterIndex - 1);

    [ButtonGroup("Navigation", 1), Tooltip("Show the next character in the list.")]
    public void Next() => SwitchCharacter(currentCharacterIndex + 1);

    [Button, Tooltip("Switch directly to a specific character by index with wrapping.")]
    public void SwitchCharacter(int newIndex)
    {
        if (characters.Count == 0)
            return;

        int count = characters.Count;
        // Wrap index around both ends
        newIndex = ((newIndex % count) + count) % count;

        // No change
        if (newIndex == currentCharacterIndex)
            return;

        // Hide old, show new
        SetCharacterActive(currentCharacterIndex, false);
        SetCharacterActive(newIndex, true);
        currentCharacterIndex = newIndex;
    }

    public void SwitchRandomCharacter()
    {
        if (characters.Count == 0)
            return;

        SwitchCharacter(Random.Range(0, characters.Count));
    }

    [Button, Tooltip("Disable all characters in the list.")]
    private void DisableAllCharacters()
    {
        for (int i = 0; i < characters.Count; i++)
            SetCharacterActive(i, false);
    }

    private void Start()
    {
        SetCharacterActive(currentCharacterIndex, true);
    }

    private void OnEnable()
    {
        // Ensure valid starting index
        currentCharacterIndex = Mathf.Clamp(startingCharacterIndex, 0, characters.Count - 1);
        DisableAllCharacters();
    }

    /// <summary>
    /// Helper to toggle head and body active state for a character.
    /// </summary>
    private void SetCharacterActive(int index, bool active)
    {
        if (index < 0 || index >= characters.Count)
            return;

        characters[index].Head.gameObject.SetActive(active);
        characters[index].Body.gameObject.SetActive(active);
    }

    [SerializeField] private bool showSetup = false;

    // Only drawn when showSetup == true
    [ShowIf(nameof(showSetup)), BoxGroup("Setup Section")]
    [SerializeField] private List<Transform> characterHeads;

    [ShowIf(nameof(showSetup)), BoxGroup("Setup Section")]
    [SerializeField] private List<Transform> characterBodies;

    [ShowIf(nameof(showSetup)), BoxGroup("Setup Section"), Button(ButtonSizes.Large)]
    private void Setup()
    {
        int characterCount = Mathf.Min(characterHeads.Count, characterBodies.Count);
        characters.Clear();
        for (int i = 0; i < characterCount; i++)
        {
            var cd = new CharacterData(head: characterHeads[i], body: characterBodies[i]);
            characters.Add(cd);
        }
    }
}

[System.Serializable]
public struct CharacterData
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;

    public readonly Transform Head => head;
    public readonly Transform Body => body;

    public CharacterData(Transform head, Transform body)
    {
        this.head = head;
        this.body = body;
    }
}
