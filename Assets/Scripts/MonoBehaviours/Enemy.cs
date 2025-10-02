using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : NPC, IAmmoHolder
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
        Chase,
        Attack
    }

    new protected void Start()
    {
        base.Start();
        home = transform.position;
        homeRotation = transform.eulerAngles;
        TargetPosition = home;
        state = State.Idle;

        gun.Pickup(transform);
        gun.transform.SetParent(transform);
        gun.transform.localPosition = new Vector3(0.644f, -0.302f, 1.167f); // Hardcoded for now
        gun.transform.localRotation = Quaternion.Euler(0, 0, 0);
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
        playerPosition = null;

        // Do a simple distance check to see if the player is in range to avoid unnecessary raycasting
        if (Vector3.Distance(transform.position, Player.position) > enemyType.VisionRange)
            return false;

        // Check if the player is within the vertical and horizontal vision angles
        Vector3 directionToPlayer = (Player.position - transform.position).normalized;

        // Calculate vertical angle and project onto the vertical plane
        Vector3 directionToPlayerVertical = Vector3.ProjectOnPlane(directionToPlayer, transform.right);
        float verticalAngle = Vector3.Angle(transform.forward, directionToPlayerVertical);

        // Calculate horizontal angle and project onto the horizontal plane
        Vector3 directionToPlayerHorizontal = Vector3.ProjectOnPlane(directionToPlayer, transform.up);
        float horizontalAngle = Vector3.Angle(transform.forward, directionToPlayerHorizontal);
        if (verticalAngle > enemyType.VisionMaxVertical || horizontalAngle > enemyType.VisionMaxHorizontal)
            return false;

        // Create the raycast
        Vector3 rayDirection = (Player.position - transform.position).normalized;
        bool hit = Physics.Raycast(transform.position, rayDirection, out RaycastHit cast, enemyType.VisionRange, enemyType.VisibleLayers);
        if (hit)
        {
            if (cast.collider.CompareTag("Player"))
            {
                playerPosition = Player.position;
                return true;
            }
        }
        return false;
    }

    new protected void Die()
    {
        gun.transform.SetParent(null, true);
        gun.Drop();
        base.Die();
    }

    // IAmmoHolder implementation. Since enemies have infinite ammo, these functions do nothing :-)

    public int GetAmmo(BulletType gunType) { return gun.Type.AmmoCapacity; }
    public void RemoveAmmo(BulletType gunType, int amount) {}
    public void AddAmmo(BulletType gunType, int amount) {}

    protected void OnDrawGizmos()
    {
        if (enemyType == null) return;
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;

        // Draw sphere for vision range
        bool inRange = Vector3.Distance(transform.position, player.position) <= enemyType.VisionRange;
        if (inRange)
            Gizmos.color = new Color32(0, 0, 0, 0);
        else
            Gizmos.color = new Color32(255, 255, 0, 98);
        //Gizmos.DrawSphere(transform.position, enemyType.VisionRange);


        // Draw vision viewport
        Vector3 forward = transform.forward * enemyType.VisionRange;
        Quaternion upRayRotation = Quaternion.AngleAxis(enemyType.VisionMaxVertical, transform.right);
        Quaternion downRayRotation = Quaternion.AngleAxis(-enemyType.VisionMaxVertical, transform.right);
        Quaternion leftRayRotation = Quaternion.AngleAxis(-enemyType.VisionMaxHorizontal, transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(enemyType.VisionMaxHorizontal, transform.up);
        Vector3 upRay = upRayRotation * forward;
        Vector3 downRay = downRayRotation * forward;
        Vector3 leftRay = leftRayRotation * forward;
        Vector3 rightRay = rightRayRotation * forward;
        if (inRange)
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.gray;
        // Draw the 4 corner rays
        Gizmos.DrawRay(transform.position, (upRayRotation * leftRay).normalized * enemyType.VisionRange);
        Gizmos.DrawRay(transform.position, (upRayRotation * rightRay).normalized * enemyType.VisionRange);
        Gizmos.DrawRay(transform.position, (downRayRotation * leftRay).normalized * enemyType.VisionRange);
        Gizmos.DrawRay(transform.position, (downRayRotation * rightRay).normalized * enemyType.VisionRange);
        // Draw the 4 edge rays
        Gizmos.DrawRay(transform.position, upRay.normalized * enemyType.VisionRange);
        Gizmos.DrawRay(transform.position, downRay.normalized * enemyType.VisionRange);
        Gizmos.DrawRay(transform.position, leftRay.normalized * enemyType.VisionRange);
        Gizmos.DrawRay(transform.position, rightRay.normalized * enemyType.VisionRange);
        // Draw the 8 edges of the max vision area
        Gizmos.DrawLine(transform.position + (upRayRotation * leftRay).normalized * enemyType.VisionRange, transform.position + upRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + (upRayRotation * rightRay).normalized * enemyType.VisionRange, transform.position + upRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + (downRayRotation * leftRay).normalized * enemyType.VisionRange, transform.position + downRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + (downRayRotation * rightRay).normalized * enemyType.VisionRange, transform.position + downRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + (upRayRotation * leftRay).normalized * enemyType.VisionRange, transform.position + leftRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + (downRayRotation * leftRay).normalized * enemyType.VisionRange, transform.position + leftRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + (upRayRotation * rightRay).normalized * enemyType.VisionRange, transform.position + rightRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + (downRayRotation * rightRay).normalized * enemyType.VisionRange, transform.position + rightRay.normalized * enemyType.VisionRange);
        // Draw the forward ray
        Gizmos.DrawRay(transform.position, forward.normalized * enemyType.VisionRange);
        // Draw a line from the forward ray to the 8 max vision rays
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + (upRayRotation * leftRay).normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + (upRayRotation * rightRay).normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + (downRayRotation * leftRay).normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + (downRayRotation * rightRay).normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + upRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + downRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + leftRay.normalized * enemyType.VisionRange);
        Gizmos.DrawLine(transform.position + forward.normalized * enemyType.VisionRange, transform.position + rightRay.normalized * enemyType.VisionRange);
        
        if (!inRange) return;

        // Draw ray
        Vector3 rayDirection = (player.position - transform.position).normalized;
        
        // Check if the player is within the vertical and horizontal vision angles
        Vector3 directionToPlayer = (Player.position - transform.position).normalized;

        // Calculate vertical angle and project onto the vertical plane
        Vector3 directionToPlayerVertical = Vector3.ProjectOnPlane(directionToPlayer, transform.right);
        float verticalAngle = Vector3.Angle(transform.forward, directionToPlayerVertical);

        // Calculate horizontal angle and project onto the horizontal plane
        Vector3 directionToPlayerHorizontal = Vector3.ProjectOnPlane(directionToPlayer, transform.up);
        float horizontalAngle = Vector3.Angle(transform.forward, directionToPlayerHorizontal);

        if (verticalAngle > enemyType.VisionMaxVertical || horizontalAngle > enemyType.VisionMaxHorizontal)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
            return;
        }
        bool hit = Physics.Raycast(transform.position, rayDirection, out RaycastHit cast, enemyType.VisionRange, enemyType.VisibleLayers);
        if (hit)
        {
            if (cast.collider.CompareTag("Player"))
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.yellow;
        }
        Gizmos.DrawLine(transform.position, player.position);
    }
}
