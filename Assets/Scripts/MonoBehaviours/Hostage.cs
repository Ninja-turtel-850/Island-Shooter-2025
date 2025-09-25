using UnityEngine;

public class Hostage : NPC
{
    [Header("Hostage Properties")]
    [SerializeField] private Transform player;
    [SerializeField] private State state;

    private enum State
    {
        Idle,
        Scared,
        Following
    }

    new void Start()
    {
        base.Start();
    }

    void FixedUpdate()
    {
        TargetPosition = player.position;
        NavigationUpdateInterval -= Time.fixedDeltaTime;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.Scared:
                break;
            case State.Following:
                if (NavigationUpdateInterval <= 0)
                {
                    SetTargetPositionAndNavigate(TargetPosition);
                    NavigationUpdateInterval = NpcType.NavigationUpdateInterval;
                }
                break;
        }
    }

    private void SwitchState(State newState)
    {
        switch (newState)
        {
            case State.Idle:
                NavMeshAgent.isStopped = true;
                break;
            case State.Scared:
                NavMeshAgent.isStopped = true;
                break;
            case State.Following:
                NavMeshAgent.isStopped = false;
                break;
        }
        state = newState;
    }
}
