using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MtMotorsFunctions
{
    public class ArchiveBlob
    {
        private readonly ILogger<ArchiveBlob> _logger;

        public ArchiveBlob(ILogger<ArchiveBlob> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ArchiveBlob))]
        public async Task  Run(
            [ServiceBusTrigger("MtMotorsTopic", "MtMotorsSub3", Connection = "ServiceBusConn")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions,
            BlobContainerClient blobContainerClient)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
            _logger.LogInformation("C# service bus trigger function processed a request.");

            string messageBody = message.Body.ToString();

            //blob file name is the unique id 
            var blobName = messageBody.Split(',')[0] + ".csv";
            string filePath = await WriteToLocalFile(blobName, messageBody);

            // upload blob
            var storageAccountConnString = Environment.GetEnvironmentVariable("BlobStorageConn");
            var blobServiceClient = new BlobServiceClient(storageAccountConnString);
            blobContainerClient = blobServiceClient.GetBlobContainerClient("mtmotorsblobst");
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(filePath, true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }

        private async Task<string> WriteToLocalFile(string fileName, string content)
        {
            string localPath = "data";
            Directory.CreateDirectory(localPath);
            string localFilePath = Path.Combine(localPath, fileName);
            await File.WriteAllTextAsync(localFilePath, content);
            return localFilePath;
        }
    }
}
