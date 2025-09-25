using UnityEngine;

[CreateAssetMenu(fileName = "NPCType", menuName = "Scriptable Objects/NPCType")]
public class NPCType : ScriptableObject
{
    [Header("NPC Attributes")]
    public float WalkSpeed;
    public float RunSpeed;
    public int Health;
    public float NavigationUpdateInterval;
    public LayerMask Player;
}
