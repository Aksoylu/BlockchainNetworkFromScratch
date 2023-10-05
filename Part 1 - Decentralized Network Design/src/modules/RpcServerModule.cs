using System.Linq.Expressions;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BlockchainNetwork;

class RpcServerModule
{
    public RuntimeConfig runtimeConfig {get; private set;}
    public PeerService peerservice {get; private set;}

    public RpcServerModule(PeerService peerservice)
    {
        this.runtimeConfig = Config.RuntimeConfig;
        this.peerservice = peerservice;
    }

    public async Task start(PeerService peerservice)
    {
        string serverUrl = $"http://{runtimeConfig.ServerIp}:{runtimeConfig.Port}/{runtimeConfig.NodeApiPath}/";
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(serverUrl);
        listener.Start();

        Console.WriteLine($"JSON-RPC server active on {serverUrl}");

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            try
            {
                this.handleRequest(context);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception Occured: {e.ToString()}");
            }
        }
    }

    private async void handleRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        IPEndPoint remoteEndPoint = context.Request.RemoteEndPoint as IPEndPoint;
        string ipAddress = remoteEndPoint.Address.ToString();

        if(request.HttpMethod != "POST")
        {
            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            response.Close();
            return;
        }
        
        try
        {
            using (StreamReader reader = new StreamReader(request.InputStream))
            {
                string requestBody = await reader.ReadToEndAsync();

                // Handle the JSON-RPC request here
                string jsonResponse = HandleJsonRpcRequest(ipAddress, requestBody);

                byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = responseBytes.Length;

                using (Stream output = response.OutputStream)
                {
                    await output.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling request: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        response.Close();
    }

    private string HandleJsonRpcRequest(string ipAddress, string requestBody)
    {
        JObject requestObject = JObject.Parse(requestBody);
        string methodName = requestObject["method"].ToString();
        JToken parameters = requestObject["params"];
        parameters["server_ip"] = ipAddress;

        JToken? result = peerservice?.executePeerService(methodName, parameters);

        // Build and return the JSON-RPC response
        JObject responseObject = new JObject
        {
             ["jsonrpc"] = "2.0",
             ["result"] = result,
             ["id"] = requestObject["id"]
        };

        return responseObject.ToString();
    }

}