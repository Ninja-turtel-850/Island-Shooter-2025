using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour, IDamageable
{
    [Header("NPC Properties")]
    [SerializeField] protected NPCType NpcType;
    [SerializeField] protected int Health;
    [SerializeField] protected float Speed;

    [Header("NavMesh Properties")]
    [SerializeField] protected NavMeshAgent NavMeshAgent;
    [SerializeField] protected Vector3 TargetPosition;
    [SerializeField] protected float NavigationUpdateInterval;

    private Rigidbody rb;

    protected void Start()
    {
        Health = NpcType.Health;
        Speed = NpcType.WalkSpeed;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        NavMeshAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    protected void SetTargetPositionAndNavigate(Vector3 targetPosition)
    {
        if (NavMeshAgent == null) return;
        TargetPosition = targetPosition;
        NavMeshAgent.SetDestination(TargetPosition);
        NavMeshAgent.speed = Speed;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    // IDamageable implementation
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }
}