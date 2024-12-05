using UnityEngine;
using UnityEngine.UI;

public class DocumentationScript : MonoBehaviour
{
    public Button button1; // Reference to the first button
    public Button button2; // Reference to the second button
    public Button button3; // Reference to the third button
    public Button buttonClose; // Reference to the fourth button to hide the panel

    public string url1 = "https://vg.no"; // URL for the first button
    public string url2 = "https://unity.com";   // URL for the second button
    public string url3 = "https://google.com";  // URL for the third button

    private void Start()
    {
        // Add listeners to the buttons
        if (button1 != null)
            button1.onClick.AddListener(() => OpenURL(url1));
        if (button2 != null)
            button2.onClick.AddListener(() => OpenURL(url2));
        if (button3 != null)
            button3.onClick.AddListener(() => OpenURL(url3));
        if (buttonClose != null)
            buttonClose.onClick.AddListener(HidePanel);
    }

    // Method to open a URL
    private void OpenURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Debug.Log($"Opening URL: {url}");
            Application.OpenURL(url); // Open the URL in the default web browser
        }
        else
        {
            Debug.LogWarning("URL is empty or null!");
        }
    }

    // Method to hide the panel
    private void HidePanel()
    {
        Debug.Log("Hiding panel.");
        gameObject.SetActive(false); // Deactivate the panel
    }
}
