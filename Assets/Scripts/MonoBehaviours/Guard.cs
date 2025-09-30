using UnityEngine;

public class Guard : Enemy
{
    [Header("Guard Properties")]
    public float MaxTurnAngle;
    public float TurnSpeed;

    new void Start()
    {
        base.Start();

        Target = home;
        SetTargetPositionAndNavigate(Target);
    }

    void Update()
    {
        // Update Navigation path
        if (NavigationUpdateInterval <= 0)
        {
            SetTargetPositionAndNavigate(Target);
            NavigationUpdateInterval = NpcType.NavigationUpdateInterval;
        }

        // Look for the player
        if (LookInterval < 0)
        {
            LookInterval = enemyType.LookInterval;
            SeesPlayer = LookForPlayer(out Vector3? seenPosition);
            if (SeesPlayer && state != State.Chase)
            {
                Target = (Vector3)seenPosition;
                SwitchState(State.Chase);
            }
            else if (!SeesPlayer && (state == State.Chase || state == State.Attack))
                SwitchState(State.Alert);
        }

        switch (state)
        {
            case State.Idle:
                IdleState();
                break;
            case State.Alert:
                AlertState();
                break;
            case State.Chase:
                ChaseState();
                break;
            case State.Attack:
                AttackState();
                break;
        }
    }

    private void IdleState()
    {
        // Check if at home position
        if (!IsAtNavigationPoint) return;

        // Turn to face home rotation and look left and right
        float angleOffset = Mathf.PingPong(Time.time * TurnSpeed, MaxTurnAngle * 2) - MaxTurnAngle;
        Vector3 newRotation = transform.eulerAngles;
        newRotation.y = homeRotation.y + angleOffset;
        transform.eulerAngles = newRotation;
    }

    private void AlertState()
    {
        // Stay alert until the last known player position is reached
        if (!IsAtNavigationPoint) return;

        // Turn to look left and right
        NavMeshAgent.updateRotation = false;
        float angleOffset = Mathf.PingPong(Time.time * TurnSpeed, MaxTurnAngle * 2) - MaxTurnAngle;
        Vector3 newRotation = transform.eulerAngles;
        newRotation.y = angleOffset;
        transform.eulerAngles = newRotation;

        // When the alert timer runs out, return to idle state
        if (AlertTimer <= 0)
        {
            NavMeshAgent.updateRotation = true;
            Target = home;
            SwitchState(State.Idle);
        }
    }

    private void ChaseState()
    {
        // If the player is in range, shoot
        float distance = Vector3.Distance(transform.position, Target);
        if (distance < enemyType.VisionRange * 0.7 && distance < gun.Type.BulletType.Range)
            SwitchState(State.Attack);
    }

    private void AttackState()
    {
        // Aim at the player
        Vector3 direction = (Target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * NavMeshAgent.angularSpeed);

        // If the gun is aimed at the player, shoot
        if (Vector3.Angle(transform.forward, direction) < gun.Type.Inacuracy)
            gun.StartShooting();
    }

    private void SwitchState(State newState)
    {
        switch (newState)
        {
            case State.Idle:
                Speed = NpcType.WalkSpeed;
                Target = home;
                NavMeshAgent.isStopped = false;
                break;
            case State.Alert:
                Speed = NpcType.RunSpeed;
                AlertTimer = enemyType.AlertDuration;
                NavMeshAgent.isStopped = false;
                break;
            case State.Chase:
                Speed = NpcType.RunSpeed;
                NavMeshAgent.isStopped = false;
                break;
            case State.Attack:
                Speed = 0;
                NavMeshAgent.isStopped = true;
                break;
        }
        state = newState;
    }

    new public void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (state == State.Idle)
        {
            SwitchState(State.Alert);
            AlertTimer = enemyType.AlertDuration;
            TargetPosition = Player.position;
            SetTargetPositionAndNavigate(TargetPosition);
        }
    }
}
