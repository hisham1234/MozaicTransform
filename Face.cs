using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using ImageProcessorCore;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

using Microsoft.Extensions.Logging;

namespace MozaicTransform
{
    public class Face
    {
        public IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
        public async Task<Stream> DetectAndBlurFaces(IFaceClient client, string recognitionModel,Stream myBlob,string connectionString,string name,ILogger log)
        {
            log.LogInformation(" | Mozaic-Transform | Calling Azure cognitive service!");
            var imageStream = new MemoryStream();
            myBlob.CopyTo(imageStream);
            myBlob.Seek(0, SeekOrigin.Begin);
            
            try
            {
              

                 var detectedFaces = await client.Face.DetectWithStreamAsync(myBlob,

                  detectionModel: DetectionModel.Detection03,
                  recognitionModel: recognitionModel);

                log.LogInformation($" | Mozaic-Transform | Cognitive-Service | {detectedFaces.Count} face(s) detected from !");

                var faceImage = new FaceBlur(imageStream);
                faceImage.detectedFaces = detectedFaces;

               
                
                return faceImage.Blur(); 

            }
            catch (Exception ex)
            {
                log.LogError("| Mozaic-Transform | Cognitive-Service | [Error] Occuured while detecting face!");
                log.LogError(ex.Message);
                return new MemoryStream();
            }

        }

        public void UploadToBlobContainer(string connectionString,string name,Stream uploadStream,string blurContainerName,ILogger log)
        {
            try
            {
                // Create a BlobServiceClient object which will be used to create a container client
                var containerClient = new BlobContainerClient(connectionString, blurContainerName);

                // Get a reference to a blob
                BlobClient blobClient = containerClient.GetBlobClient(name);

                
                blobClient.Upload(uploadStream, true);
                
                log.LogInformation(DateTime.Now.ToString("| Mozaic-Transform | Picture :" + name + " blured uploaded successfully!"));
            }
            catch (Exception ex)
            {

                log.LogError( "| Mozaic-Transform |  [Error] Picture :" + name + " blured not uploaded successfully!");
                log.LogError(ex.Message);


            }
            finally
            {
                if (uploadStream != null)
                    uploadStream.Dispose();
            }



        }

    }
}