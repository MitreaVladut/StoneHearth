using UnityEngine;

public class MagicPebble : MonoBehaviour
{
    public float lifetime = 5f;
    public float damage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has a component with a TakeDamage method
        MonoBehaviour target = collision.gameObject.GetComponent<MonoBehaviour>();
        if (target != null && !collision.gameObject.CompareTag("Player"))
        {
            System.Reflection.MethodInfo takeDamageMethod = target.GetType().GetMethod("TakeDamage");
            if (takeDamageMethod != null && takeDamageMethod.GetParameters().Length == 1 && takeDamageMethod.GetParameters()[0].ParameterType == typeof(float))
            {
                takeDamageMethod.Invoke(target, new object[] { damage });
                Debug.Log($"Pebble hit {collision.gameObject.name} for {damage} damage!");
            }
            
        }

        // Destroy the pebble on collision
        Destroy(gameObject);
    }
}
