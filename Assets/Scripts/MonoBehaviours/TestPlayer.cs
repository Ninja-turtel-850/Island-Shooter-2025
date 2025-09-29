using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour, IAmmoHolder, IDamageable
{
    private Dictionary<BulletType, int> AmmoAmount = new();
    public Gun ActiveGun { get { return Guns[CurrentGunIndex]; } }
    public Gun[] Guns = new Gun[2];
    public int CurrentGunIndex = 0;

    public Grabber Grabber;

    [SerializeField] private TMPro.TextMeshProUGUI pickupPrompt;
    private Coroutine FadeInPickupRoutine;
    private Coroutine FadeOutPickupRoutine;

    void Start()
    {

    }

    void Update()
    {
        if (ActiveGun != null)
        {
            if (Input.GetMouseButtonDown(0))
                ActiveGun.StartShooting();
            if (Input.GetMouseButtonUp(0))
                ActiveGun.StopShooting();
            if (Input.GetKeyDown(KeyCode.R))
                ActiveGun.Reload();
        }
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
            SwitchGun(0);
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
            SwitchGun(1);
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
            SwitchGun(CurrentGunIndex + (Input.GetAxis("Mouse ScrollWheel") > 0 ? 1 : -1));

        // Could probably be optimized but it's fine for a testing script
        Grabber.Position = Camera.main.transform.position;
        Grabber.Forward = Camera.main.transform.forward;
        if (Grabber.TryGetPickupable(out IPickupable pickupable))
        {
            if (FadeOutPickupRoutine != null)
            {
                StopCoroutine(FadeOutPickupRoutine);
                FadeOutPickupRoutine = null;
            }
            if (FadeInPickupRoutine == null)
                FadeInPickupRoutine = StartCoroutine(FadeInPickupUi());

            if (Input.GetKeyDown(KeyCode.E))
            {
                pickupable.Pickup(transform);
                // Check if the pickupable is a Gun
                if (pickupable is Gun gun)
                {
                    // If we already have a gun in the current slot, drop it
                    if (ActiveGun != null)
                    {
                        ActiveGun.transform.SetParent(null, true);
                        ActiveGun.Drop();
                    }
                    // Equip the new gun
                    Guns[CurrentGunIndex] = gun;
                    gun.transform.SetParent(Camera.main.transform);
                    gun.transform.localPosition = new Vector3(0.644f, -0.302f, 1.167f); // Hardcoded for now
                    gun.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
        else
        {
            if (FadeInPickupRoutine != null)
            {
                StopCoroutine(FadeInPickupRoutine);
                FadeInPickupRoutine = null;
            }
            if (FadeOutPickupRoutine == null)
                FadeOutPickupRoutine = StartCoroutine(FadeOutPickupUi());
        }
    }


    // Debuging only, should be removed later
    void OnGUI()
    {
        // Display ammo count on screen for each ammo type
        int y = 10;
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter
        };

        foreach (var ammo in AmmoAmount)
        {
            GUI.Box(new Rect(10, y, 200, 30), $"{ammo.Key.Name}: {ammo.Value}", style);
            y += 40;
        }
    }

    private void SwitchGun(int index)
    {
        if (index < 0 || index >= Guns.Length || index == CurrentGunIndex) return;

        if (ActiveGun != null) ActiveGun.gameObject.SetActive(false);
        CurrentGunIndex = index;
        if (ActiveGun != null) ActiveGun.gameObject.SetActive(true);
    }

    private IEnumerator FadeInPickupUi()
    {
        float duration = 0.3f; // Duration of the fade effect
        float elapsedTime = pickupPrompt.color.a * duration; // Start from the current alpha progress
        Color originalColor = pickupPrompt.color;
        pickupPrompt.gameObject.SetActive(true);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            pickupPrompt.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        pickupPrompt.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    private IEnumerator FadeOutPickupUi()
    {
        float duration = 0.1f; // Duration of the fade effect
        float elapsedTime = (1f - pickupPrompt.color.a) * duration; // Start from the current alpha progress
        Color originalColor = pickupPrompt.color;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            pickupPrompt.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        pickupPrompt.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        pickupPrompt.gameObject.SetActive(false);
    }

    // Implementation of IAmmoHolder interface  
    public int GetAmmo(BulletType bulletType)
    {
        if (AmmoAmount.TryGetValue(bulletType, out int ammo))
            return ammo;
        return 0;
    }

    public void RemoveAmmo(BulletType bulletType, int amount)
    {
        if (amount < 0)
            Debug.LogWarning("Removing a negative amount of ammo");

        if (AmmoAmount.ContainsKey(bulletType))
            AmmoAmount[bulletType] = Mathf.Max(AmmoAmount[bulletType] - amount, 0);
        else
            Debug.LogWarning($"Tried to remove ammo for {bulletType} but no entry exists");
    }

    public void AddAmmo(BulletType bulletType, int amount)
    {
        if (amount < 0)
            Debug.LogWarning("Adding a negative amount of ammo");

        if (AmmoAmount.ContainsKey(bulletType))
            AmmoAmount[bulletType] += amount;
        else
            AmmoAmount[bulletType] = amount;
    }

    // Implementation of IDamageable interface

    public void TakeDamage(int damage)
    {
        Debug.Log($"Player took {damage} damage");
    }
}
