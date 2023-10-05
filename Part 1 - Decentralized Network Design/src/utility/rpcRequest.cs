using System.Net;
using Newtonsoft.Json.Linq;

class RpcRequest
{
    // Send a json rpc request to single peer
    public static async void Send(string endpoint, string method, JToken payload)
    {
        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        JObject responseData = new JObject
        {
             ["jsonrpc"] = "2.0",
             ["method"] = method,
             ["params"] = payload,
             ["id"] = 1
        };

        request.Content = new StringContent(responseData.ToString(), null, "application/json");
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    // Sends same content to all peers async
    public static async void SendBroadcast(List<string> endpointList, string method, JToken payload)
    {
        String eachResponseData = new JObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method,
            ["params"] = payload,
            ["id"] = 1
        }.ToString();

        HttpClient client = new HttpClient();
        List<HttpRequestMessage> requestList = new List<HttpRequestMessage>();
        List<Task<HttpResponseMessage>> responseList = new List<Task<HttpResponseMessage>>();
        
        endpointList.ForEach(eachEndpoint => {
            HttpRequestMessage eachRequest = new HttpRequestMessage(HttpMethod.Post, eachEndpoint)
            {
                Content = new StringContent(eachResponseData, null, "application/json")
            };

            Task<HttpResponseMessage> eachResponse = client.SendAsync(eachRequest);

            responseList.Add(eachResponse);
        });

        IEnumerable<HttpResponseMessage> sendedResponseList = await Task.WhenAll(responseList);

        foreach(HttpResponseMessage eachSendedResponseMessage in sendedResponseList)
        {
            eachSendedResponseMessage.EnsureSuccessStatusCode();
        }        
    }
}