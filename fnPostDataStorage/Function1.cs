using System;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace fnPostDataStorage
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("dataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processando a imagem no Storage");
            
           
                if(!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                {
                    return new BadRequestObjectResult("O cabecalho 'file-type' é obrigatório");
                }

                var fileType = fileTypeHeader.ToString();
                var form =await req.ReadFormAsync();
                var file = form.Files["file"];

                if(file is null || file.Length == 0)
                    return new BadRequestObjectResult("O Arquivo não foi enviado");


                string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string containerName = fileType;


                BlobClient blobClient = new BlobClient(connection, containerName, file.FileName);
                BlobContainerClient containerClient = new BlobContainerClient(connection, containerName);

                await containerClient.CreateIfNotExistsAsync();

                await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

                var blobName = file.FileName;
                var blob = containerClient.GetBlobClient(blobName);

                using(var stream = file.OpenReadStream())
                {
                    await blob.UploadAsync(stream);
                }

                _logger.LogInformation($"arquivo {file.FileName} armazenado com sucesso");


                return new OkObjectResult(new
                {
                    Message = "$\"Arquivo {file.FileName} armazenado com sucesso\"",
                    BlobUri = blob.Uri

                });
               
          
        }
          
    }
}
