using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float damage = 15f;      // Damage dealt by fireball
    private PlayerController player;// Reference to the player

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindObjectOfType<PlayerController>(); // Find the player in the scene
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is the player
        if (other.gameObject == player.gameObject)
        {
            player.TakeDamage(damage);
            Debug.Log($"Fireball hit player for {damage} damage!");
            Destroy(gameObject); // Destroy fireball on hit
        }
        else if (other.gameObject.CompareTag("Ground")) // Avoid hitting ground prematurely
        {
            Destroy(gameObject); // Destroy on ground collision
        }
    }
}
