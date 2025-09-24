using UnityEngine;

public class Hostage : NPC
{
    [Header("Hostage Properties")]
    [SerializeField] private Transform player;

    new void Start()
    {
        base.Start();
        Health = 50;
        Speed = 3.5f;
    }

    new void FixedUpdate()
    {
        base.FixedUpdate();
        TargetPosition = player.position;
    }
}
