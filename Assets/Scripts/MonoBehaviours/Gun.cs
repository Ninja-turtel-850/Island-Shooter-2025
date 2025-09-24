using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour, IPickupable
{
    [Header("Gun Properties")]
    [SerializeField] protected GunType GunType;             // Reference to the BulletType ScriptableObject
    [SerializeField] protected IAmmoHolder IAmmoHolder;     // Reference to the ammo holder interface

    [Header("State variables")]
    [SerializeField] protected bool ShouldShoot;            // Should the gun be shooting, only used for automatic guns
    [SerializeField] protected int CurrentAmmo;             // Current ammo left in the gun
    [SerializeField] protected float FireTimer;             // Timer to track fire rate
    [SerializeField] protected float ReloadTimer;           // Timer to track reload time
    [SerializeField] protected int BulletsToFire;           // Number of bullets left to fire in the current shot
    protected bool IsFiring { get { return FireTimer > 0; } }
    protected bool IsReloading { get { return ReloadTimer > 0; } }
    protected bool CanShoot { get { return CurrentAmmo > 0 && !IsFiring && !IsReloading; } }

    private RaycastHit[] raycastHits;                       // Array to store raycast raycastHits
    private List<RaycastHit> hitList;                       // List to store and sort raycast hits
    private Transform GunModel;                             // Transform for the gun model
    private ParticleSystem FireVFX;                         // Particle system for muzzle flash
    private ParticleSystem ShellVFX;                        // Particle system for shell ejection
    private AudioSource GunAudio;                           // Audio source for gun sounds
    private Coroutine reloadCoroutine;                      // Coroutine reference for reloading
    private Coroutine burstCoroutine;                       // Coroutine reference for burst firing

    public GunType Type { get { return GunType; } }

    private void Awake()
    {
        if (GunType == null || GunType.BulletType == null)
        {
            Debug.LogError("GunType or BulletType is not set, gun will not function");
            enabled = false;
            return;
        }

        // Initialize variables
        CurrentAmmo = GunType.AmmoCapacity;
        raycastHits = new RaycastHit[32];
        hitList = new List<RaycastHit>(32);

        // Set up the gun model
        if (GunType.Mesh != null && GunType.Material)
        {
            GunModel = new GameObject("Gun Model").transform;
            GunModel.SetParent(transform, false);
            GunModel.SetLocalPositionAndRotation(GunType.ModelOffset, Quaternion.Euler(GunType.ModelRotation));
            GunModel.localScale = GunType.ModelScale;
            MeshFilter meshFilter = GunModel.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = GunType.Mesh;
            MeshRenderer meshRenderer = GunModel.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = GunType.Material;
        }
        else
            Debug.LogWarning("GunType Mesh or Material is not set, gun will be invisible");

        // Set up Particle Systems
        if (GunType.FireVFX != null)
        {
            FireVFX = Instantiate(GunType.FireVFX, transform).GetComponent<ParticleSystem>();
            Transform fireVFXTransform = FireVFX.transform;
            fireVFXTransform.SetParent(transform, false);
            fireVFXTransform.SetLocalPositionAndRotation(transform.rotation * GunType.ShootOffset, Quaternion.identity);
        }
        else 
            Debug.LogWarning("GunType FireVFX is not set, no muzzle flash will be played");

        if (GunType.ShellVFX != null)
            ShellVFX = Instantiate(GunType.ShellVFX, transform).GetComponent<ParticleSystem>();
        else
            Debug.LogWarning("GunType ShellVFX is not set, no shell ejection will be played");

        // Set up Audio Source
        if (GunType.FireSFX != null && GunType.ReloadSFX != null)
        {
            GunAudio = gameObject.AddComponent<AudioSource>();
            GunAudio.playOnAwake = false;
            GunAudio.spatialBlend = 1; // 3D sound
            GunAudio.minDistance = 1;
            GunAudio.maxDistance = 50;
            GunAudio.volume = 1;
            GunAudio.loop = false;
        }
        else
            Debug.LogWarning("GunType FireSFX and ReloadSFX are not set, no gun sounds will be played");
    }

    private void FixedUpdate()
    {
        // Update timers
        if (FireTimer > 0) FireTimer -= Time.deltaTime;
        if (ReloadTimer > 0) ReloadTimer -= Time.deltaTime;

        // Handle automatic shooting
        if (GunType.IsAutomatic && ShouldShoot && CanShoot) Shoot();
    }

    // Call when the gun is picked up by a player or NPC
    public void Pickup(Transform owner)
    {
        // Find the IAmmoHolder component on the owner
        IAmmoHolder = owner.GetComponent<IAmmoHolder>();
        // Change the gun's layer to "PewPew" if the owner is on the "Player" layer, otherwise set it to "Default"
        if (owner.gameObject.layer == LayerMask.NameToLayer("Player"))
            GunModel.gameObject.layer = LayerMask.NameToLayer("PewPew");
        else
            GunModel.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    // Call when the gun is dropped by a player or NPC
    public void Drop()
    {
        ShouldShoot = false;
        IAmmoHolder = null;
        GunModel.gameObject.layer = LayerMask.NameToLayer("Default");

        // Stop any ongoing coroutines
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            ReloadTimer = 0;
        }
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
            BulletsToFire = 0;
            FireTimer = 0;
        }
    }

    // Call when the player or NPC decides to pull the trigger
    public void StartShooting()
    {
        // If the gun is automatic, start shooting
        if (GunType.IsAutomatic)
        {
            ShouldShoot = true;
            return;
        }
        // Check if the gun can shoot
        if (CanShoot) Shoot();
        // Reload if the gun is out of ammo
        else if (CurrentAmmo <= 0 && !IsReloading)
            Reload();
    }

    // Call when the player or NPC releases the trigger.
    // This only does something for automatic guns, but should be called regardless to avoid spaghetti code in the player/NPC scripts
    public void StopShooting()
    {
        ShouldShoot = false;
    }

    // Call to reload the gun
    public void Reload()
    {
        // Check if there is an ammo holder
        if (IAmmoHolder == null) return;

        // Check if the gun is not already reloading or firing, if the gun is already full and if there is ammo available 
        if (!IsReloading && !IsFiring && CurrentAmmo < GunType.AmmoCapacity && IAmmoHolder.GetAmmo(GunType.BulletType) > 0)
        {
            // Play reload SFX
            if (GunType.ReloadSFX != null)
                GunAudio.PlayOneShot(GunType.ReloadSFX);

            ReloadTimer = GunType.ReloadTime;
            reloadCoroutine = StartCoroutine(ReloadRoutine());
        }
    }

    // Shoot a single shot or a burst depending on the GunType
    private void Shoot()
    {
        // Play SFX and VFX
        if (GunType.FireSFX != null)
            GunAudio.PlayOneShot(GunType.FireSFX);
        if (FireVFX != null)
            FireVFX.Play();
        if (ShellVFX != null)
            ShellVFX.Play();

        // Shoot a burst or a single shot
        FireTimer = GunType.FireTime;
        if (GunType.BulletsPerShot > 1) burstCoroutine = StartCoroutine(ShootMultiple());
        else
        {
            CurrentAmmo--;
            SpawnBullet();
        }
    }

    // Shoot multiple bullets one at a time with a delay between each bullet or all at once depending on if BurstTime > 0
    private IEnumerator ShootMultiple()
    {
        BulletsToFire = GunType.BulletsPerShot;
        float burstTimer = 0f;
        while (BulletsToFire > 0)
        {
            // Stop early if out of ammo
            if (CurrentAmmo <= 0)
            {
                BulletsToFire = 0;
                break;
            }
            // Wait for the burst timer if it's still active
            if (burstTimer > 0) yield return new WaitForSeconds(burstTimer);
            // Reset the fire timer and shoot
            burstTimer = GunType.BurstTime;
            BulletsToFire--;
            // Use one ammo per bullet if the burst does not count as one ammo
            if (!GunType.IsBurstOneAmmo)
                CurrentAmmo--;
            SpawnBullet();
        }
        // If the entire burst consumes one ammo, reduce the ammo count now
        if (GunType.IsBurstOneAmmo)
            CurrentAmmo--;
    }

    // Coroutine to handle reloading
    private IEnumerator ReloadRoutine()
    {
        // Wait for the reload time
        yield return new WaitForSeconds(GunType.ReloadTime);

        // Check if there is an ammo holder to get ammo from, the gun might have been dropped while reloading.
        if (IAmmoHolder != null)
        {
            // Calculate the amount to reload
            int amountToReload = GunType.AmmoCapacity - CurrentAmmo;
            // If there is enough ammo to reload, reload to full capacity
            if (IAmmoHolder.GetAmmo(GunType.BulletType) >= amountToReload)
            {
                CurrentAmmo = GunType.AmmoCapacity;
                IAmmoHolder.RemoveAmmo(GunType.BulletType, amountToReload);
            }
            // If there is not enough ammo to reload, reload as much as possible
            else
            {
                CurrentAmmo += IAmmoHolder.GetAmmo(GunType.BulletType);
                IAmmoHolder.RemoveAmmo(GunType.BulletType, IAmmoHolder.GetAmmo(GunType.BulletType));
            }
        }
    }

    // Spawn a raycast for a bullet and handle its trajectory and collisions
    private void SpawnBullet()
    {
        // Calculate inaccuracy
        Vector2 randomCircle = Random.insideUnitCircle * GunType.Inacuracy;
        Vector3 deviation = transform.right * randomCircle.x + transform.up * randomCircle.y;
        Vector3 shootDirection = (transform.forward + deviation).normalized;

        // Calculate spawn position and rotation
        Vector3 shootPosition = transform.position + transform.rotation * GunType.ShootOffset;

        // Check for collisions using Raycast
        Ray ray = new(shootPosition, shootDirection);
        int hitCount = Physics.RaycastNonAlloc(ray, raycastHits, GunType.BulletType.Range, GunType.BulletType.CollisionMask);
        if (hitCount == 0) return;

        // Create a list of hits to sort by distance
        hitList.Clear();
        for (int i = 0; i < hitCount; i++)
            hitList.Add(raycastHits[i]);

        // Sort hitList by distance
        hitList.Sort((a, b) => a.distance.CompareTo(b.distance));

        // Process each hit
        int penetrationCount = 0;
        foreach (RaycastHit hit in hitList)
        {
            // Ignore self-collisions
            if (hit.collider.gameObject == gameObject)
                continue;
#if UNITY_EDITOR
            Debug.Log($"Bullet hit: {hit.collider.name} (Layer {hit.collider.gameObject.layer}) at {hit.point}");
#endif
            // Apply damage
            if (hit.collider.TryGetComponent<IDamageable>(out IDamageable damageable))
                damageable.TakeDamage(GunType.BulletType.Damage);

            // Check if the bullet can penetrate the object
            bool canPenetrate = (GunType.BulletType.PenetrationMask.value & (1 << hit.collider.gameObject.layer)) != 0;
            if (!canPenetrate) return;
            penetrationCount++;

            // Stop processing hits if the bullet exceeds its maximum penetration count
            if (penetrationCount > GunType.BulletType.Penetration)
                return;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (GunType == null) return;

        Vector3 origin = transform.position + transform.rotation * GunType.ShootOffset;
        int segments = 32;

        // Helper to draw a wire cone
        void DrawCone(float angle, Color color)
        {
            if (angle <= 0f) return;

            Gizmos.color = color;

            Vector3 forward = transform.forward;
            float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * GunType.BulletType.Range;
            Vector3 baseCenter = origin + forward * GunType.BulletType.Range;

            Vector3 prevPoint = baseCenter + (transform.up * radius);
            for (int i = 1; i <= segments; i++)
            {
                float theta = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 circlePoint = baseCenter +
                    (transform.up * Mathf.Cos(theta) + transform.right * Mathf.Sin(theta)) * radius;

                Gizmos.DrawLine(prevPoint, circlePoint);
                Gizmos.DrawLine(origin, circlePoint);

                prevPoint = circlePoint;
            }
        }

        // Forward reference line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + transform.forward * GunType.BulletType.Range);

        DrawCone(GunType.Inacuracy, Color.yellow);
    }
#endif
}
