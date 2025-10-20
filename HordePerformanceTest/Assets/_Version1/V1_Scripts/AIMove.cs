using UnityEngine;

public class AIMove : MonoBehaviour
{
    [SerializeField] float speed = 0.2f;
    [SerializeField] float decisionTimeCount = 0f;
    [SerializeField] Vector2 decisionTime = new Vector2(1, 8);
    [SerializeField] Vector2 mapSize = new Vector2(200f, 200f);

    private Vector3[] moveDirections = new Vector3[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.zero, Vector3.zero };
    private int currentMoveDirection;

    private void Start()
    {     
        decisionTimeCount = Random.Range(decisionTime.x, decisionTime.y);

        PickMoveDirection();
    }

    private void Update()
    {
        if (ReturnToCenter())
        {
            transform.position = new Vector3(Random.Range(-mapSize.x / 2, mapSize.x / 2), Random.Range(-mapSize.y / 2, mapSize.y / 2), 0);
        }

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

    private bool ReturnToCenter()
    {
        if (transform.position.x >= mapSize.x / 2 || transform.position.x <= -mapSize.x / 2
            || transform.position.y >= mapSize.y/2 || transform.position.y <= -mapSize.y/2)
        {
            return true;
        }
        else return false;
    }

    void PickMoveDirection()
    {
        // Choose whether to move sideways or up/down
        currentMoveDirection = Mathf.FloorToInt(Random.Range(0, moveDirections.Length));
    }

}
