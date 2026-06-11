using UnityEngine;

public class CrystalProjectile : MonoBehaviour
{

    public float damage;                // Damage dealt by the crystal
    public float lifetime = 2f;         // Time before the crystal despawns
    private PlayerController player;    // Reference to the PlayerController

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindObjectOfType<PlayerController>(); // Find the player in the scene
        if (player != null)
        {
            damage = player.crystalShotgunDamage * player.celestialDamageMultiplier; // Apply celestial damange multiplier
        }
        Destroy(gameObject, lifetime); // Destroy after lifetime
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has a component with a TakeDamage method
        MonoBehaviour target = other.GetComponent<MonoBehaviour>();
        if (target != null && other.gameObject != player.gameObject) // Avoid hitting the player
        {
            System.Reflection.MethodInfo takeDamageMethod = target.GetType().GetMethod("TakeDamage");
            if (takeDamageMethod != null && takeDamageMethod.GetParameters().Length == 1 && takeDamageMethod.GetParameters()[0].ParameterType == typeof(float))
            {
                takeDamageMethod.Invoke(target, new object[] { damage });
                Debug.Log($"Crystal Shotgun hit {other.gameObject.name} for {damage} damage!");
                Destroy(gameObject); // Destroy on hit
            }
        }
    }
}
