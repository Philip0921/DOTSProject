using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIMove : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] float turnInterval = 1.5f;

    Rigidbody2D rb;
    private Vector2 dir;
    private float time;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        dir = Random.insideUnitCircle.normalized;
        time = 0f;
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time >= turnInterval)
        {
            time = 0f;
            dir = (dir + Random.insideUnitCircle * 0.75f).normalized;
        }
    }

    //private void CollisionAvoidance()
    //{
    //    var hit = Physics2D.CircleCast(new Vector2(rb.position.x, rb.position.y), 1f, dir, 1f);

    //    if (hit == true)
    //    {
    //        time = 0f;
    //    }
        

    //}

    private void FixedUpdate()
    {
        rb.linearVelocity = dir * speed;
    }
}
