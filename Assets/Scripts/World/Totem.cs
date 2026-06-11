using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Totem : MonoBehaviour
{
    public float interactionRange = 2f;             // Distance at which the player can interact
    private bool isPlayerNear = false;
    private bool isSitting = false;

    public TextMeshProUGUI interactionTextObject;   // Reference to the Text component on the existing Canvas
    private int currentSelection = 0; // 0 = Leave Totem, 1 = Respawn Enemies, 2 = Enter Level System
    private string[] options = { "Leave Totem", "Respawn Enemies", "Enter Level System" };


    public EnemyManager enemyManager;

    public GameObject levelingCanvas;               // Leveling System Canvas
    private LevelingSystem levelingSystem;

    public static Totem instance { get; private set; }

    void Start()
    {
        if (levelingCanvas != null)
        {
            levelingSystem = levelingCanvas.GetComponent<LevelingSystem>();
            if (levelingSystem == null)
            {
                Debug.LogError("LevelingSystem component not found on " + levelingCanvas.name);
            }
        }
        else
        {
            Debug.LogError("levelingCanvas is not assigned for Totem " + gameObject.name);
        }
        if (interactionTextObject == null)
        {
            Debug.LogWarning($"Totem {gameObject.name}: interactionTextObject is not assigned, searching for existing child text");
            Transform uiCanvas = transform.Find("UICanvas");
            if (uiCanvas != null)
            {
                Transform totemInteraction = uiCanvas.Find("TotemInteraction (1)");
                if (totemInteraction != null)
                {
                    interactionTextObject = totemInteraction.GetComponent<TextMeshProUGUI>();
                    if (interactionTextObject == null)
                    {
                        Debug.LogError($"Totem {gameObject.name}: TotemInteraction (1) under UICanvas has no TextMeshProUGUI component");
                    }
                    else
                    {
                        Debug.Log($"Totem {gameObject.name}: Found TextMeshProUGUI on {interactionTextObject.gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Totem {gameObject.name}: TotemInteraction (1) not found under UICanvas, creating new text");
                    GameObject textObj = new GameObject("TotemInteraction (1)");
                    textObj.transform.SetParent(uiCanvas, false);
                    interactionTextObject = textObj.AddComponent<TextMeshProUGUI>();
                    interactionTextObject.rectTransform.sizeDelta = new Vector2(30, 10);
                    interactionTextObject.gameObject.SetActive(false);                      // Start with text off
                }
            }
            else
            {
                Debug.LogError($"Totem {gameObject.name}: UICanvas not found, functionality limited");
            }
        }
        else
        {
            interactionTextObject.gameObject.SetActive(false); // Ensure text is off at start
            Debug.Log($"Totem {gameObject.name}: Text component initialized on {interactionTextObject.gameObject.name}");
        }
        if (enemyManager == null)
        {
            enemyManager = GetComponentInChildren<EnemyManager>();
            if (enemyManager == null)
            {
                Debug.LogError("EnemyManager not found as child of Totem " + gameObject.name);
            }
        }
    }

    void Update()
    {
        // Check if player is near the totem
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerController.instance.transform.position);
        isPlayerNear = distanceToPlayer <= interactionRange;
        Debug.Log($"Totem {gameObject.name}: Player distance = {distanceToPlayer}, isPlayerNear = {isPlayerNear}, interactionRange = {interactionRange}");

        // Show or hide and update interaction text
        if (interactionTextObject != null)
        {
            // Position text relative to totem
            interactionTextObject.rectTransform.localPosition = new Vector3(0, 0, 0); // Centered in canvas

            // Rotate canvas to face away from the player
            GameObject canvasParent = interactionTextObject.transform.parent.gameObject; // Get the UICanvas
            Vector3 directionToPlayer = PlayerController.instance.transform.position - canvasParent.transform.position;
            directionToPlayer.y = 0; // Keep rotation on the horizontal plane
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-directionToPlayer.normalized); // Invert direction to look away
                canvasParent.transform.rotation = Quaternion.Slerp(canvasParent.transform.rotation, targetRotation, Time.deltaTime * 5f);
                canvasParent.transform.localEulerAngles = new Vector3(0, canvasParent.transform.localEulerAngles.y, 0); // Lock to Y-axis rotation only
            }

            // Enable or disable the text GameObject based on proximity
            GameObject textParent = interactionTextObject.gameObject;
            bool wasActive = textParent.activeSelf;
            if (isPlayerNear)
            {
                textParent.SetActive(true); // Enable the GameObject
                Debug.Log($"Totem {gameObject.name}: Text parent set to active, was {wasActive}");
                if (isSitting)
                {
                    interactionTextObject.text = $"> {options[currentSelection]} <"; // Highlight selected option
                    Debug.Log($"Totem {gameObject.name}: Showing option '{options[currentSelection]}' for sitting");
                }
                else
                {
                    interactionTextObject.text = "Press E to Sit";
                    Debug.Log($"Totem {gameObject.name}: Showing 'Press E to Sit'");
                }
            }
            else
            {
                textParent.SetActive(false);        // Disable the GameObject
                interactionTextObject.text = "";    // Clear text when not near
                Debug.Log($"Totem {gameObject.name}: Text parent set to inactive");
            }
        }
        else
        {
            Debug.LogError($"Totem {gameObject.name}: interactionTextObject is not assigned");
        }

        // Handle interaction (press 'E' to sit)
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (!isSitting)
            {
                SitAtTotem();
            }
        }

        // Scroll through options with arrow keys while sitting
        if (isSitting)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentSelection = (currentSelection - 1 + options.Length) % options.Length;
                UpdateInteractionText();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSelection = (currentSelection + 1) % options.Length;
                UpdateInteractionText();
            }
            if (Input.GetKeyDown(KeyCode.Return)) // Use 'Enter' to select option
            {
                SelectOption();
            }
        }
    }

    private void SitAtTotem()
    {
        isSitting = true;
        currentSelection = 0; // Default to "Leave Totem" when sitting
        Debug.Log("Player is now sitting at the Totem!");
        // Restrict movement
        PlayerController.instance.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        UpdateInteractionText();
        PlayerController.instance.spawnPoint = transform.position;  // Update spawn point to totem location
        PlayerController.instance.RestoreHealth();                  // Restore player's health
        Debug.Log("Spawn point set to totem at " + transform.position);
    }

    private void LeaveTotem()
    {
        isSitting = false;
        Debug.Log("Player has left the Totem!");
        // Remove movement constraints
        PlayerController.instance.GetComponent<Rigidbody>().constraints &= ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ);
        if (levelingSystem != null) levelingSystem.DeactivateLeveling();
    }

    private void SelectOption()
    {
        if (currentSelection == 0)
        {
            LeaveTotem();
        }
        else if (currentSelection == 1)
        {
            RespawnEnemiesInZone();
        }
        else if (currentSelection == 2)
        {
            EnterLevelingMode();
        }
    }

    private void RespawnEnemiesInZone()
    {
        Debug.Log("Triggering respawn of all enemies from totem at " + transform.position);
        enemyManager.RespawnAllEnemies();
    }

    private void EnterLevelingMode()
    {
        Debug.Log("Entered leveling mode at the totem!");
        if (levelingSystem != null)
        {
            levelingSystem.ActivateLeveling(this);
            PlayerController.instance.UnlockCursor(); // Force unlock cursor
        }
    }


    private void UpdateInteractionText()
    {
        if (interactionTextObject != null && isSitting)
        {
            TextMeshProUGUI textComponent = interactionTextObject.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"> {options[currentSelection]} <";
            }
        }
    }

    private void LeaveLevelingMode()
    {
        LeaveTotem();
    }

}