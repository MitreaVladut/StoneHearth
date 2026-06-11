using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    // Health Variables
    public float maxHealth = 50f;
    private float currentHealth;

    // Health UI
    public UnityEngine.UI.Slider healthBarSlider; // Reference to the UI Slider for the health bar

    // Movement
    public float baseMoveSpeed = 5f;    // Base speed to adjust from player's speed

    // Fireball
    public GameObject fireballPrefab;   // Prefab for the fireball projectile
    public float fireballSpeed = 10f;   // Speed of the fireball
    public float fireballDamage = 15f;  // Damage for fireball attack

    // Melee Attack
    public GameObject meleeHitboxPrefab;        // Reference to the melee hitbox prefab
    private Color originalColor;                // Original color of the enemy for resetting
    public float meleeDamage = 10f;             // Damage for melee attack

    // Attack Ranges and Cooldowns
    public float meleeRange = 2f;       // Close range threshold
    public float midRange = 10f;        // Mid range threshold
    public float attackCooldown = 2f;   // Time between actions
    private float lastActionTime;       // Last time an action occurred


    private PlayerController player;    // Reference to the player
    private Rigidbody rb;               // Enemy's Rigidbody

    public GameObject airSlashPrefab;
    public bool isBoss = false;         // To exclude bosses from respawn
    public GameObject enemyPrefab;     // Enemy prefab for summoning
    private Vector3 originalPosition;   // Original Position for respawning
    private bool isDead = false;

    // Souls Mechanic
    public int soulsOnKill = 200; // Souls dropped on death, modifiable in Inspector

    // Enemy AI
    private bool isActivated = false; // Activation state
    private float detectionRange = 10f; // Enemy detection range for activation

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
        player = Object.FindFirstObjectByType<PlayerController>();
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        originalPosition = transform.position;
        isDead = false; // New state variable
        isActivated = false; // Set state as deactivated
        GetComponent<MeshRenderer>().enabled = true; // Ensure visible on start
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
            collider.isTrigger = false;
        }
        Debug.Log("Enemy " + gameObject.name + " initialized at " + originalPosition +
                  " Layer: " + LayerMask.LayerToName(gameObject.layer) +
                  " Collider enabled: " + (collider != null && collider.enabled));
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || currentHealth <= 0) return;

        // Update health bar position above enemy
        if (healthBarSlider != null)
        {
            Vector3 sliderPosition = transform.position + Vector3.up * 2f; // 2f above enemy
            healthBarSlider.transform.position = sliderPosition;
        }

        // Check if enough time has passed since the last action
        if (Time.time >= lastActionTime + attackCooldown)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (!isActivated)
            {
                if (distanceToPlayer <= detectionRange)
                {
                    isActivated = true; // Activate when player enters range
                }
                else
                {
                    return; // Do nothing if not activated and out of range
                }
            }
            if (isBoss)
            {
                int choice = Random.Range(0, 3); // 0, 1, or 2 for three choices
                if (choice == 0)
                {
                    StartCoroutine(PerformGroundSlam());
                }
                else if (choice == 1)
                {
                    StartCoroutine(PerformHorizontalSlash());
                }
                else // choice == 2
                {
                    StartCoroutine(SummonEnemies());
                }
            }
            else
            {
                int choice = Random.Range(0, 2); // 0 or 1 for two choices
                if (distanceToPlayer < meleeRange) // Close range
                {
                    if (choice == 0)
                    {
                        StartCoroutine(PerformMeleeAndBackOff());
                    }
                    else
                    {
                        StartCoroutine(BackOff());
                    }
                }
                else if (distanceToPlayer <= midRange) // Mid range
                {
                    if (choice == 0)
                    {
                        StartCoroutine(MoveTowardPlayer());
                    }
                    else
                    {
                        StartCoroutine(PerformFireballAttack());
                    }
                }
                else // Far range
                {
                    if (choice == 0)
                    {
                        StartCoroutine(PerformFireballAttack());
                    }
                    else
                    {
                        StartCoroutine(MoveTowardPlayer());
                    }
                }
            }
        }
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return; // Prevent damage if already dead

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        isDead = true;
        if (healthBarSlider != null)
        {
            healthBarSlider.gameObject.SetActive(false);
        }
        GetComponent<MeshRenderer>().enabled = false; // Hide enemy
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false; // Disable collider to prevent further hits
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Prevent physics movement
        }
        // Drop souls and collect automatically
        if (player != null)
        {
            player.AddSouls(soulsOnKill);
        }
    }

    private IEnumerator MoveTowardPlayer()
    {
        lastActionTime = Time.time; // Update last action time
        Debug.Log("Enemy moves toward player!");
        if (rb != null && player != null)
        {
            // Estimate player speed (simple approximation based on last frame movement)
            /*Vector3 playerVelocity = (player.transform.position - player.transform.position) / Time.deltaTime; // Placeholder, improve with actual tracking
            float playerSpeed = playerVelocity.magnitude;
            float enemySpeed = playerSpeed * 0.8f; // 80% of player speed for slightly slower
            enemySpeed = Mathf.Max(enemySpeed, baseMoveSpeed); // Ensure minimum speed*/

            Vector3 moveDirection = (player.transform.position - transform.position).normalized;
            float totalDistance = Vector3.Distance(transform.position, player.transform.position);
            float movePercentage = Random.Range(0.2f, 0.5f); // Random portion between 20% and 50%
            float targetDistance = totalDistance * movePercentage;

            float elapsedTime = 0f;
            float moveDuration = targetDistance / baseMoveSpeed; // Time based on distance and speed
            Vector3 startPosition = transform.position;
            float initialY = transform.position.y; // Store initial y position

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveDuration;
                Vector3 newPosition = Vector3.Lerp(startPosition, startPosition + moveDirection * targetDistance, t);
                newPosition.y = initialY; // Lock y to initial height to prevent jitter
                rb.MovePosition(newPosition);

                if (Vector3.Distance(startPosition, transform.position) >= targetDistance)
                {
                    rb.MovePosition(startPosition + moveDirection * targetDistance);
                    break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(moveDuration); // Wait for the calculated duration
        }
    }

    // Perform melee and back off
    private IEnumerator PerformMeleeAndBackOff()
    {
        lastActionTime = Time.time; // Update last action time
        Debug.Log("Enemy performs melee and backs off!");

        // Rotate to face the player
        if (player != null)
        {
            transform.LookAt(player.transform.position);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); // Keep Y rotation only
        }
        // Start color shift as attack warning
        StartCoroutine(StartColorShift());
        // Melee attack
        if (meleeHitboxPrefab != null)
        {
            // Position hitbox in front of enemy, matching its size
            Vector3 enemySize = GetComponent<Renderer>().bounds.size;
            Vector3 spawnPosition = transform.position + transform.forward * (enemySize.z * 0.5f); // Position in front
            Quaternion spawnRotation = transform.rotation;
            GameObject hitboxInstance = Instantiate(meleeHitboxPrefab, spawnPosition, spawnRotation);

            // Ensure the hitbox is active and visible, and scale it to match enemy
            hitboxInstance.SetActive(true);
            MeshRenderer hitboxRenderer = hitboxInstance.GetComponent<MeshRenderer>();
            if (hitboxRenderer != null)
            {
                hitboxRenderer.enabled = true; // Make hitbox visible
            }
            hitboxInstance.transform.localScale = enemySize; // Match enemy's size

            // Set the hitbox damage to match enemy's meleeDamage
            EnemyMelee hitboxScript = hitboxInstance.GetComponent<EnemyMelee>();
            if (hitboxScript != null)
            {
                hitboxScript.damage = meleeDamage; // Sync damage with enemy's meleeDamage
            }

            yield return new WaitForSeconds(0.2f); // Match Earth Gauntlet duration

            // Clean up hitbox
            Destroy(hitboxInstance);
        }

        // Back off smoothly
        if (rb != null)
        {
            Vector3 backDirection = (transform.position - player.transform.position).normalized;
            float targetDistance = Random.Range(1f, 2f); // Random distance between 1 and 2 units
            float elapsedTime = 0f;
            float moveDuration = targetDistance / (baseMoveSpeed * 0.8f); // Slower back off speed
            Vector3 startPosition = transform.position;
            float initialY = transform.position.y; // Store initial y position

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveDuration;
                Vector3 newPosition = Vector3.Lerp(startPosition, startPosition + backDirection * targetDistance, t);
                newPosition.y = initialY; // Lock y to initial height to prevent jitter
                rb.MovePosition(newPosition);

                // Stop if the target distance is reached
                if (Vector3.Distance(startPosition, transform.position) >= targetDistance)
                {
                    rb.MovePosition(startPosition + backDirection * targetDistance);
                    break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(moveDuration); // Wait for the calculated duration
        }
    }

    // Back off only
    private IEnumerator BackOff()
    {
        lastActionTime = Time.time; // Update last action time
        Debug.Log("Enemy backs off!");
        if (rb != null)
        {
            Vector3 backDirection = (transform.position - player.transform.position).normalized;
            float targetDistance = Random.Range(1f, 2f); // Random distance between 1 and 2 units
            float elapsedTime = 0f;
            float moveDuration = targetDistance / (baseMoveSpeed * 0.8f); // Slower back off speed
            Vector3 startPosition = transform.position;
            float initialY = transform.position.y; // Store initial y position

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveDuration;
                Vector3 newPosition = Vector3.Lerp(startPosition, startPosition + backDirection * targetDistance, t);
                newPosition.y = initialY; // Lock y to initial height to prevent jitter
                rb.MovePosition(newPosition);

                // Stop if the target distance is reached
                if (Vector3.Distance(startPosition, transform.position) >= targetDistance)
                {
                    rb.MovePosition(startPosition + backDirection * targetDistance);
                    break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(moveDuration); // Wait for the calculated duration
        }
    }

    // Perform Fireball Attack
    private IEnumerator PerformFireballAttack()
    {
        lastActionTime = Time.time; // Update last action time
        Debug.Log("Enemy shoots fireball!");

        if (fireballPrefab != null)
        {
            // Spawn fireball at enemy position
            Vector3 spawnPosition = transform.position + transform.forward * 1f;
            GameObject fireball = Instantiate(fireballPrefab, spawnPosition, transform.rotation);

            // Move fireball toward player
            Vector3 direction = (player.transform.position - spawnPosition).normalized;
            Rigidbody fireballRb = fireball.GetComponent<Rigidbody>();
            if (fireballRb != null)
            {
                fireballRb.linearVelocity = direction * fireballSpeed;
            }

            // Destroy fireball after 3 seconds or when it hits something (handled by fireball script)
            Destroy(fireball, 3f);
        }

        yield return new WaitForSeconds(0.5f); // Fireball cast animation duration
    }

    public void Respawn()
    {
        if (isDead && isBoss == false)
        {
            isDead = false;
            Debug.Log($"{gameObject.name} respawned at {originalPosition}");
            GetComponent<MeshRenderer>().enabled = true; // Show enemy
            currentHealth = maxHealth;
            if (healthBarSlider != null)
            {
                healthBarSlider.value = currentHealth;
                healthBarSlider.gameObject.SetActive(true);
            }
            transform.position = originalPosition;
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
                collider.isTrigger = false;
            }
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            isActivated = false; // Reset activation state
        }
        else
        {
            Debug.Log($"{gameObject.name} already alive, resetting at {originalPosition}");
            transform.position = originalPosition;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            isActivated = false; // Reset activation state
        }
    }
    private IEnumerator StartColorShift()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        originalColor = renderer.material.color; // Save original color for reset
        float duration = 0.5f; // Duration of color shift
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            renderer.material.color = Color.Lerp(originalColor, Color.yellowGreen, t); // Shift to yellow-green
            yield return null;
        }

        yield return new WaitForSeconds(attackCooldown - duration); // Hold color briefly

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            renderer.material.color = Color.Lerp(Color.red, originalColor, t); // Shift back
            yield return null;
        }
    }
    // BOSS Related
    private IEnumerator PerformGroundSlam()
    {
        lastActionTime = Time.time;
        Debug.Log("Boss performs Ground Slam!");

        // Visual cue (color shift)
        StartCoroutine(StartColorShift());
        yield return new WaitForSeconds(0.5f); // Wind-up time

        // Jump into the air
        if (rb != null)
        {
            Vector3 jumpHeight = transform.position + Vector3.up * 5f; // Jump to 5 units high
            float jumpDuration = 1f; // Time to reach peak
            float elapsed = 0f;

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;
                Vector3 newPosition = Vector3.Lerp(transform.position, jumpHeight, t);
                rb.MovePosition(newPosition);
                yield return null;
            }

            // Fall back down
            elapsed = 0f;
            float fallDuration = 1f; // Time to fall
            Vector3 startFallPosition = transform.position;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                Vector3 newPosition = Vector3.Lerp(startFallPosition, originalPosition, t);
                rb.MovePosition(newPosition);
                yield return null;
            }
        }

        // Deal damage on landing
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 7f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != gameObject && hitCollider.GetComponent<PlayerController>() != null)
            {
                PlayerController playerHit = hitCollider.GetComponent<PlayerController>();
                if (playerHit != null)
                {
                    playerHit.TakeDamage(45f); // Adjust damage as needed
                }
            }
        }

        yield return new WaitForSeconds(0.5f); // Attack duration
    }

    private IEnumerator PerformHorizontalSlash()
    {
        lastActionTime = Time.time;
        Debug.Log("Boss performs Horizontal Slash!");

        // Visual cue (color shift)
        StartCoroutine(StartColorShift());
        yield return new WaitForSeconds(0.5f); // Wind-up time

        // Spawn air slash projectile with length (Z-axis) as the leading edge
        if (airSlashPrefab != null && player != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 1f;
            // Calculate direction toward player
            Vector3 directionToPlayer = (player.transform.position - spawnPosition).normalized;
            // Set rotation to align Z-axis (length) with direction
            Quaternion spawnRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0, 90, 0);
            GameObject slash = Instantiate(airSlashPrefab, spawnPosition, spawnRotation);
            Rigidbody slashRb = slash.GetComponentInChildren<Rigidbody>();
            if (slashRb != null)
            {
                slashRb.linearVelocity = directionToPlayer * fireballSpeed; // Move along Z-axis
                EnemyMelee slashScript = slash.GetComponentInChildren<EnemyMelee>();
                if (slashScript != null)
                {
                    slashScript.damage = meleeDamage * 1.5f;
                }
            }
            else
            {
                Debug.LogError("No Rigidbody found in airSlashPrefab!");
            }
            Destroy(slash, 3f); // Despawn after 3 seconds or on collision
        }
        else
        {
            if (airSlashPrefab == null)
                Debug.LogError("airSlashPrefab is not assigned!");
            if (player == null)
                Debug.LogError("Player reference is null!");
        }
        yield return new WaitForSeconds(0.5f); // Attack duration
    }

    private IEnumerator SummonEnemies()
    {
        lastActionTime = Time.time;
        Debug.Log("Boss summons enemies!");

        // Visual cue (color shift)
        StartCoroutine(StartColorShift());
        yield return new WaitForSeconds(0.5f); // Wind-up time

        if (enemyPrefab != null)
        {
            int enemyCount = Random.Range(1, 4); // Summon 1 to 3 enemies
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnOffset = Random.insideUnitSphere * 5f; // Random position within 5 units
                spawnOffset.y = 0; // Keep on ground
                GameObject newEnemy = Instantiate(enemyPrefab, transform.position + spawnOffset, Quaternion.identity);
                Enemy enemyScript = newEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.isBoss = false; // Ensure summoned enemies are not bosses
                }
            }
            Debug.Log($"Summoned {enemyCount} enemies.");
        }
        else
        {
            Debug.LogError("enemyPrefab is null, no enemies summoned!");
        }

        yield return new WaitForSeconds(0.5f); // Summoning duration
    }
}