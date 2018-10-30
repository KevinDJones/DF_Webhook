using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public static class Orchestrators
{
    [FunctionName("O_SendMessage")]
    public static async Task SendMessage(
    [OrchestrationTrigger] DurableOrchestrationContextBase context,
    ILogger log)
    {
        MessageInput input = context.GetInput<MessageInput>();
        var smsKey = await context.CallActivityAsync<string>("A_SaveTwilioMapping", context.InstanceId);
        input.SmsKey = smsKey;

        await context.CallActivityAsync<object>("A_SendTwilioText", input);

        var status = await context.WaitForExternalEvent<string>("TwilioWebhook");
        log.LogWarning("Got status from Twilio! " + status);
    }
}