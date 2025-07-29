using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "CharacterSO", order = 0)]
public class CharacterSO : ScriptableObject
{
    public int level;
    public int maxXP => 30 + 30 * level * level;
    public int maxPray;
    // public RuntimeAnimatorController animatorController;
    public Sprite characterName;
    public Sprite sprite;
}
