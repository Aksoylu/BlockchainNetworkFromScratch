using System.ComponentModel;
using System.Reflection;
using System.Security.Authentication;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

namespace BlockchainNetwork;

class PeerService
{
    public List<PeerModel> peerRepository = new List<PeerModel>();
    string peerPath;

    public PeerService()
    {
        if(!Config.RuntimeConfig.IsGenesisBlock && Config.RuntimeConfig.PeerEndpoints.Count != 0)
        {
            peerRepository = loadPeerRepositoryFromConfig(Config.RuntimeConfig.PeerEndpoints);
        }
        
        this.peerPath = $"{Config.RuntimeConfig.MetaDataPath}/peers.json";

        peerRepository = peerRepository.Concat(loadPeerRepository()).ToList();

        for(int i = 0; i < peerRepository.Count; i++)
        {
            Console.WriteLine(peerRepository[i].GetEndpoint());
        }
    }

    public JToken executePeerService(string methodName, JToken parameters)
    {
        MethodInfo? methodInfo = typeof(PeerService).GetMethod(methodName);
        if(methodInfo == null)
        {
            throw new RuntimeBinderException($"method {methodName} not found");
        }

        Object? executionResult = methodInfo.Invoke(this, new object[] {parameters});

        if (executionResult is JToken jTokenResult)
        {
            return jTokenResult;
        }

        throw new InvalidOperationException("Method error occured");
    }

    /**
    * @string {server_ip}: set by system
    * @int {port}
    * @double {version}
    * @string {api_path}
    */
    public JToken registerNewNode(JToken parameters)
    {
        // Sender server ip
        string? serverIp = (string)parameters["server_ip"] ?? null;

        string? nodeIp = (string)parameters["node_ip"] ?? null;
        int port = (int)parameters["port"];
        double version = (double)parameters["version"];
        string? apiPath = (string)parameters["api_path"] ?? null;

        if(serverIp == null || nodeIp == null || port == 0 || version == 0 || apiPath == null)
        {
            throw new WarningException("parameter mismatch");
        }

        if(isSenderBanned(serverIp))
        {
            throw new AuthenticationException("Sender peer is banner from network");
        }

        if(isPeerExist(nodeIp))
        {
            throw new AuthenticationException("Peer IP is already exist in network");
        }

        addToPeerRepository(nodeIp, port, version, apiPath);
        dumpPeerRepository();
        executeNewNodeBroadcast(nodeIp, port, version, apiPath);

        JObject response =  new JObject();
        response["result"] = "OK";
        return response;
    }

    // Excludes untrusted peers and sends data to all peers 
    private void executeNewNodeBroadcast(string nodeIp, int port, double version, string apiPath)
    {
        List<string> peerEndpointList = new List<string>();

        for(int i = 0; i < peerRepository.Count; i++)
        {
            PeerModel eachPeer = peerRepository[i];

            if(!eachPeer.trust)
                continue;

            peerEndpointList.Add(eachPeer.GetEndpoint());
        }

        JObject responseData = new JObject
        {
             ["nodeIp"] = nodeIp,
             ["port"] = port,
             ["version"] = version,
             ["apiPath"] = apiPath
        };

        RpcRequest.SendBroadcast(peerEndpointList, "registerNewNode", responseData);
    }

    private void addToPeerRepository(string serverIp, int port, double version, string  apiPath)
    {
        PeerModel peerModel = new PeerModel(serverIp, port, version, apiPath);
        peerRepository.Add(peerModel);
    }

    private bool isSenderBanned(string senderIp)
    {
        for(int i = 0; i < peerRepository.Count; i++)
        {
            PeerModel eachPeer = peerRepository[i];
            if(eachPeer.ipAddress == senderIp && !eachPeer.trust)
            {
                return true;
            }
        }
        return false;
    }

    private bool isPeerExist(string serverIp)
    {
        for(int i = 0; i < peerRepository.Count; i++)
        {
            PeerModel eachPeer = peerRepository[i];
            if(eachPeer.ipAddress == serverIp)
            {
                return true;
            }
        }

        return false;
    }

    // Write all peers to file.
    // Do not write peers that already existing in config.yaml
    private void dumpPeerRepository()
    {
        List<PeerModel> cleanedPeerRepository = new List<PeerModel>();
        peerRepository.ForEach(eachPeer => {
            if(!Config.RuntimeConfig.PeerEndpoints.Contains(eachPeer.GetEndpoint()))
            {
                cleanedPeerRepository.Add(eachPeer);
            }
        });

        Storage.WriteToJsonFile(peerPath, cleanedPeerRepository);
    }

    private List<PeerModel> loadPeerRepository()
    {
        List<PeerModel> loadedPeerList = Storage.LoadFromJsonFile<List<PeerModel>>(peerPath) ?? new List<PeerModel>();
        for(int i = 0; i < loadedPeerList.Count; i++)
        {
            Console.WriteLine(loadedPeerList[i].GetEndpoint());
        }
        return loadedPeerList;
    }

    private List<PeerModel> loadPeerRepositoryFromConfig(List<string> peerEndpointList)
    {
        List<PeerModel> peerList = new List<PeerModel>();

        for(int i = 0; i < peerEndpointList.Count; i++)
        {
            PeerModel newPeer = new PeerModel(peerEndpointList[i]);
            if(newPeer.ipAddress != null)
            {
                peerList.Add(newPeer);
            }
        }

        return peerList;
    }
}