using UnityEngine;
using UnityEngine.AI;

public class Seeker : MonoBehaviour
{
    public Transform target;              // set by the Spawner
    public float viewDistance = 20f;
    [Range(0, 180)] public float viewAngle = 100f;
    public float eyeHeight = 1.5f;
    public float patrolSpeed = 50f, chaseSpeed = 6f;
    public float searchTime = 6f;
    public float patrolRadius = 40f;
    public LayerMask obstacleMask = ~0;   // walls, domes, etc.

    enum State { Patrol, Chase, Search }
    State state = State.Patrol;

    NavMeshAgent agent;
    Vector3 lastSeen;
    float searchTimer;

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Update()
    {
        bool sees = CanSeeTarget();

        switch (state)
        {
            case State.Patrol:
                agent.speed = patrolSpeed;
                if (sees) { state = State.Chase; break; }
                if (!agent.hasPath || agent.remainingDistance < 1f) PickPatrolPoint();
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                if (sees)
                {
                    lastSeen = target.position;
                    agent.SetDestination(lastSeen);
                }
                else
                {
                    state = State.Search;
                    searchTimer = searchTime;
                    agent.SetDestination(lastSeen);
                }
                break;

            case State.Search:
                agent.speed = patrolSpeed;
                if (sees) { state = State.Chase; break; }
                if (!agent.pathPending && agent.remainingDistance < 1f)
                {
                    searchTimer -= Time.deltaTime;
                    transform.Rotate(0, 90f * Time.deltaTime, 0); // look around
                    if (searchTimer <= 0f) { state = State.Patrol; PickPatrolPoint(); }
                }
                break;
        }
    }

    bool CanSeeTarget()
    {
        if (target == null) return false;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 goal = target.position + Vector3.up * 1f;
        Vector3 dir = goal - eye;

        if (dir.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) return false;

        // Line of sight: anything solid between us blocks it
        if (Physics.Raycast(eye, dir.normalized, out RaycastHit hit, viewDistance, obstacleMask))
            return hit.transform == target || hit.transform.IsChildOf(target);

        return false;
    }

    void PickPatrolPoint()
    {
        Vector3 random = transform.position + Random.insideUnitSphere * patrolRadius;
        if (NavMesh.SamplePosition(random, out NavMeshHit navHit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(navHit.position);
    }
}