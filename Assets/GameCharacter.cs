using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.U2D.Animation;

[CreateAssetMenu]
public class GameCharacter : ScriptableObject
{
    public string m_name;
    public SpriteLibraryAsset m_spriteLibrary;
    public AnimatorController m_spriteAnimator;
}
