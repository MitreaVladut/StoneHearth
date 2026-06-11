using UnityEngine;

public class EarthShatter : MonoBehaviour
{
    public float damage; // Damage dealt by the hitbox
    private PlayerController player; // Reference to the PlayerController


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (player != null)
        {
            damage = player.earthShatterDamage * player.earthDamageMultiplier; // Apply Earth Proficiency multiplier
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has a component with a TakeDamage method
        MonoBehaviour target = other.GetComponent<MonoBehaviour>();
        if (target != null && !other.CompareTag("Player"))
        {
            System.Reflection.MethodInfo takeDamageMethod = target.GetType().GetMethod("TakeDamage");
            if (takeDamageMethod != null && takeDamageMethod.GetParameters().Length == 1 && takeDamageMethod.GetParameters()[0].ParameterType == typeof(float))
            {
                takeDamageMethod.Invoke(target, new object[] { damage });
                Debug.Log($"Earth Shatter hit {other.gameObject.name} for {damage} damage!");
            }
        }
    }
}
