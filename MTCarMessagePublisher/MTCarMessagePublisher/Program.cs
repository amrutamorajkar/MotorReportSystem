using Azure.Messaging.ServiceBus;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;


namespace MTCarMessagePublisher
{
    internal class Program
    {
        static string filePath = "https://raw.githubusercontent.com/vincentarelbundock/Rdatasets/master/csv/datasets/mtcars.csv";

        static void Main(string[] args)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "text/csv");
                var result = client.GetAsync($"{filePath}").GetAwaiter().GetResult();
                result.EnsureSuccessStatusCode();

                // Azure service bus client   
                ServiceBusClient sbClient;

                // the sender used to publish messages to the azure service bus topic
                ServiceBusSender sender;

                // The Service Bus client types are safe to cache and use as a singleton for the lifetime
                // of the application, which is best practice when messages are being published or read
                // regularly.

                string connectionString = "Endpoint=sb://mtmotorssub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=crTDHxHLDysrKDz/IwZ7ilj9aIKpb2Foe+ASbLaCySM=";
                string topicName = "MtMotorsTopic";

                sbClient = new ServiceBusClient(connectionString);

                sender = sbClient.CreateSender(topicName);

                try
                {
                    using (var s = result.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                    {
                        using (var sr = new StreamReader(s))
                        {
                            while(!sr.EndOfStream)
                            {
                                var current = sr.ReadLine();

                                var createdDate = DateTime.Now.AddSeconds(-DateTime.Now.Second);
                                // id = {car name}_{Current Time upto Minute Ticks}
                                var id = string.Join("", current.Split(',')[0].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)) + "_"  + createdDate.Ticks;
                                var message = $"{id},{createdDate},{current}";
                                var serviceBusMessage = new ServiceBusMessage(message);

                                Thread.Sleep(1000);

                                try
                                {
                                    sender.SendMessageAsync(serviceBusMessage).Wait();
                                    Console.WriteLine($"Message : {message} has been published to the topic");
                                    Console.WriteLine();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Message : {message} could not be published to topic due to {ex.ToString()}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured {ex.ToString()}");
                }
                finally
                {
                    sender.DisposeAsync().GetAwaiter().GetResult();
                    sbClient.DisposeAsync().GetAwaiter().GetResult();
                }

            }
        }
    }
}
