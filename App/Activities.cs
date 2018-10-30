using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public static class Activities
{
    [FunctionName("A_SaveTwilioMapping")]
    public static async Task<string> Run(
        [ActivityTrigger] string input,
        [Table(MessageMapping.TableName)] IAsyncCollector<MessageMapping> table)
    {
        var mapping = new MessageMapping()
        {
            PartitionKey = "Sms",
            RowKey = Guid.NewGuid().ToString("N"),
            OrchestrationId = input
        };

        await table.AddAsync(mapping);
        return mapping.RowKey;
    }
    
    [FunctionName("A_SendTwilioText")]
    public static async Task SendMessage(
        [ActivityTrigger] MessageInput input,
        [TwilioSms] IAsyncCollector<CreateMessageOptions> message
        )
    {
        var smsText = new CreateMessageOptions(new PhoneNumber(input.Phone))
        {
            From = new PhoneNumber(Environment.GetEnvironmentVariable("TwilioNumber")),
            Body = input.Message,
            StatusCallback = new Uri(Environment.GetEnvironmentVariable("Host") + "/api/TwilioHandler/" + input.SmsKey)
        };

        await message.AddAsync(smsText);
    }
}