using UnityEngine;
using System;
using System.IO;

public class DataRecorder : MonoBehaviour
{
    public static DataRecorder Instance { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void SaveData(TimeSpan DurationAnesthesia, TimeSpan DurationBeforeFirstInsertion, TimeSpan DurationToCompleteMiddle, int NbInsertion, int NbTouchNerve, int NbTouchVein, int NbTouchArtery)
    {
        // Path to the CSV file in the specific location
        string folderPath = @"C:\Users\Devadmin\Documents\HusseinAMMAR\Anesthesia-simulator new";
        string filePath = Path.Combine(folderPath, "data-recorded.csv");
        Debug.Log("CSV file path: " + filePath);

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Prepare the CSV headers and data
        string[] csvHeaders = new string[]
        {
            "Date",
            "Duration of Anesthesia",
            "Time Before First Insertion",
            "Time to Complete 50% of Anesthesia",
            "Number of Needle Insertions",
            "Number of Nerves Touched",
            "Number of Veins Touched",
            "Number of Arteries Touched"
        };

        string[] csvData = new string[]
        {
            DateTime.Now.ToString(),
            string.Format("{0:D2}:{1:D2}", DurationAnesthesia.Minutes, DurationAnesthesia.Seconds),
            string.Format("{0:D2}:{1:D2}", DurationBeforeFirstInsertion.Minutes, DurationBeforeFirstInsertion.Seconds),
            string.Format("{0:D2}:{1:D2}", DurationToCompleteMiddle.Minutes, DurationToCompleteMiddle.Seconds),
            NbInsertion.ToString(),
            NbTouchNerve.ToString(),
            NbTouchVein.ToString(),
            NbTouchArtery.ToString()
        };

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true)) // 'true' to append data
            {
                // Check if the file exists to determine whether to write headers
                if (writer.BaseStream.Length == 0)
                {
                    // Write headers if the file is empty (newly created)
                    writer.WriteLine(string.Join(",", csvHeaders));
                }

                // Write data
                writer.WriteLine(string.Join(",", csvData));
            }

            Debug.Log("Data written successfully to: " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write file: " + e.Message);
        }
    }
}









