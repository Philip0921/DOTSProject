using UnityEngine;

public class AIMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 0.2f;

    [Header("Decision timing (seconds)")]
    [SerializeField] Vector2 decisionTimeRange = new Vector2(1, 8);
    [SerializeField] float decisionTimer;

    [Header("Bounds")]
    [SerializeField] Vector2 mapSize = new Vector2(70f, 40f);

    private Vector3[] moveDirections = new Vector3[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.zero, Vector3.zero };
    private int currentDirIndex;

    private void Start()
    {     
        decisionTimer = Random.Range(decisionTimeRange.x, decisionTimeRange.y);

        PickMoveDirection();
    }
    public void Init(Vector2 boundsSize, float customSpeed, Vector2 decisionRange)
    {
        mapSize = boundsSize;
        speed = customSpeed;
        decisionTimeRange = decisionRange;

        decisionTimer = Random.Range(decisionTimeRange.x, decisionTimeRange.y);
        PickMoveDirection();
    }


    // Sampler calls this once per frame for "AI thinking"
    public void TickDecision(float dt)
    {
        decisionTimer -= dt;
        if (decisionTimer <= 0f)
        {
            decisionTimer = Random.Range(decisionTimeRange.x, decisionTimeRange.y);
            PickMoveDirection();
        }
    }

    // Sampler calls this once per frame for "movement / bounds"
    public void TickMove(float dt)
    {
        // Wrap if we leave bounds (teleport inside)
        if (OutsideBounds())
        {
            transform.position = new Vector3(
                Random.Range(-mapSize.x * 0.5f, mapSize.x * 0.5f),
                Random.Range(-mapSize.y * 0.5f, mapSize.y * 0.5f),
                0f
            );
        }

        transform.position += moveDirections[currentDirIndex] * (speed * dt);
    }

    bool OutsideBounds()
    {
        Vector3 p = transform.position;
        float halfX = mapSize.x * 0.5f;
        float halfY = mapSize.y * 0.5f;

        return
            p.x >= halfX || p.x <= -halfX ||
            p.y >= halfY || p.y <= -halfY;
    }


    void PickMoveDirection()
    {
        // Choose whether to move sideways or up/down
        currentDirIndex = Mathf.FloorToInt(Random.Range(0, moveDirections.Length));
    }

}
