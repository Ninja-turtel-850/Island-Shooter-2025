using UnityEngine;

public class Patroler : Enemy
{
    [Header("Patroler Properties")]
    [SerializeField] private Vector3[] patrolPoints;
    [SerializeField] private int currentPatrolIndex = 0;
    [SerializeField] private bool looping;
    [SerializeField] private bool reverse;

    new void Start()
    {
        base.Start();

        // Get the enemy's height from the ground
        float height;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 20f))
            height = hit.distance;
        else
            height = 0;

        // Shoot ray down to the ground from each patrol point to find the ground position
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (Physics.Raycast(patrolPoints[i] + Vector3.up * 10, Vector3.down, out hit, 20f))
                patrolPoints[i] = hit.point + Vector3.up * height;
        }

        Target = patrolPoints[currentPatrolIndex];
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
            else if (state == State.Chase)
                SwitchState(State.Alert);
        }

        // State machine
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
        // Patrol between points
        if (IsAtNavigationPoint)
        {
            // Move to the next patrol point
            currentPatrolIndex += reverse ? -1 : 1;

            if (looping && (currentPatrolIndex >= patrolPoints.Length || currentPatrolIndex < 0))
                currentPatrolIndex = 0;
            else
            {
                // Ensure currentPatrolIndex stays within bounds
                if (currentPatrolIndex >= patrolPoints.Length)
                {
                    currentPatrolIndex = patrolPoints.Length - 1;
                    reverse = true;
                }
                else if (currentPatrolIndex < 0)
                {
                    currentPatrolIndex = 0;
                    reverse = false;
                }
            }

            Target = patrolPoints[currentPatrolIndex];
            SetTargetPositionAndNavigate(Target);
        }
    }

    private void AlertState()
    {
        // Stay alert until the last known player position is reached
        if (!IsAtNavigationPoint) return;

        // When the alert timer runs out, return to idle state
        if (AlertTimer <= 0)
        {
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
                break;
            case State.Alert:
                AlertTimer = enemyType.AlertDuration;
                Speed = NpcType.RunSpeed;
                break;
            case State.Chase:
                Speed = NpcType.RunSpeed;
                if (state == State.Idle)
                    home = transform.position;
                break;
            case State.Attack:
                Speed = 0;
                gun.StartShooting();
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

    new void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        // Draw spheres at each patrol point
        Gizmos.color = Color.green;
        foreach (var point in patrolPoints)
        {
            Gizmos.DrawSphere(point, 0.2f);
        }

        // Draw lines connecting the patrol points
        Gizmos.color = Color.blue;
        for (int i = 0; i < patrolPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
        }
    }
}
