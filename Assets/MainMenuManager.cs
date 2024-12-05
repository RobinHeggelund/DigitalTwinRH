using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MySqlConnector;
using System;

public class MainMenuManager : MonoBehaviour
{
    // UI Elements
    public GameObject loginPanel;            // Login panel
    public TMP_InputField InputField_Username; // Username input field (TMP)
    public TMP_InputField InputField_Password; // Password input field (TMP)
    public TMP_Dropdown dropdownTungLine;    // Dropdown for "TungLine"

    // Buttons
    public Button buttonDigitalTwin;
    public Button buttonEducational;
    public Button buttonExit;
    public Button buttonLogin;
    public Button buttonLoginBack;

    // MySQL connection string
    private string connectionString = "Server=192.168.38.100;Database=tunglab;User ID=remoteuser;Password=123456;";

    private void Start()
    {
        // Assign button listeners
        buttonDigitalTwin.onClick.AddListener(OnDigitalTwinClick);
        buttonEducational.onClick.AddListener(OnEducationalClick);
        buttonExit.onClick.AddListener(OnExitClick);
        buttonLogin.onClick.AddListener(OnLoginClick);
        buttonLoginBack.onClick.AddListener(OnLoginBackClick);

        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible

        // Add listener to the dropdown
        dropdownTungLine.onValueChanged.AddListener(OnDropdownValueChanged);

        // Initially disable buttons until "TungLine" is selected
        UpdateButtonStates();
    }

    // Method to update button states based on dropdown value
    private void UpdateButtonStates()
    {
        bool isTungLineSelected = dropdownTungLine.options[dropdownTungLine.value].text == "TungLine";
        buttonDigitalTwin.interactable = isTungLineSelected;
        buttonEducational.interactable = isTungLineSelected;
    }

    // Dropdown value change handler
    private void OnDropdownValueChanged(int value)
    {
        UpdateButtonStates();
    }

    // Method for "Digital Twin" button
    public void OnDigitalTwinClick()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }
    }

    // Method for "Login Back" button
    public void OnLoginBackClick()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(false);
        }
    }

    // Method for "Educational" button
    public void OnEducationalClick()
    {
        PlayerPrefs.SetInt("sim", 1); // Use 1 for true
        PlayerPrefs.Save();
        SceneManager.LoadScene("Playground");
    }

    // Method for "Exit" button
    public void OnExitClick()
    {
        Application.Quit();
        Debug.Log("Application Quit");
    }

    // Method for "Login" button
    public async void OnLoginClick()
    {
        string username = InputField_Username.text;
        string password = InputField_Password.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Username or password cannot be empty.");
            return;
        }

        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                Debug.Log("Database connection successful.");

                string query = "SELECT COUNT(*) FROM Gruppe3_Login WHERE username = @username AND password = @password";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    var result = await command.ExecuteScalarAsync();
                    int count = result != null ? Convert.ToInt32(result) : 0;

                    if (count > 0)
                    {
                        PlayerPrefs.SetInt("sim", 0); // Use 0 for false
                        PlayerPrefs.Save();

                        Debug.Log("Login successful. Loading Playground...");
                        SceneManager.LoadScene("Playground");
                    }
                    else
                    {
                        Debug.LogWarning("Invalid username or password.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError($"Database error: {ex.Message}");
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
