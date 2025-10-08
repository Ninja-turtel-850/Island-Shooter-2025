using UnityEngine;
using System.Collections;

public class CutsceneSwitcher : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject player;           // Je player object
    public Camera playerCamera;         // De camera van de speler
    public Camera cutsceneCamera;       // De cutscene camera
    public float cutsceneDuration = 15f; // Lengte van de cutscene in seconden

    void Start()
    {
        // Begin cutscene
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        // Speler en player camera uit
        player.SetActive(false);
        playerCamera.enabled = false;

        // Cutscene camera aan
        cutsceneCamera.enabled = true;

        // Wacht 15 seconden
        yield return new WaitForSeconds(cutsceneDuration);

        // Wissel naar speler
        cutsceneCamera.enabled = false;
        player.SetActive(true);
        playerCamera.enabled = true;
    }
}
