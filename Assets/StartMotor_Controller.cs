using MySqlConnector;
using UnityEngine;
using System.Collections.Generic;

public class StartMotor_Controller : MonoBehaviour
{
    // Reference to the display prefabs for numeric topics
    public DisplayController rpmDisplay;
    public DisplayController tempCyl1Display;
    public DisplayController tempCyl2Display;
    public DisplayController tempCyl3Display;
    public DisplayController tempCyl4Display;
    public DisplayController tempCyl5Display;

    public DisplayController tempBearing1Display;
    public DisplayController tempBearing2Display;
    public DisplayController tempBearing3Display;
    public DisplayController tempBearing4Display;
    public DisplayController tempBearing5Display;

    public DisplayController genTempL1Display;
    public DisplayController genTempL2Display;
    public DisplayController genTempL3Display;

    // Reference to the boolean display for RUNNING
    public BooleanDisplayController runDisplay;

    // Reference to the MotorAnimator for motor animation
    public Animator motorAnimator;

    // List of topics to fetch from the database for numeric displays
    private List<string> numericTopics = new List<string>
    {
        "TunglabDecoded/ME RPM > 475",     // MOTOR RPM
        "TunglabDecoded/601 TE 305",      // TEMP EXHAUSTGASS CYL 1
        "TunglabDecoded/601 TE 306",      // TEMP EXHAUSTGASS CYL 2
        "TunglabDecoded/601 TE 307",      // TEMP EXHAUSTGASS CYL 3
        "TunglabDecoded/601 TE 308",      // TEMP EXHAUSTGASS CYL 4
        "TunglabDecoded/601 TE 309",      // TEMP EXHAUSTGASS CYL 5
        "TunglabDecoded/601 TE 401",      // TEMP MAIN BEARING 1
        "TunglabDecoded/601 TE 402",      // TEMP MAIN BEARING 2
        "TunglabDecoded/601 TE 403",      // TEMP MAIN BEARING 3
        "TunglabDecoded/601 TE 404",      // TEMP MAIN BEARING 4
        "TunglabDecoded/601 TE 405",      // TEMP MAIN BEARING 5
        "TunglabDecoded/667 TE 001",      // GENERATOR WINDING TEMPERATURE L1
        "TunglabDecoded/667 TE 002",      // GENERATOR WINDING TEMPERATURE L2
        "TunglabDecoded/667 TE 003"       // GENERATOR WINDING TEMPERATURE L3
    };

    // Corresponding list of DisplayControllers for numeric topics
    private List<DisplayController> numericDisplayControllers;

    // Boolean topic for RUNNING
    private string runTopic = "TunglabDecoded/ME RUN";

    // Simulation flag
    private bool sim;

    // Simulated values
    private float rpm = 1500.0f;           // Motor RPM
    private float[] cylTemps = { 300.0f, 310.0f, 320.0f, 330.0f, 340.0f }; // Cylinder temperatures
    private float[] bearingTemps = { 50.0f, 52.0f, 54.0f, 56.0f, 58.0f }; // Bearing temperatures
    private float[] genTemps = { 70.0f, 72.0f, 74.0f };                   // Generator winding temperatures

    private void Start()
    {
        // Check if we are in simulation mode
        sim = PlayerPrefs.GetInt("sim", 0) == 1; // Default to false (0) if not set

        // Initialize the numeric displayControllers list
        numericDisplayControllers = new List<DisplayController>
        {
            rpmDisplay,
            tempCyl1Display,
            tempCyl2Display,
            tempCyl3Display,
            tempCyl4Display,
            tempCyl5Display,
            tempBearing1Display,
            tempBearing2Display,
            tempBearing3Display,
            tempBearing4Display,
            tempBearing5Display,
            genTempL1Display,
            genTempL2Display,
            genTempL3Display
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

        // Set the friendly tag name for the RUNNING display
        runDisplay.SetTagName("Running");

        // Start continuously updating the displays
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
        rpm += Random.Range(-10f, 10f);
        for (int i = 0; i < cylTemps.Length; i++)
        {
            cylTemps[i] += Random.Range(-5f, 5f);
        }
        for (int i = 0; i < bearingTemps.Length; i++)
        {
            bearingTemps[i] += Random.Range(-1f, 1f);
        }
        for (int i = 0; i < genTemps.Length; i++)
        {
            genTemps[i] += Random.Range(-2f, 2f);
        }

        // Update the displays with the simulated values
        numericDisplayControllers[0].UpdateValue(rpm); // Motor RPM
        for (int i = 0; i < cylTemps.Length; i++)
        {
            numericDisplayControllers[i + 1].UpdateValue(cylTemps[i]);
        }
        for (int i = 0; i < bearingTemps.Length; i++)
        {
            numericDisplayControllers[i + 6].UpdateValue(bearingTemps[i]);
        }
        for (int i = 0; i < genTemps.Length; i++)
        {
            numericDisplayControllers[i + 11].UpdateValue(genTemps[i]);
        }

        // Simulate boolean values
        bool isRunning = true;
        runDisplay.UpdateValue(isRunning ? 1 : 0);

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
                await connection.OpenAsync(); // Open connection asynchronously
                Debug.Log("Database connection successful.");

                // Handle numeric topics
                for (int i = 0; i < numericTopics.Count; i++)
                {
                    string query = @"
                        SELECT tagvalue 
                        FROM Gruppe3_MainEngine
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

                // Handle Boolean topic (RUNNING)
                string queryBoolean = @"
                    SELECT tagvalue 
                    FROM Gruppe3_MainEngine 
                    WHERE tagname = @tagname 
                    ORDER BY idx DESC 
                    LIMIT 1";

                using (var command = new MySqlCommand(queryBoolean, connection))
                {
                    command.Parameters.AddWithValue("@tagname", runTopic);

                    var result = await command.ExecuteScalarAsync();
                    if (result != null && float.TryParse(result.ToString(), out float value))
                    {
                        runDisplay.UpdateValue(value);
                        motorAnimator.SetBool("isRunning", value != 0); // Update animator based on running status
                    }
                    else
                    {
                        Debug.LogWarning($"No valid data found for topic: {runTopic}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.LogError($"Database error: {ex.Message}");
            }
        }
    }

    // Helper function to extract data type from the topic (e.g., "ME RPM > 475" -> "MOTOR RPM")
    private string GetDataTypeFromTopic(string topic)
    {
        if (topic.Contains("ME RPM")) return "RPM";
        if (topic.Contains("TE 305")) return "Ex T1";
        if (topic.Contains("TE 306")) return "Ex T2";
        if (topic.Contains("TE 307")) return "Ex T3";
        if (topic.Contains("TE 308")) return "Ex T4";
        if (topic.Contains("TE 309")) return "Ex T5";
        if (topic.Contains("TE 401")) return "MB T1";
        if (topic.Contains("TE 402")) return "MB T2";
        if (topic.Contains("TE 403")) return "MB T3";
        if (topic.Contains("TE 404")) return "MB T4";
        if (topic.Contains("TE 405")) return "MB T5";
        if (topic.Contains("667 TE 001")) return "L1";
        if (topic.Contains("667 TE 002")) return "L2";
        if (topic.Contains("667 TE 003")) return "L3";
        if (topic.Contains("ME RUN")) return "Running"; // Data type for RUNNING
        return "Unknown";
    }
}
