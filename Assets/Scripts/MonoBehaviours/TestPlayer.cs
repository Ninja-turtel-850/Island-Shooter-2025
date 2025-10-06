using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPlayer : MonoBehaviour, IAmmoHolder, IDamageable
{
    // ===== AMMO & WEAPON SYSTEM =====
    private Dictionary<BulletType, int> AmmoAmount = new();
    public Gun Gun;
    public Grabber Grabber;

    [Header("UI Prompts")]
    [SerializeField] private TMPro.TextMeshProUGUI pickupPrompt;
    private Coroutine FadeInPickupRoutine;
    private Coroutine FadeOutPickupRoutine;

    // ===== HEALTH SYSTEM =====
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 7;
    [SerializeField] private float currentHealth;

    // ===== DAMAGE OVERLAY =====
    [Header("Damage Overlay Settings")]
    [SerializeField] private float maxOverlayAlpha = 0.7f;
    [SerializeField] private float startOverlayAlpha = 0.3f; // rood bij start
    private Image damageOverlay;

    void Start()
    {
        // Debugging setup
        Gun?.Pickup(transform);
        if (Gun != null)
            AmmoAmount[Gun.Type.BulletType] = 40;

        currentHealth = maxHealth;

        // Maak het rode overlay aan
        CreateDamageOverlay();
        SetOverlayAlpha(startOverlayAlpha);
    }

    void Update()
    {
        // ===== INPUT HANDLING =====
        if (Input.GetMouseButtonDown(0))
            Gun?.StartShooting();
        if (Input.GetMouseButtonUp(0))
            Gun?.StopShooting();
        if (Input.GetKeyDown(KeyCode.R))
            Gun?.Reload();

        // Pickup logic
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

        // Testdamage: toets H verlaagt health
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(10);
    }

    // ===== DAMAGE SYSTEM =====
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damage} damage, health = {currentHealth}");
        UpdateOverlay();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("💀 Player is dead!");
        // Voeg hier respawn/game over toe
    }

    // ===== OVERLAY LOGIC =====
    private void CreateDamageOverlay()
    {
        // Forceer altijd een eigen canvas in overlay mode
        GameObject canvasObj = new GameObject("DamageCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // bovenop alles
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Maak het rode beeld
        GameObject overlayObj = new GameObject("DamageOverlay");
        overlayObj.transform.SetParent(canvasObj.transform, false);
        damageOverlay = overlayObj.AddComponent<Image>();
        damageOverlay.color = new Color(1f, 0f, 0f, startOverlayAlpha);

        // Fullscreen instellen
        RectTransform rect = damageOverlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Debug.Log("✅ Damage overlay aangemaakt");
    }

    private void UpdateOverlay()
    {
        if (damageOverlay == null) return;

        float healthPercent = currentHealth / maxHealth;
        float alpha = (1f - healthPercent) * maxOverlayAlpha;

        SetOverlayAlpha(alpha);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (damageOverlay == null) return;
        Color c = damageOverlay.color;
        c.a = alpha;
        damageOverlay.color = c;
    }

    // ===== AMMO SYSTEM =====
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

    // ===== UI FADE EFFECT =====
    private IEnumerator FadeInPickupUi()
    {
        float duration = 0.3f;
        float elapsedTime = pickupPrompt.color.a * duration;
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
        float duration = 0.1f;
        float elapsedTime = (1f - pickupPrompt.color.a) * duration;
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

    // ===== DEBUG GUI =====
    void OnGUI()
    {
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

        GUI.Box(new Rect(10, y, 200, 30), $"Health: {currentHealth}", style);
    }
}
