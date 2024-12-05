using MySqlConnector;
using UnityEngine;
using System.Collections.Generic;

public class SeperatorOlje_Controller : MonoBehaviour
{
    // References to the Boolean display controllers for each topic
    public BooleanDisplayController drainboxAlarmDisplay; // Display for "Drainbox high level alarm"
    public BooleanDisplayController runningDisplay;      // Display for "Running"
    public BooleanDisplayController prelubePressureLowDisplay; // Display for "Prelube Oil Pressure Low"

    // List of topics to fetch from the database
    private List<string> booleanTopics = new List<string>
    {
        "TunglabDecoded/DIG LAH 108",   // Drainbox high level alarm
        "TunglabDecoded/DIG ZS 407 107", // Running
        "TunglabDecoded/DIG PAL 101"   // Prelube Oil Pressure Low
    };

    // Corresponding list of Boolean display controllers
    private List<BooleanDisplayController> booleanDisplayControllers;

    private void Start()
    {
        // Initialize the Boolean display controllers list
        booleanDisplayControllers = new List<BooleanDisplayController>
        {
            drainboxAlarmDisplay,
            runningDisplay,
            prelubePressureLowDisplay
        };

        // Ensure that the number of displays matches the number of Boolean topics
        if (booleanDisplayControllers.Count != booleanTopics.Count)
        {
            Debug.LogError("Number of Boolean display controllers doesn't match the number of Boolean topics!");
            return;
        }

        // Set the friendly tag names for the Boolean displays
        booleanDisplayControllers[0].SetTagName("Drainbox AH");
        booleanDisplayControllers[1].SetTagName("Running");
        booleanDisplayControllers[2].SetTagName("Pressure AL");

        // Start continuously updating the displays
        InvokeRepeating(nameof(UpdateDisplaysFromDatabase), 0f, 0.1f); // Call every 2 seconds
    }

    // Method to query the database and update each display
    private async void UpdateDisplaysFromDatabase()
    {
        string connectionString = "Server=192.168.38.100;Database=tunglab;User ID=remoteuser;Password=123456;";

        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync(); // Open connection asynchronously
                Debug.Log("Database connection successful.");

                // Handle Boolean topics
                for (int i = 0; i < booleanTopics.Count; i++)
                {
                    string query = @"
                        SELECT tagvalue 
                        FROM Gruppe3_OljeSeperator
                        WHERE tagname = @tagname 
                        ORDER BY idx DESC 
                        LIMIT 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tagname", booleanTopics[i]);

                        var result = await command.ExecuteScalarAsync(); // Execute query asynchronously
                        if (result != null)
                        {
                            // Parse the result to a float (expecting 0 or 1 for Boolean)
                            if (float.TryParse(result.ToString(), out float value))
                            {
                                booleanDisplayControllers[i].UpdateValue(value); // Update Boolean display
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to parse value for topic: {booleanTopics[i]}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No data found for topic: {booleanTopics[i]}");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError($"Database error: {ex.Message}");
            }
            finally
            {
                await connection.CloseAsync(); // Close connection asynchronously
            }
        }
    }
}
