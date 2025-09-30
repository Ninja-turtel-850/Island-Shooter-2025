using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Scriptable Objects/EnemyType")]
public class EnemyType : ScriptableObject
{
    [Header("Enemy Properties")]
    public float LookInterval;                  // Time interval to look for the player
    public float AlertDuration;                 // How long does the enemy spend in alert state before going back to idle

    [Header("Vision Settings")]
    public float VisionRange;                   // Max lenght of the raycasts
    public float VisionMaxVertical;             // Max vertical angle the enemy can see
    public float VisionMaxHorizontal;           // Max horizontal angle the enemy can see
    public LayerMask VisibleLayers;             // All layers the enemy can see

}
