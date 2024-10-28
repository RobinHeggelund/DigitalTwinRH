using UnityEngine;
using TMPro;  // For handling TextMeshPro

public class BooleanDisplayController : MonoBehaviour
{
    // Reference to the gray object (for false state)
    public GameObject grayObject;

    // Reference to the green object (for true state)
    public GameObject greenObject;

    // Reference to the TextMeshPro component showing the tag name
    public TextMeshPro tagNameText;

    // Method to update the display based on the value (0 for gray, non-zero for green)
    public void UpdateValue(float newValue)
    {
        if (newValue == 0)
        {
            // Display the gray object, hide the green object
            grayObject.SetActive(true);
            greenObject.SetActive(false);
        }
        else
        {
            // Display the green object, hide the gray object
            grayObject.SetActive(false);
            greenObject.SetActive(true);
        }
    }

    // Method to set the tag name (e.g., "DG1-BUS-FRQ")
    public void SetTagName(string tagName)
    {
        if (tagNameText != null)
        {
            tagNameText.text = tagName;
        }
    }
}
