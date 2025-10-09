using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class InteractableCutscene : MonoBehaviour
{
    [Header("References")]
    public Animator animator;             // Animator van het object
    public Camera cutsceneCamera;         // Cutscene camera
    public GameObject player;             // Player object
    public Camera playerCamera;           // Player camera

    [Header("Settings")]
    public float cutsceneDuration = 10f;  // Lengte van de cutscene
    public string endScreenSceneName = "EndScreen"; // Naam van je eindscherm-scene

    private bool canInteract = false;
    private bool hasPlayed = false;

    public Vector3 CameraPosition;
    public Vector3 CameraRotation;

    void Update()
    {
        // Speler drukt op E om te interacteren
        if (canInteract && !hasPlayed && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        hasPlayed = true;

        // Speler tijdelijk uitschakelen
        player.SetActive(false);
        playerCamera.enabled = false;
        cutsceneCamera.enabled = true;
        cutsceneCamera.transform.position = CameraPosition;
        cutsceneCamera.transform.rotation = Quaternion.Euler(CameraRotation);

        // Start de animatie
        if (animator != null)
        {
            animator.SetTrigger("Play");
        }

        // Wacht tot cutscene klaar is
        yield return new WaitForSeconds(cutsceneDuration);

        // Ga naar de eindscherm-scene
        SceneManager.LoadScene(endScreenSceneName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
        }
    }
}
