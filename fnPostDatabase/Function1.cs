using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnPostDatabase
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("movie")]
        [CosmosDBOutput("%DatabaseName%", "movies", Connection ="CosmoDBConnection", CreateIfNotExists = true, PartitionKey = "id")]
        public async Task<object> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            MovieRequest movie = null;


            var content =await new StreamReader(req.Body).ReadToEndAsync();
            movie = JsonConvert.DeserializeObject<MovieRequest>(content);


            return JsonConvert.SerializeObject(movie);
        }
    }
}
