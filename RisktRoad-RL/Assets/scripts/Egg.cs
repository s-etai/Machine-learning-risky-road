using UnityEngine;

public class Egg : MonoBehaviour
{
    public GameManager gameManager; // Reference to GameManager for game over.

    void Update()
    {
        //Score is based on how far the egg travels.
        float score = transform.position.x;
        gameManager.AddScore(score);

        //Reset if the egg falls bellow the map.
        if (transform.position.y < -100f)
        {
            gameManager.GameOver();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collisions with the truck
        if (collision.gameObject.CompareTag("Truck"))
            return;

        // Anything else, game over

        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

}
