using UnityEngine;
using UnityEngine.AI;

public class Hostage : NPC
{
    public float followDistance = 2.0f;
    public float rescueDistance = 2.0f;


    private bool isRescue = false;


    void Update()
    {
        float CheckDistance = isRescue ? followDistance : rescueDistance;
        if (Vector3.Distance(transform.position, Player.position) < CheckDistance)
        {
            SetTargetPositionAndNavigate(Player.position);
            isRescue = true;

        }
        else isRescue = false;
    }
}
