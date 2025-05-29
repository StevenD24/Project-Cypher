using UnityEngine;
using UnityEngine.UI;

public class Pickups : MonoBehaviour
{
    public int coinScore = 0;
    public GameObject cfxr3HitLightEffect;
    public Text scoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        scoreText.text="Score: 0";
     }

    // Update is called once per frame
    void Update() { }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Coin")
        {
            coinScore++;
            scoreText.text = "Score: " + coinScore;
            Debug.Log("Coins collected: " + coinScore);

            // Play CFXR3 Hit Light B effect at Player's position
            if (cfxr3HitLightEffect != null)
            {
                GameObject effectInstance = Instantiate(
                    cfxr3HitLightEffect,
                    transform.position, // Player's position
                    Quaternion.identity
                );

                // Make the effect follow the player by making it a child
                effectInstance.transform.SetParent(transform);

                Destroy(effectInstance, 2f); // Destroy after 2 seconds
            }

            Destroy(collision.gameObject); // Destroy the coin
        }
    }
}
