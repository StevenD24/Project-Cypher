using UnityEngine;

public class Pickups : MonoBehaviour
{
    public int coinScore = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Coin")
        {   
            coinScore++;
            Destroy(collision.gameObject);
        }
    }
}
