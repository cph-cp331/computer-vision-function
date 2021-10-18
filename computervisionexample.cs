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
using System.Net.Http;
using System.Net;
using System.Text;

namespace My.Functions
{
    public static class computervisionexample
    {
        static string subscriptionKey = "f67a72fc989b4c90a061a355d081758b";
        static string endpoint = "https://cp331computervision.cognitiveservices.azure.com/";
        private const string ANALYZE_URL_IMAGE = "https://berlingske.bmcdn.dk/media/cache/resolve/image_x_large/image/131/1316138/23476506-health-coronavirusglobal-groceries.jpg";

        [FunctionName("computervisionexample")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string imageurl = req.Query["imageurl"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            imageurl = imageurl ?? data?.imageurl;

            log.LogInformation("imageurl" + imageurl);
            List<string> result = new List<string>();
            if (imageurl != "" && imageurl != null)
            {
                if (CheckURLValid(imageurl))
                {
                    ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);
                    result = await AnalyzeImageUrl(client, imageurl, log);
                }
                else
                {
                    result.Add("Url is invalid.");
                }
            }
            else
            {
                result.Add("Parameter not found");
            }

            var myObj = new { response= result};
            var jsonToReturn = JsonConvert.SerializeObject(myObj);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
            new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
            return client;
        }

        public static bool CheckURLValid(this string source) => Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult) && uriResult.Scheme == Uri.UriSchemeHttps;

        public static async Task<List<string>> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl, ILogger log)
        {
            List<string> result = new List<string>();
            try
            {
                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

                ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);


                 log.LogInformation("Summary:");
                foreach (var caption in results.Description.Captions)
                {
                    result.Add($"{caption.Text} with confidence {caption.Confidence}");
                     log.LogInformation($"{caption.Text} with confidence {caption.Confidence}");
                }

                // Well-known (or custom, if set) brands.
                log.LogInformation("Brands:");
                foreach (var brand in results.Brands)
                {
                    result.Add($"Logo of {brand.Name} with confidence {brand.Confidence} at location {brand.Rectangle.X}, " +
                      $"{brand.Rectangle.X + brand.Rectangle.W}, {brand.Rectangle.Y}, {brand.Rectangle.Y + brand.Rectangle.H}");

                     log.LogInformation($"Logo of {brand.Name} with confidence {brand.Confidence} at location {brand.Rectangle.X}, " +
                      $"{brand.Rectangle.X + brand.Rectangle.W}, {brand.Rectangle.Y}, {brand.Rectangle.Y + brand.Rectangle.H}");
                }

                // Faces
                log.LogInformation("Faces:");
                foreach (var face in results.Faces)
                {
                    result.Add($"A {face.Gender} of age {face.Age} at location {face.FaceRectangle.Left}, " +
                      $"{face.FaceRectangle.Left}, {face.FaceRectangle.Top + face.FaceRectangle.Width}, " +
                      $"{face.FaceRectangle.Top + face.FaceRectangle.Height}");
                    log.LogInformation($"A {face.Gender} of age {face.Age} at location {face.FaceRectangle.Left}, " +
                      $"{face.FaceRectangle.Left}, {face.FaceRectangle.Top + face.FaceRectangle.Width}, " +
                      $"{face.FaceRectangle.Top + face.FaceRectangle.Height}");
                }
                log.LogInformation("CD Test");
                

            }
            catch (Exception e)
            {
                 log.LogInformation("Exception: " + e.Message);
                result.Add("Could not load image.");
            }
            return result;
        }
    }
}
