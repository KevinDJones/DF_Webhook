using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

public static class Starters
{
    [FunctionName("Starter")]
    public static async Task<HttpResponseMessage> Starter(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestMessage req,
        [OrchestrationClient] DurableOrchestrationClient starter,
        ILogger log)
    {
        var input = await req.Content.ReadAsAsync<MessageInput>();
        var orchestratorId = await starter.StartNewAsync("O_SendMessage", input);
        return starter.CreateCheckStatusResponse(req, orchestratorId);
    }

    [FunctionName("TwilioHandler")]
    public static async Task<HttpResponseMessage> TwilioHancler(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TwilioHandler/{id}")]
        HttpRequestMessage req,
        [OrchestrationClient] DurableOrchestrationClient client,
        [Table(MessageMapping.TableName, "Sms", "{id}")] MessageMapping mapping,
        ILogger log)
    {
        var content = await req.Content.ReadAsStringAsync();
        var formBody = ParseForm(content);
        var status = formBody["SmsStatus"];

        if (status == "delivered" || status == "failed")
        {
            await client.RaiseEventAsync(mapping.OrchestrationId, "TwilioWebhook", status);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Content = new StringContent("OK", Encoding.UTF8, "text/xml");
        return response;
    }

    public static Dictionary<string, string> ParseForm(string formBody)
    {
        var dictionary = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(formBody))
        {
            return dictionary;
        }

        var bodyParts = formBody.Split('&');

        foreach (var item in bodyParts)
        {
            var keyvalue = item.Split('=');
            dictionary.Add(keyvalue[0], keyvalue[1]);
        }

        return dictionary;
    }
}