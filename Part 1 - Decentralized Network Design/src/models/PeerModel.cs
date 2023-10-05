using System.Data;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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

    private string endpointParserPattern = @"^(\d+\.\d+\.\d+\.\d+):(\d+)/(.+)$";

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
        Match match = Regex.Match(peerUrl, endpointParserPattern);
        if (!match.Success)
        {
            throw new DataException("Peer url is invalid");
        }

        //peer1.example.com:8080/jsonrpc
        this.ipAddress = match.Groups[1].Value;
        this.port = Convert.ToInt32(match.Groups[2].Value);
        this.version = Config.RuntimeConfig.NodeVersion;
        this.apiPath = match.Groups[3].Value;
        this.serverHash = Crypt.HashSha256(this.ipAddress);
        this.trust = true;
    }
}