using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;

public class MeshCycler : MonoBehaviour
{
    public GameObject[] meshes; // Array to hold the different checklist meshes
    public Transform hand; // Reference to the hand/pen GameObject
    public Transform book; // Reference to the book object for open/close animation
    public GameObject[] checkmarks; // Array to hold the 25 checkmark GameObjects
    public GameObject inputFieldUI; // UI Panel containing Input Field and Submit Button
    public TMP_InputField nameInputField; // Reference to the TMP Input Field
    public Button submitButton; // Reference to the Submit Button
    public Vector3 bookClosedPosition; // Local position of the book when closed
    public float scrollSpeed = 0.2f; // Speed of the hand movement
    public float bookMoveSpeed = 5f; // Speed at which the book moves between open and closed positions
    public KeyCode markItemKey = KeyCode.Mouse0; // Mouse left-click to mark/unmark items
    public KeyCode toggleBookKey = KeyCode.Mouse1; // Right mouse button to toggle book open/close
    public GameObject objectToDisable;
    public GameObject controllerDisplay;
    public GameObject menu; // Assign your menu GameObject in the Unity Editor
    private Vector3 bookOpenPosition; // Dynamically assigned book's open position
    private int currentMeshIndex = 0; // Index to track the currently active mesh
    private int currentItemIndex = 0; // Index to track the currently selected item on the list
    private Dictionary<int, List<bool>> checklistStates; // Stores the state (marked/unmarked) for all meshes
    private Dictionary<int, int> currentItemIndexes; // Stores the current item index for each checklist
    private bool isBookOpen = true; // Tracks whether the book is open
    private bool isAskingForName = false; // Tracks if we're in the name input state
    private bool sim = false;

