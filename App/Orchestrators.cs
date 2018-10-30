using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public static class Orchestrators
{
    [FunctionName("O_SendMessage")]
    public static async Task Run(
        [OrchestrationTrigger] DurableOrchestrationContextBase context,
        ILogger log)
    {
        var input = context.GetInput<MessageInput>();

        var smsKey = await context.CallActivityAsync<string>("A_SaveTwilioMapping", context.InstanceId);
        input.SmsKey = smsKey;

        await context.CallActivityAsync<string>("A_SendTwilioText", input);

        var status = await context.WaitForExternalEvent<string>("TwilioCallback");

        log.LogWarning("Got a status from Twilio! " + status);
    }
}