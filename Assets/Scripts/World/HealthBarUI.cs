using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Slider healthBarSlider; // Reference to the Slider component
    public PlayerController player; // Reference to the player

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If not assigned in the Inspector, try to find the player
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        // Ensure the slider is assigned
        if (healthBarSlider == null)
        {
            healthBarSlider = GetComponent<Slider>();
        }

        // Initialize the slider
        if (player != null)
        {
            healthBarSlider.value = player.GetHealthPercentage();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update the slider value based on the player's health
        if (player != null)
        {
            healthBarSlider.value = player.GetHealthPercentage();
        }
    }
}
