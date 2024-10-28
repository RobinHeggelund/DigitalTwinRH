using UnityEngine;
using TMPro;

public class DisplayController : MonoBehaviour
{
    // Reference to the TextMeshPro component showing the value
    public TextMeshPro valueText;

    // Reference to the TextMeshPro component showing the data type (RPM, kW, etc.)
    public TextMeshPro dataTypeText;

    // Method to update the display's value
    public void UpdateValue(float newValue)
    {
        valueText.text = newValue.ToString("F2");  // Format with 2 decimal places
    }

    // Method to set the display's data type (e.g., RPM, kW)
    public void SetDataType(string dataType)
    {
        dataTypeText.text = dataType;
    }
}
