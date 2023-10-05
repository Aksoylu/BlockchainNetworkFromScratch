using System.Text.Json.Serialization;

namespace BlockchainNetwork;

class PeerModel
{
    public string ipAddress {get; private set;}
    public int port {get; private set;}
    public double version {get; private set;}
    public string apiPath {get; private set;}
    public string serverHash {get; private set;}
    public bool trust {get; private set;}
    
    public string GetEndpoint()
    {
        return $"{ipAddress}:{port}/{apiPath}"; 
    }

    [JsonConstructor]
    public PeerModel(string ipAddress, int port, double version, string apiPath)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.version = version;
        this.apiPath = apiPath;
        this.serverHash = Crypt.HashSha256(this.ipAddress);
        this.trust = true;
    }

    public PeerModel(string peerUrl)
    {
        this.ipAddress = peerUrl;
        this.port = 1;
        this.version = 1;
        this.apiPath = "apiPath";
        this.serverHash = "Crypt.HashSha256(this.ipAddress)";
        this.trust = true;
    }
}