using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainNetwork;

class Program
{
    static async Task Main(string[] args)
    {
        Config.Load();

        if(Config.RuntimeConfig == null)
        {
            throw new SystemException("JSON-RPC server cannot started");
        }

        if(Config.RuntimeConfig.IsGenesisNode)
        {
            Console.WriteLine("Server started as Genesis Node");
        }

        
        PeerService peerService = new PeerService();

        RpcServerModule rpcServer = new RpcServerModule(peerService);
        await rpcServer.start(peerService);
    }
    
}
