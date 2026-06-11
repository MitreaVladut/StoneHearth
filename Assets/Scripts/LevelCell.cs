using UnityEngine;
using UnityEngine.UI;
public class LevelCell : MonoBehaviour
{
    public string category; // Set to "HP", "Earth", or "Magic" in Inspector

    private Image image; // Auto-fetched Image component

    void Start()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("Image component not found on " + gameObject.name);
        }
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (image == null) return;

        int level = 0;
        PlayerController player = PlayerController.instance;
        if (category == "HP") level = player.hpLevel;
        else if (category == "Earth") level = player.earthLevel;
        else if (category == "Magic") level = player.magicLevel;

        int rowIndex = transform.GetSiblingIndex() / 3; // 3 columns (HP, Earth, Magic)
        int colIndex = transform.GetSiblingIndex() % 3; // 0 = HP, 1 = Earth, 2 = Magic

        // Only update color if this cell's column matches its category's column
        if (colIndex == 0 && category == "HP" ||
            colIndex == 1 && category == "Earth" ||
            colIndex == 2 && category == "Magic")
        {
            if (level > 0 && level <= 4)
            {
                float greenValue = Mathf.Lerp(0.5f, 1.0f, (float)level / 4); // 0.5 (gray) to 1.0 (full green)
                if (rowIndex < level) // Color all rows up to the current level for this category
                {
                    image.color = new Color(0f, greenValue, 0f, 1.0f); // RGB: Red=0, Green=greenValue, Blue=0
                }
                else
                {
                    image.color = new Color(0.5f, 0.5f, 0.5f, 1.0f); // Gray for unlevelled rows
                }
            }
            else
            {
                image.color = new Color(0.5f, 0.5f, 0.5f, 1.0f); // Gray if no levels or maxed
            }
        }
    }
}
