using MySqlConnector;
using UnityEngine;
using System.Collections.Generic;

public class DG2_Controller : MonoBehaviour
{
    // Reference to the display prefabs (for each of the numeric topics)
    public DisplayController busFreqDisplay;
    public DisplayController busVoltageDisplay;
    public DisplayController cosPhiDisplay;
    public DisplayController currentDisplay;
    public DisplayController freqLoadDisplay;
    public DisplayController genFreqDisplay;

    // Reference to the boolean display for DG2-RUN
    public BooleanDisplayController runDisplay;

    // Reference to the boolean display for DG2-INHIB
    public BooleanDisplayController inhibDisplay;

    // Reference to the MotorAnimator for motor rumbling animation
    public Animator motorAnimator;

    // List of topics to fetch from the database for numeric displays
    private List<string> numericTopics = new List<string>
    {
        "TunglabDecoded/DG2-BUS-FRQ",
        "TunglabDecoded/DG2-BUS-V",
        "TunglabDecoded/DG2-COS-PHI",
        "TunglabDecoded/DG2-CURR",
        "TunglabDecoded/DG2-FRQ-LOD",
        "TunglabDecoded/DG2-GEN-FRQ"
    };

    // Corresponding list of DisplayControllers for numeric topics
    private List<DisplayController> numericDisplayControllers;

    // Boolean topics for DG2-RUN and DG2-INHIB
    private string runTopic = "TunglabDecoded/DG2-RUN";
    private string inhibTopic = "TunglabDecoded/DG2-INHIB";

    // Simulation flag
    private bool sim;

    // Simulated values
    private float busFreq = 50.0f;   // Simulated frequency
    private float busVoltage = 230.0f; // Simulated voltage
    private float cosPhi = 0.9f;    // Simulated power factor
    private float current = 20.0f;  // Simulated current
    private float freqLoad = 1.0f;  // Simulated load frequency
    private float genFreq = 50.0f;  // Simulated generator frequency

    private void Start()
    {
        // Check if we are in simulation mode
        sim = PlayerPrefs.GetInt("sim", 0) == 1; // Default to false (0) if not set

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
        runDisplay.SetTagName("Running");
        inhibDisplay.SetTagName("Inhibited");

        // Start updating displays
        if (sim)
        {
            // Use simulated values
            InvokeRepeating(nameof(UpdateSimulatedValues), 0f, 0.1f); // Update every 0.1 seconds
        }
        else
        {
            // Fetch values from the database
            InvokeRepeating(nameof(UpdateDisplaysFromDatabase), 0f, 0.1f); // Update every 0.1 seconds
        }
    }

    // Method to simulate values
    private void UpdateSimulatedValues()
    {
        // Add small random variations to simulate changing values
        busFreq += Random.Range(-0.1f, 0.1f);
        busVoltage += Random.Range(-1f, 1f);
        cosPhi += Random.Range(-0.01f, 0.01f);
        current += Random.Range(-0.5f, 0.5f);
        freqLoad += Random.Range(-0.01f, 0.01f);
        genFreq += Random.Range(-0.1f, 0.1f);

        // Update the displays with the simulated values
        numericDisplayControllers[0].UpdateValue(busFreq);
        numericDisplayControllers[1].UpdateValue(busVoltage);
        numericDisplayControllers[2].UpdateValue(cosPhi);
        numericDisplayControllers[3].UpdateValue(current);
        numericDisplayControllers[4].UpdateValue(freqLoad);
        numericDisplayControllers[5].UpdateValue(genFreq);

        // Simulate boolean values
        bool isRunning = true;
        bool isInhibited = false;
        if (freqLoad > 1.0)
        {
            isInhibited = true;
        }


        runDisplay.UpdateValue(isRunning ? 1 : 0);
        inhibDisplay.UpdateValue(isInhibited ? 1 : 0);

        // Set the animator parameter based on running status
        motorAnimator.SetBool("isRunning", isRunning);
    }

    // Method to query the database and update each display
    private async void UpdateDisplaysFromDatabase()
    {
        string connectionString = "Server=192.168.38.100;Database=tunglab;User ID=remoteuser;Password=123456;";

        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                Debug.Log("Database connection successful.");

                // Handle numeric topics
                for (int i = 0; i < numericTopics.Count; i++)
                {
                    string query = @"
                        SELECT tagvalue 
                        FROM Gruppe3_DG2
                        WHERE tagname = @tagname 
                        ORDER BY idx DESC 
                        LIMIT 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tagname", numericTopics[i]);

                        var result = await command.ExecuteScalarAsync();
                        if (result != null && float.TryParse(result.ToString(), out float value))
                        {
                            numericDisplayControllers[i].UpdateValue(value);
                        }
                        else
                        {
                            Debug.LogWarning($"No valid data found for topic: {numericTopics[i]}");
                        }
                    }
                }

                // Handle Boolean topics (DG2-RUN and DG2-INHIB)
                UpdateBooleanValue(connection, runTopic, runDisplay);
                UpdateBooleanValue(connection, inhibTopic, inhibDisplay);
            }
            catch (MySqlException ex)
            {
                Debug.LogError($"Database error: {ex.Message}");
            }
        }
    }

    private async void UpdateBooleanValue(MySqlConnection connection, string topic, BooleanDisplayController displayController)
    {
        string query = @"
            SELECT tagvalue 
            FROM Gruppe3_DG2 
            WHERE tagname = @tagname 
            ORDER BY idx DESC 
            LIMIT 1";

        using (var command = new MySqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@tagname", topic);

            var result = await command.ExecuteScalarAsync();
            if (result != null && float.TryParse(result.ToString(), out float value))
            {
                displayController.UpdateValue(value);
            }
            else
            {
                Debug.LogWarning($"No valid data found for topic: {topic}");
            }
        }
    }

    // Helper function to extract data type from the topic
    private string GetDataTypeFromTopic(string topic)
    {
        if (topic.Contains("BUS-FRQ")) return "BUS HZ";
        if (topic.Contains("BUS-V")) return "Voltage";
        if (topic.Contains("COS-PHI")) return "Cos φ";
        if (topic.Contains("CURR")) return "Current";
        if (topic.Contains("FRQ-LOD")) return "Load";
        if (topic.Contains("GEN-FRQ")) return "Gen HZ";
        return "Unknown";
    }
}
