using System;
using System.IO;
using System.Text.Json;

namespace BlockchainNetwork;

class StorageModule
{
    // Method to write JSON data to a file
    public static void WriteToJsonFile(string filePath, object data)
    {
        // Serialize the data to JSON format
        string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true // To format the JSON data with indentation
        });

        // Write the JSON data to the file
        File.WriteAllText(filePath, jsonData);
    }

    // Method to load JSON data from a file
    public static T? LoadFromJsonFile<T>(string filePath) where T : class
    {
        if (File.Exists(filePath))
        {
            // Read the JSON data from the file
            string jsonData = File.ReadAllText(filePath);

            // Serialize and return
            T? data = JsonSerializer.Deserialize<T>(jsonData.ToString());

            return data;
        }

        return default;
    }
}