using MySqlConnector;
using UnityEngine;
using System.Collections.Generic;

public class MotorController : MonoBehaviour
{
    // Reference to the display prefabs (for each of the numeric topics)
    public DisplayController busFreqDisplay;
    public DisplayController busVoltageDisplay;
    public DisplayController cosPhiDisplay;
    public DisplayController currentDisplay;
    public DisplayController freqLoadDisplay;
    public DisplayController genFreqDisplay;

    // Reference to the boolean display for DG1-RUN
    public BooleanDisplayController runDisplay;

    // Reference to the boolean display for DG1-INHIB
    public BooleanDisplayController inhibDisplay;  // New boolean display controller for DG1-INHIB

    // Reference to the MotorAnimator for motor rumbling animation
    public Animator motorAnimator;  // Reference to the MotorAnimator

    // List of topics to fetch from the database for numeric displays
    private List<string> numericTopics = new List<string>
    {
        "TunglabDecoded/DG1-BUS-FRQ",
        "TunglabDecoded/DG1-BUS-V",
        "TunglabDecoded/DG1-COS-PHI",
        "TunglabDecoded/DG1-CURR",
        "TunglabDecoded/DG1-FRQ-LOD",
        "TunglabDecoded/DG1-GEN-FRQ"
    };

    // Corresponding list of DisplayControllers for numeric topics
    private List<DisplayController> numericDisplayControllers;

    // Boolean topics for DG1-RUN and DG1-INHIB
    private string runTopic = "TunglabDecoded/DG1-RUN";
    private string inhibTopic = "TunglabDecoded/DG1-INHIB";  // New boolean topic for DG1-INHIB

    private void Start()
    {
        // Initialize the numeric displayControllers list
        numericDisplayControllers = new List<DisplayController>
        {
            busFreqDisplay,
            busVoltageDisplay,
            cosPhiDisplay,
            currentDisplay,
            freqLoadDisplay,
            genFreqDisplay
        };

        // Ensure that the number of displays matches the number of numeric topics
        if (numericDisplayControllers.Count != numericTopics.Count)
        {
            Debug.LogError("Number of numeric display controllers doesn't match the number of numeric topics!");
            return;
        }

        // Set the data types (labels) for each numeric display
        for (int i = 0; i < numericDisplayControllers.Count; i++)
        {
            numericDisplayControllers[i].SetDataType(GetDataTypeFromTopic(numericTopics[i]));
        }

        // Set the friendly tag names for the Boolean displays
        runDisplay.SetTagName("Running");  // Friendly name for DG1-RUN
        inhibDisplay.SetTagName("Inhibited");  // Friendly name for DG1-INHIB

        // Start continuously updating the displays
        InvokeRepeating(nameof(UpdateDisplaysFromDatabase), 0f, 2f); // Call every 2 seconds
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

                // Handle numeric topics
                for (int i = 0; i < numericTopics.Count; i++)
                {
                    // Query to get the highest idx (latest entry) for each tagname
                    string query = @"
                        SELECT tagvalue 
                        FROM Gruppe3Unity 
                        WHERE tagname = @tagname 
                        ORDER BY idx DESC 
                        LIMIT 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tagname", numericTopics[i]);

                        var result = await command.ExecuteScalarAsync(); // Execute query asynchronously
                        if (result != null)
                        {
                            // Parse the result to a float and update the numeric display
                            if (float.TryParse(result.ToString(), out float value))
                            {
                                numericDisplayControllers[i].UpdateValue(value); // Update display with the new value
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to parse value for topic: {numericTopics[i]}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No data found for topic: {numericTopics[i]}");
                        }
                    }
                }

                // Handle Boolean topic (DG1-RUN)
                {
                    string query = @"
                        SELECT tagvalue 
                        FROM Gruppe3Unity 
                        WHERE tagname = @tagname 
                        ORDER BY idx DESC 
                        LIMIT 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tagname", runTopic);

                        var result = await command.ExecuteScalarAsync(); // Execute query asynchronously
                        if (result != null)
                        {
                            // Parse the result to a float (expecting 0 or 1 for Boolean)
                            if (float.TryParse(result.ToString(), out float value))
                            {
                                runDisplay.UpdateValue(value); // Update Boolean display

                                // Set the isRunning parameter in MotorAnimator to true or false based on the value
                                bool isRunning = value != 0;
                                motorAnimator.SetBool("isRunning", isRunning);  // Set Animator parameter
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to parse value for topic: {runTopic}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No data found for topic: {runTopic}");
                        }
                    }
                }

                // Handle Boolean topic (DG1-INHIB)
                {
                    string query = @"
                        SELECT tagvalue 
                        FROM Gruppe3Unity 
                        WHERE tagname = @tagname 
                        ORDER BY idx DESC 
                        LIMIT 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tagname", inhibTopic);

                        var result = await command.ExecuteScalarAsync(); // Execute query asynchronously
                        if (result != null)
                        {
                            // Parse the result to a float (expecting 0 or 1 for Boolean)
                            if (float.TryParse(result.ToString(), out float value))
                            {
                                inhibDisplay.UpdateValue(value); // Update Boolean display for DG1-INHIB
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to parse value for topic: {inhibTopic}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No data found for topic: {inhibTopic}");
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

    // Helper function to extract data type from the topic (e.g., "BUS-FRQ" -> "Frequency (Hz)")
    private string GetDataTypeFromTopic(string topic)
    {
        if (topic.Contains("BUS-FRQ")) return "BUS HZ";
        if (topic.Contains("BUS-V")) return "Voltage";
        if (topic.Contains("COS-PHI")) return "Cos φ";
        if (topic.Contains("CURR")) return "Current";
        if (topic.Contains("FRQ-LOD")) return "Load";
        if (topic.Contains("GEN-FRQ")) return "Gen HZ";
        if (topic.Contains("RUN")) return "Running"; // Data type for Boolean DG1-RUN
        if (topic.Contains("INHIB")) return "Inhibited"; // Data type for Boolean DG1-INHIB
        return "Unknown";
    }
}
