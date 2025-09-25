using UnityEngine;

public class Enemy : NPC
{
    [Header("Enemy Properties")]
    [SerializeField] protected EnemyType enemyType;
    [SerializeField] protected Gun gun;
    [SerializeField] protected State state;
    [SerializeField] protected Vector3 Target;
    [SerializeField] protected float LookInterval;
    [SerializeField] protected float AlertTimer;
    [SerializeField] protected Vector3 home;                               // the spawnpoint, position of the enemy when the scene is loaded
    [SerializeField] protected Vector3 homeRotation;                       // the spawnpoint, rotation of the enemy when the scene is loaded
    [SerializeField] protected bool SeesPlayer;
    protected bool IsAtNavigationPoint { get { return NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance; } }

    protected enum State
    {
        Idle,
        Alert,
        Chase
    }

    new protected void Start()
    {
        base.Start();
        home = transform.position;
        homeRotation = transform.eulerAngles;
        TargetPosition = home;
        state = State.Idle;
    }

    protected void FixedUpdate()
    {
        NavigationUpdateInterval -= Time.fixedDeltaTime;
        LookInterval -= Time.fixedDeltaTime;
        if (state == State.Alert && IsAtNavigationPoint)
            AlertTimer -= Time.fixedDeltaTime;
    }

    protected bool LookForPlayer(out Vector3? playerPosition)
    {
        LookInterval = enemyType.LookInterval;
        // Create the rays
        playerPosition = null;
        bool foundPlayer = false;
        float rayDistanceX = enemyType.VisionRays.x / enemyType.VisionMaxHorizontal;
        float rayDistanceY = enemyType.VisionRays.y / enemyType.VisionMaxVertical;
        RaycastHit cast;
        for (int i = 0; i < enemyType.VisionRays.y; i++) // verical rays
        {
            for (int j = 0; j < enemyType.VisionRays.x; j++) // horizontal rays
            {
                Vector3 rayDirection = transform.forward + (transform.right * (j - (int)(enemyType.VisionRays.x / 2)) / rayDistanceX) + (transform.up * (i - (int)(enemyType.VisionRays.y / 2)) / rayDistanceY);
                foundPlayer = Physics.Raycast(transform.position, rayDirection, out cast, enemyType.VisionRange, NpcType.Player);
                if (foundPlayer)
                {
                    playerPosition = cast.transform.position;
                    break;
                }
            }
            if (foundPlayer)
                break;
        }
        return foundPlayer;
    }

    protected void OnDrawGizmos()
    {
        if (enemyType == null) return;

        float rayDistanceX = enemyType.VisionRays.x / enemyType.VisionMaxHorizontal;
        float rayDistanceY = enemyType.VisionRays.y / enemyType.VisionMaxVertical;

        Gizmos.color = Color.red;

        for (int i = 0; i < enemyType.VisionRays.y; i++) // vertical rays
        {
            for (int j = 0; j < enemyType.VisionRays.x; j++) // horizontal rays
            {
                Vector3 rayDirection = transform.forward + (transform.right * (j - (int)(enemyType.VisionRays.x / 2)) / rayDistanceX) + (transform.up * (i - (int)(enemyType.VisionRays.y / 2)) / rayDistanceY);
                Gizmos.DrawRay(transform.position, rayDirection.normalized * enemyType.VisionRange);
            }
        }
    }
}
