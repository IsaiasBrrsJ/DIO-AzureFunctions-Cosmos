using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.IdentityModel.Protocols;
using System.Linq;

namespace fnGetMovieDetail
{
    public  class Function1
    {
        private readonly IConfiguration configuration;
        public Function1(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [FunctionName("detail")]
        public  async Task<HttpResponse> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);
            var results =await GetCosmos(req.Query["id"]);

            await responseMessage.WriteAsJsonsAsync(results.FirstOrDefault());

            return responseMessage;

        }

        private  async Task<IEnumerable<MovieResult>> GetCosmos(string id)
        {

            var connectionString = configuration["Values:CosmosConnection"];

            var cosmosClient = new CosmosClient(connectionString);

            var container = cosmosClient.GetContainer("DioFlixDB", "movies");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = '{id}'");

            var result = container.GetItemQueryIterator<MovieResult>(query);
            var results = new List<MovieResult>(); 

            var iterator = await result.ReadNextAsync();

            if (result.HasMoreResults)
                results.AddRange(await result.ReadNextAsync());


            return results;
        }
    }
}
