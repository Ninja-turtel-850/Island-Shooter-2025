using UnityEngine;

[CreateAssetMenu(fileName = "GunType", menuName = "Scriptable Objects/GunType")]
public class GunType : ScriptableObject
{
    [Header("Gun Properties")]
    public int AmmoCapacity = 8;                // Maximum ammo capacity
    public float FireTime = 0.05f;              // Time it takes between shots in seconds
    public float BurstTime = 0;                 // Time it takes between bullets in a burst in seconds
    public float ReloadTime = 2;                // Time it takes to reload in seconds
    public int BulletsPerShot = 1;              // Number of bullets fired per shot
    public float Recoil = 0;                    // Recoil force applied to the gun
    public float Inacuracy = 0;                 // Inaccuracy added per shot
    public bool IsAutomatic = false;            // Is the gun automatic or not
    public bool IsBurstOneAmmo = false;         // Does each burst consume one ammo or one ammo per bullet
    public Vector3 ShootOffset;                 // The offset from the guns transform position to spawn bullets

    [Header("Bullet Properties")]
    public BulletType BulletType;               // Reference to the BulletType ScriptableObject

    [Header("3D model")]
    public Mesh Mesh;                           // The 3D model of the gun
    public Material Material;                   // The material of the gun model
    public Vector3 ModelOffset;                 // The offset to apply to the gun model when instantiated
    public Vector3 ModelRotation;               // The rotation to apply to the gun model when instantiated
    public Vector3 ModelScale = Vector3.one;    // The scale to apply to the gun model when instantiated

    [Header("VFX")]
    public GameObject FireVFX;                  // Prefab with particle system for muzzle flash
    public GameObject ShellVFX;                 // Prefab with particle system for shell ejection

    [Header("SFX")]
    public AudioClip FireSFX;                   // Sound played when firing
    public AudioClip ReloadSFX;                 // Sound played when reloading
}
