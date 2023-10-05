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
        ConfigModule.Load();

        PeerService peerService = new PeerService();
        await RpcModule.startRpcServer(peerService);
    }
    
}
