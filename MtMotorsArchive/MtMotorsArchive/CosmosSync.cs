using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MtMotorsFunctions
{
    public class CosmosSync
    {
        private readonly ILogger<CosmosSync> _logger;

        public CosmosSync(ILogger<CosmosSync> logger)
        {
            _logger = logger;
        }

        [Function(nameof(CosmosSync))]
        public async Task<CosmosResponse> Run(
            [ServiceBusTrigger("MtMotorsTopic", "MtMotorsSub2", Connection = "ServiceBusConn")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
            _logger.LogInformation("C# service bus trigger function processed a request.");

            var queueMessage = message.Body.ToString();

            string[] messageParams = queueMessage.Split(',');

            await messageActions.CompleteMessageAsync(message);

            return new CosmosResponse()
            {
                Document = new MyDocument
                {
                    id = messageParams[0],
                    dateCreated = messageParams[1],
                    carDetails = string.Join(",", queueMessage.Split(',').Take(new Range(2, messageParams.Count() - 2)))
                }
            };
        }
    }
}
