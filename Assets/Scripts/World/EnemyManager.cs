using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    public static EnemyManager instance;

    private Enemy[] enemies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemies = FindObjectsOfType<Enemy>(); // Initialize enemy list
        Debug.Log("EnemyManager found " + enemies.Length + " enemies");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RespawnAllEnemies()
    {
        Debug.Log("Respawning all enemies");
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.Respawn();
            }
        }
    }
}
