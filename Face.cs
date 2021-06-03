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
            log.LogInformation("========DETECT FACES========");
            var imageStream = new MemoryStream();
            myBlob.CopyTo(imageStream);
            myBlob.Seek(0, SeekOrigin.Begin);
            try
            {
              

                var detectedFaces = await client.Face.DetectWithStreamAsync(myBlob,

                  detectionModel: DetectionModel.Detection03,
                  recognitionModel: recognitionModel);
                     

                var image = new Image(imageStream);
               

                foreach (var face in detectedFaces)
                {
                    var rec = new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height);
                
                    image.BoxBlur(20, rec);


                }
                
                var blurImageStream = new MemoryStream();
               
                
                image.Save(blurImageStream);
                
                
                blurImageStream.Seek(0, SeekOrigin.Begin);
                log.LogInformation($"{detectedFaces.Count} face(s) detected from image.");
                return blurImageStream;

            }
            catch (Exception ex)
            {
                log.LogInformation("Error Occuured while detecting face");
                log.LogInformation(ex.Message);
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
                
                log.LogInformation("Picture :" + name + " blured uploaded successfully");
            }
            catch (Exception ex)
            {

                log.LogInformation("Picture :" + name + " blured not uploaded successfully");
                log.LogInformation(ex.Message);


            }
            finally
            {
                if (uploadStream != null)
                    uploadStream.Dispose();
            }



        }

    }
}