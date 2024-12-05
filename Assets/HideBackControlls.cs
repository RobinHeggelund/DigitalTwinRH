using UnityEngine;
using UnityEngine.UI;

public class HideBackControlls : MonoBehaviour
{
    public GameObject targetObject; // Object to hide
    public Button buttonBackControls; // Reference to Button_Back_Controlls

    private void Start()
    {
        if (buttonBackControls != null)
        {
            // Attach the click listener to the button
            buttonBackControls.onClick.AddListener(HideTargetObject);
        }
    }

    private void HideTargetObject()
    {
        if (targetObject != null)
        {
            Debug.Log("Hiding target object.");
            targetObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Target object is not assigned!");
        }
    }
}
