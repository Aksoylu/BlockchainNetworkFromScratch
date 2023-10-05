using YamlDotNet.Serialization;

namespace BlockchainNetwork;

class RuntimeConfig
{
    [YamlMember(Alias = "server_ip")]
    public required string ServerIp { get; set; }

    [YamlMember(Alias = "port")]
    public required int Port { get; set; }

    [YamlMember(Alias = "node_version")]
    public required double NodeVersion { get; set; }
    
    [YamlMember(Alias = "node_api_path")]
    public required string NodeApiPath { get; set; }

    [YamlMember(Alias = "metadata_path")]
    public required string MetaDataPath { get; set; }

    [YamlMember(Alias = "peer_endpoints")]
    public required List<string> PeerEndpoints { get; set; }

    public bool IsGenesisBlock {get; set;}
}