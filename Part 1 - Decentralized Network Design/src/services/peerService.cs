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
        if(!ConfigModule.RuntimeConfig.IsGenesisBlock && ConfigModule.RuntimeConfig.PeerEndpoints.Count != 0)
        {
            peerRepository = loadPeerRepositoryFromConfig(ConfigModule.RuntimeConfig.PeerEndpoints);
        }
        
        this.peerPath = $"{ConfigModule.RuntimeConfig.MetaDataPath}/peers.json";

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
        string? serverIp = (string)parameters["server_ip"] ?? null;
        int port = (int)parameters["port"];
        double version = (double)parameters["version"];
        string? apiPath = (string)parameters["api_path"] ?? null;

        if(serverIp == null || port == 0 || version == 0 || apiPath == null)
        {
            throw new WarningException("parameter mismatch");
        }

        if(isPeerExist(serverIp))
        {
            throw new AuthenticationException("Peer IP is banned from network");
        }
        addToPeerRepository(serverIp, port, version, apiPath);
        dumpPeerRepository();

        executeBroadcast(serverIp, port, version, apiPath);

        JObject response =  new JObject();
        response["result"] = "OK";
        return response;
    }

    // TODO
    private void executeBroadcast(string serverIp, int port, double version, string apiPath)
    {
        for(int i = 0; i < peerRepository.Count; i++)
        {
            PeerModel eachPeer = peerRepository[i];
            if(!eachPeer.trust)
                continue;

            String endpoint = eachPeer.GetEndpoint();
            // TODO : Send rpc request except banneds here...
        } 
    }

    private void addToPeerRepository(string serverIp, int port, double version, string  apiPath)
    {
        PeerModel peerModel = new PeerModel(serverIp, port, version, apiPath);
        peerRepository.Add(peerModel);
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

    private void dumpPeerRepository()
    {
        StorageModule.WriteToJsonFile(peerPath, peerRepository);
    }

    private List<PeerModel> loadPeerRepository()
    {
        List<PeerModel> loadedPeerList = StorageModule.LoadFromJsonFile<List<PeerModel>>(peerPath) ?? new List<PeerModel>();
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