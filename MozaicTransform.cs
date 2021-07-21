using System;
using System.Configuration;
using System.IO;
using Azure.Storage.Blobs;
using ImageProcessorCore;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
namespace MozaicTransform
{
    public static class MozaicTransform
    {

        
        [FunctionName("MozaicTransform")]
        public static void Run([BlobTrigger("%CRISIS_CONTAINER%/{name}", Connection  = "STORAGE_CONNECTION_APP_SETTING")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + " |" + "Mozaic Transform Called");

            var face = new Face();
            var text = new Text();
          
            var ENDPOINT_FACE = GetEnvironmentVariable("ENDPOINT_FACE");
            var SUBSCRIPTION_KEY_FACE = GetEnvironmentVariable("SUBSCRIPTION_KEY_FACE");

            var ENDPOINT_TEXT = GetEnvironmentVariable("ENDPOINT_TEXT");
            var SUBSCRIPTION_KEY_TEXT = GetEnvironmentVariable("SUBSCRIPTION_KEY_TEXT");

            var connectionString = GetEnvironmentVariable("STORAGE_CONNECTION_APP_SETTING");
            var blurContainer= GetEnvironmentVariable("BLUR_CONTAINER");

            IFaceClient faceClient = face.Authenticate(ENDPOINT_FACE, SUBSCRIPTION_KEY_FACE);
            ComputerVisionClient textClient = text.Authenticate(SUBSCRIPTION_KEY_TEXT,ENDPOINT_TEXT);


            
            var blurfFaceStream= face.DetectAndBlurFaces(faceClient, RecognitionModel.Recognition04,myBlob,connectionString,name,log);
            if (blurfFaceStream.Result != null)
            {
                var blurTextStream = text.DetectAndBlurText(textClient, blurfFaceStream.Result,log);
                if(blurTextStream!=null)
                {
                    face.UploadToBlobContainer(connectionString, name, blurTextStream.Result, blurContainer, log);
                }
                else
                {
                    log.LogInformation("Error while Bluring text");
                }
                
            }
            else
            {
                log.LogInformation("Detecting text did not processed");
            }
                
            
            
             
             
        }

        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

      

    }
}
