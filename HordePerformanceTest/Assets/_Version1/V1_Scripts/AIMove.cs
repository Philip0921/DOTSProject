using UnityEngine;

public class AIMove : MonoBehaviour
{
    [SerializeField] float speed = 0.2f;
    [SerializeField] float decisionTimeCount = 0f;
    [SerializeField] Vector2 decisionTime = new Vector2(1, 8);

    private Vector3[] moveDirections = new Vector3[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.zero, Vector3.zero };
    private int currentMoveDirection;

    private void Start()
    {
        decisionTimeCount = Random.Range(decisionTime.x, decisionTime.y);

        PickMoveDirection();
    }

    private void Update()
    {
        transform.position += moveDirections[currentMoveDirection] * Time.deltaTime * speed;

        if (decisionTimeCount > 0) decisionTimeCount -= Time.deltaTime;
        else
        {
            // Choose a random time delay for taking a decision ( changing direction, or standing in place for a while )
            decisionTimeCount = Random.Range(decisionTime.x, decisionTime.y);

            // Choose a movement direction, or stay in place
            PickMoveDirection();
        }
    }

    void PickMoveDirection()
    {
        // Choose whether to move sideways or up/down
        currentMoveDirection = Mathf.FloorToInt(Random.Range(0, moveDirections.Length));
    }

}
