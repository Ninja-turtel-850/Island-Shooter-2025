using UnityEngine;

public class PlayOnce : MonoBehaviour
{
    private Animator animator;
    private bool hasPlayed = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!hasPlayed)
        {
            animator.Play("BeginBoot2");
            hasPlayed = true;
        }
    }
}