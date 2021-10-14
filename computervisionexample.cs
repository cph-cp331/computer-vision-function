using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;

namespace My.Functions
{
    public static class computervisionexample
    {
        static string subscriptionKey = "f67a72fc989b4c90a061a355d081758b";
        static string endpoint = "https://cp331computervision.cognitiveservices.azure.com/";
        private const string ANALYZE_URL_IMAGE = "https://berlingske.bmcdn.dk/media/cache/resolve/image_x_large/image/131/1316138/23476506-health-coronavirusglobal-groceries.jpg";

        [FunctionName("computervisionexample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string imageurl = req.Query["imageurl"];

            string result = "";
            if (imageurl != "" && imageurl != null)
            {
                ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);
                result = await AnalyzeImageUrl(client, imageurl);
            }
            else
            {
                result = "invalid image";
            }

            return new OkObjectResult(result);
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
            new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
            return client;
        }

        public static async Task<string> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("ANALYZE IMAGE - URL");
            Console.WriteLine();


            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);

            string result = "";
            Console.WriteLine("Summary:");
            foreach (var caption in results.Description.Captions)
            {
                result += $"{Environment.NewLine}{caption.Text} with confidence {caption.Confidence}";
                Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
            }

            return result;
        }
    }
}
