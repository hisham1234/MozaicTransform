using ImageProcessorCore;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace MozaicTransform
{
    public class Text
    {
        
        public ComputerVisionClient Authenticate(string key,string endpoint)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        public async Task<Stream> DetectAndBlurText(ComputerVisionClient client,Stream blolbStream, ILogger log)
        {
            try
            {
                using var streamImage = new MemoryStream();
                blolbStream.CopyTo(streamImage);
                blolbStream.Seek(0, SeekOrigin.Begin);
                streamImage.Seek(0, SeekOrigin.Begin);
                // Read text from stream
                var textHeaders = await client.ReadInStreamAsync(blolbStream);
                // After the request, get the operation location (operation ID)
                string operationLocation = textHeaders.OperationLocation;
                Thread.Sleep(2000); // From the documentation, this is necessary

                // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
                // We only need the ID and not the full URL
                const int numberOfCharsInOperationId = 36;
                string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

                // Extract the text
                ReadOperationResult results;
                log.LogInformation(" | Mozaic-Transform | Extracting the text with Cognitive-Service!");
                do
                {
                    results = await client.GetReadResultAsync(Guid.Parse(operationId));
                }
                while ((results.Status == OperationStatusCodes.Running ||
                    results.Status == OperationStatusCodes.NotStarted));
                var cordinates = results.AnalyzeResult.ReadResults[0].Lines;
               
                log.LogInformation(results.AnalyzeResult.ReadResults[0].Lines.Count+" lines found") ;
                log.LogInformation(" | Mozaic-Transform | Cognitive-Service | " + results.AnalyzeResult.ReadResults[0].Lines.Count + " lines found!");
                var txt = new TextBlur(streamImage);
                txt.Cordinates = cordinates;
                return txt.Blur();
            }
            catch (Exception ex)
            {
                log.LogError("| Mozaic-Transform | [Error] when detecting and blurring the text!");
                log.LogError(ex.Message);
                return new MemoryStream();
                
            }


        }

        
    }
}
