using MySqlConnector;
using UnityEngine;
using System.Collections.Generic;

public class MotorController : MonoBehaviour
{
    // Outputs for DG1
    public DisplayController dg1BusFreqDisplay;
    public DisplayController dg1BusVoltageDisplay;
    public DisplayController dg1CosPhiDisplay;
    public DisplayController dg1CurrentDisplay;
    public DisplayController dg1FreqLoadDisplay;
    public DisplayController dg1GenFreqDisplay;

    // Outputs for DG2
    public DisplayController dg2BusFreqDisplay;
    public DisplayController dg2BusVoltageDisplay;
    public DisplayController dg2CosPhiDisplay;
    public DisplayController dg2CurrentDisplay;
    public DisplayController dg2FreqLoadDisplay;
    public DisplayController dg2GenFreqDisplay;

    // Outputs for Main Engine
    public DisplayController mainMotorRpmDisplay;
    public DisplayController mainTempExhaustCyl1Display;
    public DisplayController mainTempExhaustCyl2Display;
    public DisplayController mainTempExhaustCyl3Display;
    public DisplayController mainTempExhaustCyl4Display;
    public DisplayController mainTempExhaustCyl5Display;
    public DisplayController mainTempMainBearing1Display;
    public DisplayController mainTempMainBearing2Display;
    public DisplayController mainTempMainBearing3Display;
    public DisplayController mainTempMainBearing4Display;
    public DisplayController mainTempMainBearing5Display;
    public DisplayController mainGeneratorWindingTempL1Display;
    public DisplayController mainGeneratorWindingTempL2Display;
    public DisplayController mainGeneratorWindingTempL3Display;

    // Outputs for Oil Separator
    public BooleanDisplayController oilDrainboxAlarmDisplay;
    public BooleanDisplayController oilRunningDisplay;
    public BooleanDisplayController oilPrelubePressureLowDisplay;

    // Boolean Outputs for DG1 and DG2
    public BooleanDisplayController dg1RunDisplay;
    public BooleanDisplayController dg1InhibDisplay;
    public BooleanDisplayController dg2RunDisplay;
    public BooleanDisplayController dg2InhibDisplay;

    // List of topics for DG1, DG2, Main Engine, and Oil Separator
    private Dictionary<string, List<string>> numericTopics = new Dictionary<string, List<string>>
    {
        { "DG1", new List<string>
            {
                "TunglabDecoded/DG1-BUS-FRQ",
                "TunglabDecoded/DG1-BUS-V",
                "TunglabDecoded/DG1-COS-PHI",
                "TunglabDecoded/DG1-CURR",
                "TunglabDecoded/DG1-FRQ-LOD",
                "TunglabDecoded/DG1-GEN-FRQ"
            }
        },
        { "DG2", new List<string>
            {
                "TunglabDecoded/DG2-BUS-FRQ",
                "TunglabDecoded/DG2-BUS-V",
                "TunglabDecoded/DG2-COS-PHI",
                "TunglabDecoded/DG2-CURR",
                "TunglabDecoded/DG2-FRQ-LOD",
                "TunglabDecoded/DG2-GEN-FRQ"
            }
        },
        { "Main", new List<string>
            {
                "TunglabDecoded/ME RPM > 475",
                "TunglabDecoded/601 TE 305",
                "TunglabDecoded/601 TE 306",
                "TunglabDecoded/601 TE 307",
                "TunglabDecoded/601 TE 308",
                "TunglabDecoded/601 TE 309",
                "TunglabDecoded/601 TE 401",
                "TunglabDecoded/601 TE 402",
                "TunglabDecoded/601 TE 403",
                "TunglabDecoded/601 TE 404",
                "TunglabDecoded/601 TE 405",
                "TunglabDecoded/667 TE 001",
                "TunglabDecoded/667 TE 002",
                "TunglabDecoded/667 TE 003"
            }
        }
    };

    private Dictionary<string, Dictionary<string, string>> booleanTopics = new Dictionary<string, Dictionary<string, string>>
    {
        { "DG1", new Dictionary<string, string>
            {
                { "TunglabDecoded/DG1-RUN", "Run" },
                { "TunglabDecoded/DG1-INHIB", "Inhibit" }
            }
        },
        { "DG2", new Dictionary<string, string>
            {
                { "TunglabDecoded/DG2-RUN", "Run" },
                { "TunglabDecoded/DG2-INHIB", "Inhibit" }
            }
        },
        { "Oil", new Dictionary<string, string>
            {
                { "TunglabDecoded/DIG LAH 108", "Drainbox High Level Alarm" },
                { "TunglabDecoded/DIG ZS 407 107", "Running" },
                { "TunglabDecoded/DIG PAL 101", "Prelube Oil Pressure Low" }
            }
        }
    };

