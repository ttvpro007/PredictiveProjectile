using UnityEngine;

[RequireComponent(typeof(CharacterSwitcher))]
public class CharacterRandomizer : MonoBehaviour
{
    private CharacterSwitcher characterSwitcher;

    private void Awake()
    {
        characterSwitcher = GetComponent<CharacterSwitcher>();
    }

    private void OnEnable()
    {
        characterSwitcher.SwitchRandomCharacter();
    }
}
