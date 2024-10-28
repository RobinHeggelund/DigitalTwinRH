using UnityEngine;

public class FloatDisplay : MonoBehaviour
{
    public TextMesh textMesh;  // Assign your 3D Text object here
    public float valueToDisplay = 0f;  // The float value you want to display

    void Start()
    {
        // You can initialize the textMesh reference here if needed
        if (textMesh == null)
            textMesh = GetComponent<TextMesh>();
    }

    void Update()
    {
        // Update the 3D Text with the current value of the float
        textMesh.text = valueToDisplay.ToString("F2");  // Display with 2 decimal places
    }

    // Call this function to update the float value
    public void UpdateValue(float newValue)
    {
        valueToDisplay = newValue;
    }
}
