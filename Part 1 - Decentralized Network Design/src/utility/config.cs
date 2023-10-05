using System.Data;
using YamlDotNet.Serialization;

namespace BlockchainNetwork;

class Config
{
    public static RuntimeConfig RuntimeConfig {get; private set;}

    public static void Load()
    {
        string yamlFilePath = "../config.yaml"; // Replace with the path to your YAML file

        try
        {
            // Create a deserializer
            var deserializer = new DeserializerBuilder().Build();

            // Read the YAML file content
            string yamlContent = File.ReadAllText(yamlFilePath);

            // Deserialize the YAML into your C# class
            RuntimeConfig config = deserializer.Deserialize<RuntimeConfig>(yamlContent);
            
            // Decide Node Is Genesis
            config.IsGenesisNode = IsGenesisNode(config);

            Config.RuntimeConfig = config;
        }
        catch (Exception e)
        {
            throw new DataException($"Error: Runtime Config Cannot Loaded {e.ToString()}");
        }
    }

    private static bool IsGenesisNode(RuntimeConfig config)
    {
        return (config.PeerEndpoints.Count == 1 && config.PeerEndpoints[0] == "GENESIS");
    }
}