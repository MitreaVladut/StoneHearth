using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float maxHealth = 100;
    private float currentHealth;
    public Vector3 spawnPoint;

    public float speed = 10.0f;
    public float smoothTime = 0.1f;
    private Rigidbody rb;
    private Vector3 moveDirection = Vector3.zero; // Smoothed movement direction
    private Vector3 velocity = Vector3.zero; // For SmoothDamp

    public Transform cameraTransform;

    // Damage Multiplier for leveling mastery
    public float earthDamageMultiplier = 1.0f;
    public float celestialDamageMultiplier = 1.0f;

    // levels for leveling system
    public int hpLevel = 0;
    public int earthLevel = 0;
    public int magicLevel = 0;
    public int souls = 0;
    public int globalBaseCost = 100;


    // ability restrictions behind level
    private bool hasCrystalShotgun = false;
    private bool hasEarthShatter = false;

    /* IRON BODY ABILITY */
    public bool ironBody = false;
    public GameObject ironBodyIndicator;

    /* MAGIC PEBBLE SHOOTING */
    public GameObject magicPebblePrefab; // Reference to the Magic Pebble prefab
    public float shootForce = 20f; // Speed at which the pebble is shot
    public float shootOffset = 2f; // Distance in front of the player to spawn the pebble

    /* EARTH GAUNTLET ABILITY */
    public GameObject earthGauntletHitbox; // Reference to the hitbox GameObject (child of player)

    /* EARTH SHATTER ABILITY */
    public GameObject earthShatterHitboxPrefab; // Reference to the hitbox GameObject (child of player)
    public float earthShatterDamage = 20f;

    /* CRYSTAL SHOTGUN ABILITY */
    public GameObject crystalProjectilePrefab; // Prefab for the crystal projectile
    public int crystalCount = 5; // Number of crystals per shot
    public float crystalShotgunDamage = 15f; // Base damage per crystal
    public float crystalShootForce = 15f; // Force applied to each crystal
    public float crystalShootOffset = 1f; // Distance in front of the player to spawn crystals    
    public float spreadAngle = 20f; // Spread angle in degrees
    


    // Cooldown variables
    private float ironBodyCooldown = 2.0f; // Cooldown duration in seconds
    private float lastIronBodyTime = -10.0f; // Time when ironBody last deactivated

    private float magicPebbleCooldown = 2.5f; // Cooldown duration in seconds
    private float lastMagicPebbleTime = -10.0f; // Time when ironBody last deactivated

    private float earthGauntletCooldown = 0.4f; // Cooldown duration in seconds
    private float lastEarthGauntletTime = -10.0f; // Time when Earth Gauntlet last activated


    private float earthShatterCooldown = 3f; // Cooldown duration in seconds
    private float lastEarthShatterTime = -10f; // Time when Earth Shatter last activated


    public float crystalShotgunCooldown = 2f; // Cooldown duration in seconds
    private float lastCrystalShotgunTime = -10f; // Time when Crystal Shotgun last activated
    /**/


    public Image ironBodyIcon;          // IronBody icon for cooldown
    public Image magicPebbleIcon;       // Magic Pebble icon for cooldown
    public Image earthGauntletIcon;     // Earth Gauntlet icon for cooldown
    public Image earthShatterIcon;      // Earth Shatter icon for cooldown
    public Image crystalShotgunIcon;    // Crystal Shotgun icon for cooldown
    public static PlayerController instance { get; private set; }

    // Souls Mechanic
    public int soulCount = 0;           // Number of souls
    public TextMeshProUGUI soulText;    // Text for displaying souls

    public GameObject levelingCanvas;   // Totem Leveling System for Cursor Lock/Unlock
    private bool isCursorLocked = true;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth physics updates
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Prevent tilting

        // Set the instance
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        // Initialize icons as fully visible
        if (ironBodyIcon != null) ironBodyIcon.color = new Color(1, 1, 1, 1);
        if (magicPebbleIcon != null) magicPebbleIcon.color = new Color(1, 1, 1, 1);
        if (earthGauntletIcon != null) earthGauntletIcon.color = new Color(1, 1, 1, 1);
        if (earthShatterIcon != null) earthShatterIcon.color = new Color(1, 1, 1, 0);
        if (crystalShotgunIcon != null) crystalShotgunIcon.color = new Color(1, 1, 1, 0);


        soulCount = souls;      // Sync soulCount with souls at start
        UpdateSoulDisplay();    // Initialize soul display
        UpdateMaxHP();          // Initialize HP based on level
        UnlockCursor();         // Ensure cursor is visible at start

        spawnPoint = transform.position; // Set initial spawn point

        // Initialize multipliers based on starting levels
        earthDamageMultiplier = 1.0f + (earthLevel * 0.2f);
        celestialDamageMultiplier = 1.0f + (magicLevel * 0.2f);
        Debug.Log($"Initial globalBaseCost = {globalBaseCost}, Souls = {souls}");
    }   

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }
    

    void Update()
    {
        // Get movement input
        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical"); // W/S

        // Get camera's forward and right vectors, projected on the ground plane
        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 targetDirection = (cameraForward * moveZ + cameraRight * moveX).normalized;

        // Smooth the movement direction
        moveDirection = Vector3.SmoothDamp(moveDirection, targetDirection, ref velocity, smoothTime);

        // Rotate player to face movement direction (using transform.rotation to avoid physics conflicts)
        if (moveDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        // Test taking damage with Y
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TakeDamage(40); // Take 40 damage when Space is pressed
        }


        // Update icon transparency and handle ability activation
        UpdateIconTransparencyAndInput();


        // Activate ironBody ability with Space
        if (Input.GetKeyDown(KeyCode.Space) && !ironBody && Time.time >= lastIronBodyTime + ironBodyCooldown)
        {
            ActivateIronBody();
        }

        // Shoot magic pebble with 2
        if (Input.GetKeyDown(KeyCode.Alpha2) && Time.time >= lastMagicPebbleTime + magicPebbleCooldown)
        {
            ShootMagicPebble();
        }

        // Activate Earth Gauntlet ability with 1
        if (Input.GetKeyDown(KeyCode.Alpha1) && Time.time >= lastEarthGauntletTime + earthGauntletCooldown)
        {
            ActivateEarthGauntlet();
        }

        // Activate Earth Shatter ability with 3
        if (Input.GetKeyDown(KeyCode.Alpha3) && Time.time >= lastEarthShatterTime + earthShatterCooldown && hasEarthShatter == true)
        {
            ActivateEarthShatter();
        }

        // Shoot Crystal Shotgun with 4
        if (Input.GetKeyDown(KeyCode.Alpha4) && Time.time >= lastCrystalShotgunTime + crystalShotgunCooldown && hasCrystalShotgun == true)
        {
            ActivateCrystalShotgun();
        }

        // Update soul display
        UpdateSoulDisplay();

        /// Toggle cursor based on leveling canvas state
        if (levelingCanvas != null && levelingCanvas.activeSelf)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }
    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        Vector3 movement = moveDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void ActivateIronBody()
    {
        ironBody = true; // Set ironBody to true
        if (ironBodyIndicator != null)
        {
            ironBodyIndicator.SetActive(true); // Show the indicator
        }

        // Start the coroutine to deactivate after 1.5 seconds
        StartCoroutine(DeactivateIronBodyAfterDelay(1.5f));
    }

    // Coroutine to deactivate ironBody after a delay
    private IEnumerator DeactivateIronBodyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for 1.5 seconds

        ironBody = false; // Set ironBody back to false
        if (ironBodyIndicator != null)
        {
            ironBodyIndicator.SetActive(false); // Hide the indicator
        }
        lastIronBodyTime = Time.time;
    }

    private void ShootMagicPebble()
    {
        if (magicPebblePrefab == null) return;

        // Calculate spawn position
        Vector3 spawnPosition = transform.position + transform.forward * shootOffset;

        // Instantiate the pebble at the spawn position with the player's rotation
        Quaternion spawnRotation = transform.rotation * Quaternion.Euler(90, 0, 0); // Rotate 90 degrees on X to make projectile go with a point towards direction
        GameObject pebble = Instantiate(magicPebblePrefab, spawnPosition, spawnRotation);

        // Get the pebble's Rigidbody and apply force in the player's forward direction
        Rigidbody pebbleRb = pebble.GetComponent<Rigidbody>();
        MagicPebble pebbleScript = pebble.GetComponent<MagicPebble>();
        if (pebbleRb != null)
        {
            pebbleScript.damage = 25f * celestialDamageMultiplier;                  // Set damage with current multiplier
            pebbleRb.AddForce(transform.forward * shootForce, ForceMode.Impulse);   // Apply an instant force
            Debug.Log($"Pebble shot with force: {transform.forward * shootForce}");
        }

        // Start cooldown coroutine with 2.5 seconds delay immedietly
        lastMagicPebbleTime = Time.time; // Set the cooldown time right after shooting 
        StartCoroutine(ShootMagicPebbleAfterDelay(2.5f));
    }

    private void ActivateEarthGauntlet()
    {
        lastEarthGauntletTime = Time.time; // Start cooldown immediately
        StartCoroutine(PerformEarthGauntlet());
    }

    private IEnumerator PerformEarthGauntlet()
    {
        // Activate the hitbox
        if (earthGauntletHitbox != null)
        {
            EarthGauntletHitbox hitbox = earthGauntletHitbox.GetComponent<EarthGauntletHitbox>();
            if (hitbox != null)
            {
                hitbox.damage = 25f * earthDamageMultiplier; // Set damage with current multiplier
            }
            earthGauntletHitbox.SetActive(true);
        }

        // Wait for 0.2 seconds (duration of the attack)
        yield return new WaitForSeconds(0.2f);

        // Deactivate the hitbox
        if (earthGauntletHitbox != null)
        {
            earthGauntletHitbox.SetActive(false);
        }
    }

    private IEnumerator ShootMagicPebbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }


    private void ActivateEarthShatter()
    {
        lastEarthShatterTime = Time.time; // Start cooldown immediately
        StartCoroutine(PerformEarthShatter());
    }

    private IEnumerator PerformEarthShatter()
    {
        // Instantiate the hitbox in the direction the player is facing
        if (earthShatterHitboxPrefab != null)
        {
            // Calculate spawn position closer to the ground
            float spawnOffset = 1f; // Distance in front of the player to spawn the hitbox
            float groundOffset = 0.50f; // Half of player height
            Vector3 spawnPosition = (transform.position - Vector3.up * groundOffset) + transform.forward * spawnOffset;

            // Use the player's rotation to align the hitbox with the facing direction
            Quaternion spawnRotation = transform.rotation;

            // Instantiate the hitbox
            GameObject earthShatterHitbox = Instantiate(earthShatterHitboxPrefab, spawnPosition, spawnRotation);
            EarthShatter shatter = earthShatterHitbox.GetComponent<EarthShatter>();

            if (shatter != null)
            {
                shatter.damage = earthShatterDamage * earthDamageMultiplier; // Set damage with current multiplier
            }

            // Wait for 0.3 seconds (duration of the ability)
            yield return new WaitForSeconds(0.3f);

            // Destroy the hitbox immediately
            if (earthShatterHitbox != null)
            {
                Destroy(earthShatterHitbox);
            }
        }
    }

    private void ActivateCrystalShotgun()
    {
        lastCrystalShotgunTime = Time.time; // Start cooldown immediately
        ShootCrystalShotgun();
    }

    private void ShootCrystalShotgun()
    {
        if (crystalProjectilePrefab == null) return;

        // Calculate spawn position
        Vector3 spawnPosition = transform.position + transform.forward * shootOffset;

        // Spawn multiple crystals with spread
        for (int i = 0; i < crystalCount; i++)
        {
            // Calculate angle for spread
            float angle = spreadAngle * ((float)i / (crystalCount - 1) - 0.5f); // Distribute angles evenly
            Quaternion rotation = Quaternion.Euler(0, angle, 0) * transform.rotation;

            // Instantiate the crystal
            GameObject crystal = Instantiate(crystalProjectilePrefab, spawnPosition, rotation);
            CrystalProjectile crystalScript = crystal.GetComponent<CrystalProjectile>();

            if (crystalScript != null)
            {
                crystalScript.damage = crystalShotgunDamage * celestialDamageMultiplier; // Set damage with current multiplier
            }

            // Apply force to the crystal
            Rigidbody crystalRb = crystal.GetComponent<Rigidbody>();
            if (crystalRb != null)
            {
                crystalRb.AddForce(crystal.transform.forward * shootForce, ForceMode.Impulse);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (!ironBody) // Only take damage if ironBody is false
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health stays between 0 and maxHealth

            if (currentHealth <= 0)
            {
                Respawn();
                Debug.Log("The player has died");
                soulCount = 0;
            }
        }
    }

    private void Respawn()
    {
        transform.position = spawnPoint;    // Respawn at last spawn point
        currentHealth = maxHealth;          // Restore health
        rb.linearVelocity = Vector3.zero;   // Stop any movement
        Debug.Log("Player respawned at " + spawnPoint);
    }
    public void RestoreHealth()
    {
        currentHealth = maxHealth;          // Restore health to maximum
        //UpdateSoulDisplay(); 
        Debug.Log("Health restored to " + currentHealth);
    }

    // Getter for health percentage (for UI)
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth; // Returns a value between 0 and 1
    }

    private void UpdateIconTransparencyAndInput()
    {
        float ironBodyRemaining = Mathf.Max(0, ironBodyCooldown - (Time.time - lastIronBodyTime));
        float magicPebbleRemaining = Mathf.Max(0, magicPebbleCooldown - (Time.time - lastMagicPebbleTime));
        float earthGauntletRemaining = Mathf.Max(0, earthGauntletCooldown - (Time.time - lastEarthGauntletTime));
        float earthShatterRemaining = Mathf.Max(0, earthShatterCooldown - (Time.time - lastEarthShatterTime));
        float crystalShotgunRemaining = Mathf.Max(0, crystalShotgunCooldown - (Time.time - lastCrystalShotgunTime));

        if (ironBodyIcon != null)
            ironBodyIcon.color = new Color(1, 1, 1, 1 - (ironBodyRemaining / ironBodyCooldown));
        if (magicPebbleIcon != null)
            magicPebbleIcon.color = new Color(1, 1, 1, 1 - (magicPebbleRemaining / magicPebbleCooldown));
        if (earthGauntletIcon != null)
            earthGauntletIcon.color = new Color(1, 1, 1, 1 - (earthGauntletRemaining / earthGauntletCooldown));
        if (earthShatterIcon != null && hasEarthShatter == true)
            earthShatterIcon.color = new Color(1, 1, 1, 1 - (earthShatterRemaining / earthShatterCooldown));
        if (crystalShotgunIcon != null && hasCrystalShotgun == true)
            crystalShotgunIcon.color = new Color(1, 1, 1, 1 - (crystalShotgunRemaining / crystalShotgunCooldown));
    }
    
    public void AddSouls(int amount)
    {
        souls += amount;
        soulCount = souls;  // Sync soulCount with souls
        UpdateSoulDisplay();
    }

    private void UpdateSoulDisplay()
    {
        if (soulText != null)
        {
            soulText.text = $"Souls: {soulCount}";
        }
    }

    private void UpdateMaxHP()
    {
        maxHealth = 100f + (hpLevel * 25f); // Base 100 + 25 per level
        currentHealth = maxHealth;          // Reset current health to max
        Debug.Log("Max HP updated to: " + maxHealth);
    }

    public void LevelUp(string category)
    {
        int nextCost = globalBaseCost;

        if (souls >= nextCost)
        {
            switch (category)
            {
                case "HP":
                    if (hpLevel < 4)
                    {
                        hpLevel++;
                        souls -= nextCost;
                        globalBaseCost = (int)((globalBaseCost + 200) * 1.3f); // Update global base cost for next level
                        UpdateMaxHP();
                        Debug.Log($"Leveled up HP to {hpLevel}, Remaining Souls = {souls}");
                    }
                    else
                    {
                        Debug.Log("Max level (4) reached for HP");
                    }
                    break;
                case "Earth":
                    if (earthLevel < 4)
                    {
                        earthLevel++;
                        souls -= nextCost;
                        globalBaseCost = (int)((globalBaseCost + 200) * 1.3f);  // Update global base cost for next level
                        earthDamageMultiplier = 1.0f + (earthLevel * 0.2f);     // Increase by 20% per level
                        Debug.Log($"Leveled up Earth to {earthLevel}, Remaining Souls = {souls}, Damage Multiplier = {earthDamageMultiplier}");
                    }
                    else
                    {
                        Debug.Log("Max level (4) reached for Earth");
                    }
                    break;
                case "Magic":
                    if (magicLevel < 4)
                    {
                        magicLevel++;
                        souls -= nextCost;
                        globalBaseCost = (int)((globalBaseCost + 200) * 1.3f);  // Update global base cost for next level
                        celestialDamageMultiplier = 1.0f + (magicLevel * 0.2f); // Increase by 20% per level
                        Debug.Log($"Leveled up Magic to {magicLevel}, Remaining Souls = {souls}, Damage Multiplier = {celestialDamageMultiplier}");
                    }
                    else
                    {
                        Debug.Log("Max level (4) reached for Magic");
                    }
                    break;
            }
            soulCount = souls;      // Sync soulCount with souls after deduction
            UpdateSoulDisplay();    // Update the soul UI text
            CheckSpecialUnlocks();  // Check for new ability unlocks
        }
        else
        {
            Debug.Log("Insufficient souls! Required: " + nextCost + ", Available: " + souls);
        }
    }
    
    private void CheckSpecialUnlocks()
    {
        if (earthLevel >= 2 && magicLevel >= 2 && !hasCrystalShotgun)
        {
            UnlockAbility("CrystalShotgun");
            crystalShotgunIcon.color = new Color(1, 1, 1, 1);
            hasCrystalShotgun = true;
        }

        if (earthLevel >= 3)
        {
            UnlockAbility("EarthShatter");
            earthShatterIcon.color = new Color(1, 1, 1, 1);
            hasEarthShatter = true;
        }
    }

    private void UnlockAbility(string ability)
    {
        Debug.Log("Unlocked: " + ability);
    }
}