    void Start()
    {   
        // Hide the mouse 

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Make the cursor invisible

        // Capture the book's current position as the open position
        bookOpenPosition = book.localPosition;
        sim = PlayerPrefs.GetInt("sim", 0) == 1; // Default to false (0) if not set

        if (!sim && objectToDisable != null)
        {
            objectToDisable.SetActive(false);
            controllerDisplay.SetActive(false);
        }

        // Initialize checklist states and item indexes for all meshes
        checklistStates = new Dictionary<int, List<bool>>();
        currentItemIndexes = new Dictionary<int, int>();
        for (int i = 0; i < meshes.Length; i++)
        {
            checklistStates[i] = new List<bool>(new bool[checkmarks.Length]);
            currentItemIndexes[i] = 0; // Start at the top of each checklist
        }

        // Ensure all checkmarks are initially hidden
        foreach (var checkmark in checkmarks)
        {
            checkmark.SetActive(false);
        }

        // Hide the Input Field UI at the start
        if (inputFieldUI != null)
        {
            inputFieldUI.SetActive(false);
        }

        // Add listener to the Submit Button
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitName);
        }

        // Show only the first mesh at the start
        ShowOnlyCurrentMesh();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (menu != null)
            {
                bool isActive = menu.activeSelf;

                // Toggle menu visibility
                menu.SetActive(!isActive);

                // Reverse logic for another object
                if (sim && controllerDisplay != null)
                {
                    controllerDisplay.SetActive(isActive); // Set the opposite state
                }

                Debug.Log($"Menu is now {(isActive ? "closed" : "open")}");
            }
            else
            {
                Debug.LogWarning("Menu GameObject is not assigned!");
            }
        }

        // Toggle book open/close with the right mouse button
        if (Input.GetKeyDown(toggleBookKey))
        {
            isBookOpen = !isBookOpen;
            Debug.Log($"Book is now {(isBookOpen ? "open" : "closed")}");
        }

        // Smoothly move the book to its open or closed position
        book.localPosition = Vector3.Lerp(
            book.localPosition,
            isBookOpen ? bookOpenPosition : bookClosedPosition,
            Time.deltaTime * bookMoveSpeed
        );

        // Disable interaction when the book is closed or we're asking for a name
        if (!isBookOpen || isAskingForName)
        {
            if (isAskingForName && Input.GetKeyDown(KeyCode.Return))
            {
                submitButton.onClick.Invoke(); // Programmatically invoke the button click
            }

            return;
        }

        // Cycle through meshes using Q and E
        if (Input.GetKeyDown(KeyCode.E))
        {
            SaveCurrentItemIndex(); // Save the current position before switching
            currentMeshIndex++;
            if (currentMeshIndex >= meshes.Length)
            {
                currentMeshIndex = 0; // Loop back to the first mesh
            }
            RestoreCurrentItemIndex(); // Restore the position for the new page
            ShowOnlyCurrentMesh();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SaveCurrentItemIndex(); // Save the current position before switching
            currentMeshIndex--;
            if (currentMeshIndex < 0)
            {
                currentMeshIndex = meshes.Length - 1; // Loop back to the last mesh
            }
            RestoreCurrentItemIndex(); // Restore the position for the new page
            ShowOnlyCurrentMesh();
        }

        // Only allow interaction if we are on the checklist page
        if (currentMeshIndex == 0) // Assuming the first mesh is the checklist
        {
            hand.gameObject.SetActive(true); // Show the hand/pen

            // Scroll through items on the checklist using the mouse wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentItemIndex -= Mathf.RoundToInt(scroll / Mathf.Abs(scroll)); // Scroll up or down
                currentItemIndex = Mathf.Clamp(currentItemIndex, 0, checkmarks.Length - 1);

                // Move the hand along its local Y-axis
                float targetY = -2.235f + scrollSpeed + currentItemIndex * -scrollSpeed; // Assume negative scroll moves hand downward
                hand.localPosition = new Vector3(hand.localPosition.x, targetY, hand.localPosition.z);
            }

            // Mark or unmark the currently selected item
            if (Input.GetKeyDown(markItemKey))
            {
                checklistStates[currentMeshIndex][currentItemIndex] = !checklistStates[currentMeshIndex][currentItemIndex];
                checkmarks[currentItemIndex].SetActive(checklistStates[currentMeshIndex][currentItemIndex]);
                Debug.Log($"Item {currentItemIndex + 1} marked: {checklistStates[currentMeshIndex][currentItemIndex]}");
            }

            // Export checklist to PDF if all items are marked
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (AllItemsMarked())
                {
                    AskForName(); // Show the input field to ask for the player's name
                }
                else
                {
                    Debug.LogWarning("Checklist cannot be exported until all items are marked!");
                }
            }
        }
        else
        {
            hand.gameObject.SetActive(false); // Hide the hand/pen
            HideCheckmarks(); // Hide all checkmarks on non-checklist pages
        }
    }

    // Save the current position (item index) for the current checklist
    void SaveCurrentItemIndex()
    {
        currentItemIndexes[currentMeshIndex] = currentItemIndex;
    }

    // Restore the position (item index) when switching back to a checklist
    void RestoreCurrentItemIndex()
    {
        currentItemIndex = currentItemIndexes[currentMeshIndex];

        // Move the hand to the restored position
        float targetY = -2.235f + scrollSpeed + currentItemIndex * -scrollSpeed;
        hand.localPosition = new Vector3(hand.localPosition.x, targetY, hand.localPosition.z);
    }

    // Show only the current mesh and hide all others
    void ShowOnlyCurrentMesh()
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].SetActive(i == currentMeshIndex);
        }

        if (currentMeshIndex == 0)
        {
            // Restore checkmarks for the checklist page
            RestoreCheckmarks();
        }
        else
        {
            // Hide checkmarks on non-checklist pages
            HideCheckmarks();
        }

        Debug.Log($"Switched to page {currentMeshIndex + 1}");
    }

    // Restore checkmarks for the checklist page
    void RestoreCheckmarks()
    {
        for (int i = 0; i < checkmarks.Length; i++)
        {
            checkmarks[i].SetActive(checklistStates[currentMeshIndex][i]);
        }
    }

    // Hide all checkmarks
    void HideCheckmarks()
    {
        foreach (var checkmark in checkmarks)
        {
            checkmark.SetActive(false);
        }
    }

    // Check if all items on the checklist are marked
    bool AllItemsMarked()
    {
        foreach (bool marked in checklistStates[currentMeshIndex])
        {
            if (!marked) return false; // If any item is unmarked, return false
        }
        return true; // All items are marked
    }

    // Show the input field to ask for the player's name
    void AskForName()
    {
        isAskingForName = true;
        inputFieldUI.SetActive(true);

        // Focus on the input field
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    // Called when the player submits their name
    void OnSubmitName()
    {
        string playerName = nameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            Debug.Log($"Player name submitted: {playerName}");
            ExportToPDF(playerName); // Export the checklist with the player's name
            CloseNameInput();
        }
        else
        {
            Debug.LogWarning("Name cannot be empty!");
        }
    }

    // Close the name input UI
    void CloseNameInput()
    {
        inputFieldUI.SetActive(false);
        isAskingForName = false;
        nameInputField.text = ""; // Clear the input field
    }

    // Export marked checklist items to a PDF
    void ExportToPDF(string playerName)
    {
        try
        {
            string blankPdfPath = Path.Combine(Application.dataPath, "ChecklistBlank.pdf");

            if (!File.Exists(blankPdfPath))
            {
                Debug.LogError("Blank checklist PDF not found!");
                return;
            }

            string dateTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH"); // Year-Month-Day_Hour
            string fileName = $"DocumentF7211_{playerName}_{dateTime}.pdf";

            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string destinationPath = Path.Combine(desktopPath, fileName);

            File.Copy(blankPdfPath, destinationPath, overwrite: true);

            Debug.Log($"Checklist PDF copied and renamed to: {destinationPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to copy and rename the PDF: {ex.Message}");
        }
    }
}
