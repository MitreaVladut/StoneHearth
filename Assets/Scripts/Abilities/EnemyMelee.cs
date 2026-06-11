using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    public float damage = 10f; // Match with enemy's meleeDamage

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject); // Despawn on hit
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject); // Despawn on hit
            Debug.Log($"Melee hitbox hit {other.gameObject.name} for {damage} damage!");
        }
    }
}
