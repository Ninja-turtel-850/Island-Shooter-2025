using UnityEngine;

[CreateAssetMenu(fileName = "BulletType", menuName = "Scriptable Objects/BulletType")]
public class BulletType : ScriptableObject
{
    public string Name = "Default Bullet";   // Name of the bullet type
    public float Range = 1000f;              // Maximum range of the bullet
    public int Damage = 1;                   // Damage dealt by the bullet
    public int Penetration = 0;              // Amount of objects the bullet can penetrate before being destroyed
    public LayerMask CollisionMask = ~0;     // Layers that the bullet can hit
    public LayerMask PenetrationMask;        // Layers that the bullet can penetrate
}
