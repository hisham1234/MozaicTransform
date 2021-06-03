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

                //using FileStream stream = File.OpenRead(@"D:\images.jpg");
                using var streamImage = new MemoryStream();
                blolbStream.CopyTo(streamImage);
                blolbStream.Seek(0, SeekOrigin.Begin);
                streamImage.Seek(0, SeekOrigin.Begin);
                // Read text from stream
                var textHeaders = await client.ReadInStreamAsync(blolbStream);
                // After the request, get the operation location (operation ID)
                string operationLocation = textHeaders.OperationLocation;
                Thread.Sleep(2000);

                // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
                // We only need the ID and not the full URL
                const int numberOfCharsInOperationId = 36;
                string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

                // Extract the text
                ReadOperationResult results;
                
                do
                {
                    results = await client.GetReadResultAsync(Guid.Parse(operationId));
                }
                while ((results.Status == OperationStatusCodes.Running ||
                    results.Status == OperationStatusCodes.NotStarted));
                var cordinates = results.AnalyzeResult.ReadResults[0].Lines;
                // stream.Seek(0, SeekOrigin.Begin);
                log.LogInformation(results.AnalyzeResult.ReadResults[0].Lines.Count+" lines found") ;
                return ApplyBlur(streamImage, cordinates);
            }
            catch (Exception ex)
            {
                return new MemoryStream();
                //  throw;
            }


        }

        public MemoryStream ApplyBlur(Stream stream, IList<Line> cordinates)
        {
            try
            {
                
                var blurTextStream = new MemoryStream();
                var image = new Image<Color, uint>(stream);

                foreach (var line in cordinates)
                {
                   

                    foreach (var word in line.Words)
                    {
                        var width = word.BoundingBox[2] - word.BoundingBox[0];
                        var height = word.BoundingBox[7] - word.BoundingBox[1];
                        var x = word.BoundingBox[0];//-100;
                        var y = word.BoundingBox[3];//-100;


                        var rec = new Rectangle(Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(width), Convert.ToInt32(height));


                        image.BoxBlur(40, rec);
                    }




                }
                image.Save(blurTextStream);
                blurTextStream.Seek(0, SeekOrigin.Begin);
                return blurTextStream;
            }
            catch (Exception ex)
            {
                return new MemoryStream();
                // throw;
            }

        }
    }
}
