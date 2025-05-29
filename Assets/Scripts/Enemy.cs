using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform pointA,
        pointB;
    public int Speed;
    private Vector3 currentTarget;
    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position == pointA.position)
        {
            currentTarget = pointB.position;
            sr.flipX = false;
        }
        else if (transform.position == pointB.position)
        {   
            sr.flipX = true;
            currentTarget = pointA.position;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            Speed * Time.deltaTime
        );
    }
}
