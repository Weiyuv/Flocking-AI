
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AIFloking : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 3f;
    public float maxSpeed = 5f;
    public float maxForce = 8f;
    public float rotationSpeed = 5f;

    [Header("Percepcao")]
    public float neighborRadius = 5f;
    public float separationRadius = 1.5f;
    public LayerMask boidLayer;

    [Header("Pesos")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;

    [Header("Limites do mundo")]
    public float worldRadius = 25f;
    public float boundaryWeight = 3f;

    [Header("Obstaculos")]
    public LayerMask obstacleLayer;
    public float obstacleDetectionDistance = 3f;
    public float obstacleAvoidanceWeight = 5f;
    public float obstacleSphereRadius = 0.5f;

    private Rigidbody body;
    private AIFlokingGrid spatialGrid;
    private Vector3 steering;

    internal Rigidbody Body => body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();

        body.useGravity = false;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
    }

    void OnEnable()
    {
        RegisterWithGrid();
    }

    void Start()
    {
        RegisterWithGrid();
    }

    void OnDisable()
    {
        if (spatialGrid != null)
            spatialGrid.Unregister(this);

        spatialGrid = null;
    }

    void RegisterWithGrid()
    {
        if (spatialGrid != null)
            return;

        spatialGrid = AIFlokingGrid.Instance;

        if (spatialGrid != null)
            spatialGrid.Register(this);
    }

    void FixedUpdate()
    {
        if (body == null)
            return;

        if (spatialGrid == null)
            RegisterWithGrid();

        Vector3 position = body.position;
        Vector3 velocity = body.linearVelocity;

        if (!IsFinite(position) || !IsFinite(velocity))
        {
            body.linearVelocity = transform.forward * speed;
            body.position = Vector3.zero;
            return;
        }

        steering = Vector3.zero;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 center = Vector3.zero;

        int neighborCount = 0;
        int separationCount = 0;

        if (spatialGrid != null)
        {
            Vector3Int currentCell = spatialGrid.WorldToCell(position);

            int cellRange = Mathf.CeilToInt(
                neighborRadius / Mathf.Max(0.1f, spatialGrid.cellSize)
            );

            float neighborRadiusSquared = neighborRadius * neighborRadius;
            float separationRadiusSquared = separationRadius * separationRadius;

            for (int x = -cellRange; x <= cellRange; x++)
            {
                for (int y = -cellRange; y <= cellRange; y++)
                {
                    for (int z = -cellRange; z <= cellRange; z++)
                    {
                        Vector3Int cellPosition = currentCell +
                            new Vector3Int(x, y, z);

                        List<AIFloking> cell =
                            spatialGrid.GetCell(cellPosition);

                        if (cell == null)
                            continue;

                        for (int i = 0; i < cell.Count; i++)
                        {
                            AIFloking other = cell[i];

                            if (other == null || other == this ||
                                other.body == null)
                                continue;

                            if (boidLayer.value != 0 &&
                                (boidLayer.value &
                                (1 << other.gameObject.layer)) == 0)
                                continue;

                            Vector3 offset = other.body.position - position;
                            offset.y = 0f;

                            float distanceSquared = offset.sqrMagnitude;

                            if (distanceSquared < 0.0001f ||
                                distanceSquared > neighborRadiusSquared)
                                continue;

                            neighborCount++;

                            center += other.body.position;

                            Vector3 otherVelocity = other.body.linearVelocity;
                            otherVelocity.y = 0f;

                            alignment += otherVelocity;

                            if (distanceSquared < separationRadiusSquared)
                            {
                                separation -= offset / distanceSquared;
                                separationCount++;
                            }
                        }
                    }
                }
            }
        }

        if (neighborCount > 0)
        {
            Vector3 cohesion = center / neighborCount - position;
            cohesion.y = 0f;

            if (separationCount > 0)
                separation /= separationCount;

            alignment /= neighborCount;

            steering += separation * separationWeight;
            steering += SteerTowards(alignment) * alignmentWeight;
            steering += SteerTowards(cohesion) * cohesionWeight;
        }

        steering += CalculateBoundaryForce(position) * boundaryWeight;
        steering += CalculateObstacleAvoidance(velocity) *
                     obstacleAvoidanceWeight;

        if (!IsFinite(steering))
            steering = Vector3.zero;

        steering = Vector3.ClampMagnitude(steering, maxForce);

        velocity += steering * Time.fixedDeltaTime;
        velocity.y = 0f;

        float targetSpeed = Mathf.Max(0f, maxSpeed);

        if (velocity.sqrMagnitude > 0.0001f)
        {
            velocity = Vector3.ClampMagnitude(velocity, targetSpeed);
        }
        else
        {
            velocity = transform.forward * Mathf.Min(speed, targetSpeed);
        }

        if (!IsFinite(velocity))
            velocity = transform.forward * Mathf.Min(speed, targetSpeed);

        body.linearVelocity = velocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);

            body.MoveRotation(
                Quaternion.Slerp(
                    body.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                )
            );
        }
    }

    Vector3 SteerTowards(Vector3 desired)
    {
        desired.y = 0f;

        if (desired.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 desiredVelocity = desired.normalized * maxSpeed;

        Vector3 currentVelocity = body.linearVelocity;
        currentVelocity.y = 0f;

        return Vector3.ClampMagnitude(
            desiredVelocity - currentVelocity,
            maxForce
        );
    }

    Vector3 CalculateBoundaryForce(Vector3 position)
    {
        Vector3 fromCenter = position - transform.parentPositionOrZero();
        fromCenter.y = 0f;

        float radius = Mathf.Max(1f, worldRadius);

        if (fromCenter.sqrMagnitude <= radius * radius)
            return Vector3.zero;

        if (fromCenter.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return -fromCenter.normalized;
    }

    Vector3 CalculateObstacleAvoidance(Vector3 velocity)
    {
        Vector3 direction = velocity;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = transform.forward;

        direction.Normalize();

        if (Physics.SphereCast(
            body.position,
            obstacleSphereRadius,
            direction,
            out RaycastHit hit,
            obstacleDetectionDistance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore))
        {
            Vector3 reflected = Vector3.Reflect(direction, hit.normal);
            reflected.y = 0f;

            if (reflected.sqrMagnitude > 0.001f)
                return reflected.normalized - direction;
        }

        return Vector3.zero;
    }

    bool IsFinite(Vector3 value)
    {
        return
            !float.IsNaN(value.x) &&
            !float.IsNaN(value.y) &&
            !float.IsNaN(value.z) &&
            !float.IsInfinity(value.x) &&
            !float.IsInfinity(value.y) &&
            !float.IsInfinity(value.z);
    }
}

static class AIFlokingExtensions
{
    public static Vector3 parentPositionOrZero(this Transform transform)
    {
        return transform.parent != null
            ? transform.parent.position
            : Vector3.zero;
    }
}
