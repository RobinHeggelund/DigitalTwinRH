using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuCanvasController : MonoBehaviour
{
    public GameObject documentationPanel; // Attach object 1 (Documentation panel)
    public GameObject controlsPanel;      // Attach object 2 (Controls panel)
    public Button buttonLogout;           // Reference to Button_Logout
    public Button buttonDocumentation;    // Reference to Button_Documentation
    public Button buttonControls;         // Reference to Button_Controls

    private bool isMenuOpen => gameObject.activeSelf; // Check if this menu is active

    private void Start()
    {
        // Ensure panels are initially hidden
        if (documentationPanel != null) documentationPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // Attach button click listeners
        if (buttonLogout != null)
            buttonLogout.onClick.AddListener(OnLogoutClicked);
        if (buttonDocumentation != null)
            buttonDocumentation.onClick.AddListener(OnDocumentationClicked);
        if (buttonControls != null)
            buttonControls.onClick.AddListener(OnControlsClicked);

        // Ensure mouse settings are correct based on the menu state
        UpdateMouseState();
    }

    private void Update()
    {
        // Absorb all input if the menu is open
        if (isMenuOpen && Input.anyKeyDown)
        {
            Debug.Log("Input absorbed by the menu.");
        }
    }

    private void OnEnable()
    {
        UpdateMouseState(); // Enable mouse when the menu is opened
    }

    private void OnDisable()
    {
        UpdateMouseState(); // Disable mouse when the menu is closed
    }

    private void UpdateMouseState()
    {
        if (isMenuOpen)
        {
            Time.timeScale = 0f; // Pause the game
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor
            Cursor.visible = true; // Show the cursor
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
            Cursor.visible = false; // Hide the cursor
        }
    }

    // Handle logout button click
    private void OnLogoutClicked()
    {
        Debug.Log("Logging out and navigating to MainMenuScene.");
        Time.timeScale = 1f; // Ensure the game is resumed before changing the scene
        SceneManager.LoadScene("MainMenuScene"); // Replace with your actual main menu scene name
    }

    // Handle documentation button click
    private void OnDocumentationClicked()
    {
        Debug.Log("Displaying Documentation panel.");
        if (documentationPanel != null)
        {
            documentationPanel.SetActive(true);
            controlsPanel.SetActive(false); // Hide other panels if necessary
        }
    }

    // Handle controls button click
    private void OnControlsClicked()
    {
        Debug.Log("Displaying Controls panel.");
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
            documentationPanel.SetActive(false); // Hide other panels if necessary
        }
    }
}