    private Dictionary<string, List<DisplayController>> numericDisplayControllers = new Dictionary<string, List<DisplayController>>();
    private Dictionary<string, Dictionary<string, BooleanDisplayController>> booleanDisplayControllers = new Dictionary<string, Dictionary<string, BooleanDisplayController>>();

    private void Start()
    {
        // Initialize numeric display controllers
        numericDisplayControllers["DG1"] = new List<DisplayController>
        {
            dg1BusFreqDisplay,
            dg1BusVoltageDisplay,
            dg1CosPhiDisplay,
            dg1CurrentDisplay,
            dg1FreqLoadDisplay,
            dg1GenFreqDisplay
        };

        numericDisplayControllers["DG2"] = new List<DisplayController>
        {
            dg2BusFreqDisplay,
            dg2BusVoltageDisplay,
            dg2CosPhiDisplay,
            dg2CurrentDisplay,
            dg2FreqLoadDisplay,
            dg2GenFreqDisplay
        };

        numericDisplayControllers["Main"] = new List<DisplayController>
        {
            mainMotorRpmDisplay,
            mainTempExhaustCyl1Display,
            mainTempExhaustCyl2Display,
            mainTempExhaustCyl3Display,
            mainTempExhaustCyl4Display,
            mainTempExhaustCyl5Display,
            mainTempMainBearing1Display,
            mainTempMainBearing2Display,
            mainTempMainBearing3Display,
            mainTempMainBearing4Display,
            mainTempMainBearing5Display,
            mainGeneratorWindingTempL1Display,
            mainGeneratorWindingTempL2Display,
            mainGeneratorWindingTempL3Display
        };

        // Initialize boolean display controllers
        booleanDisplayControllers["DG1"] = new Dictionary<string, BooleanDisplayController>
        {
            { "TunglabDecoded/DG1-RUN", dg1RunDisplay },
            { "TunglabDecoded/DG1-INHIB", dg1InhibDisplay }
        };

        booleanDisplayControllers["DG2"] = new Dictionary<string, BooleanDisplayController>
        {
            { "TunglabDecoded/DG2-RUN", dg2RunDisplay },
            { "TunglabDecoded/DG2-INHIB", dg2InhibDisplay }
        };

        booleanDisplayControllers["Oil"] = new Dictionary<string, BooleanDisplayController>
        {
            { "TunglabDecoded/DIG LAH 108", oilDrainboxAlarmDisplay },
            { "TunglabDecoded/DIG ZS 407 107", oilRunningDisplay },
            { "TunglabDecoded/DIG PAL 101", oilPrelubePressureLowDisplay }
        };

        InvokeRepeating(nameof(UpdateDisplaysFromDatabase), 0f, 2f);
    }

    private async void UpdateDisplaysFromDatabase()
    {
        string connectionString = "Server=192.168.38.100;Database=tunglab;User ID=remoteuser;Password=123456;";

        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();

                // Update numeric topics
                foreach (var system in numericTopics.Keys)
                {
                    for (int i = 0; i < numericTopics[system].Count; i++)
                    {
                        string query = $@"
                            SELECT tagvalue 
                            FROM {system} 
                            WHERE tagname = @tagname 
                            ORDER BY idx DESC 
                            LIMIT 1";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@tagname", numericTopics[system][i]);

                            var result = await command.ExecuteScalarAsync();
                            if (result != null && float.TryParse(result.ToString(), out float value))
                            {
                                numericDisplayControllers[system][i].UpdateValue(value);
                            }
                        }
                    }
                }

                // Update boolean topics
                foreach (var system in booleanTopics.Keys)
                {
                    foreach (var topic in booleanTopics[system].Keys)
                    {
                        string query = $@"
                            SELECT tagvalue 
                            FROM {system} 
                            WHERE tagname = @tagname 
                            ORDER BY idx DESC 
                            LIMIT 1";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@tagname", topic);

                            var result = await command.ExecuteScalarAsync();
                            if (result != null && float.TryParse(result.ToString(), out float value))
                            {
                                booleanDisplayControllers[system][topic].UpdateValue(value);
                            }
                        }
                    }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
