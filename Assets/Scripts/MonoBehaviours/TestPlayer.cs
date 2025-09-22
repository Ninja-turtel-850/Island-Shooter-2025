using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPlayer : MonoBehaviour, IAmmoHolder, IDamageable
{
    private Dictionary<BulletType, int> AmmoAmount = new();
    public Gun Gun;
    public Grabber Grabber;

    [SerializeField] private TMPro.TextMeshProUGUI pickupPrompt;
    private Coroutine FadeInPickupRoutine;
    private Coroutine FadeOutPickupRoutine;

    void Start()
    {
        // Debuging only, should be removed later
        Gun.Pickup(transform);
        AmmoAmount[Gun.Type.BulletType] = 40;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Gun.StartShooting();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Gun.StopShooting();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Gun.Reload();
        }

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
                pickupable.Pickup(transform);
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
