using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MtMotorsFunctions
{
    public class CosmosResponse
    {
        [CosmosDBOutput("mtcars", "mtcars",
            Connection = "CosmosDbConn", CreateIfNotExists = true)]
        public MyDocument Document { get; set; }
    }

    public class MyDocument
    {
        public string id { get; set; }
        public string dateCreated { get; set; }
        public string carDetails { get; set; }
    }
}
