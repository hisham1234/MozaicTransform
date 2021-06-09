using ImageProcessorCore;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MozaicTransform
{
    public abstract class ImageHelper
    {
        public Stream ImageStream { get; set; }

        public ImageHelper(Stream Image)
        {
            ImageStream = Image;
        }
        public abstract Stream Blur();
        


        
                

        
    }

    public class FaceBlur : ImageHelper
    {
        public FaceBlur(Stream Image)
            :base(Image)
        {

        }
       // IList<DetectedFace> detectedFaces;
        public IList<DetectedFace> detectedFaces { get; set; }
        public override Stream Blur()
        {
            try
            {
                var image = new Image(ImageStream);


                foreach (var face in detectedFaces)
                {
                    var rec = new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height);

                    image.BoxBlur(20, rec);


                }

                var blurImageStream = new MemoryStream();


                image.Save(blurImageStream);


                blurImageStream.Seek(0, SeekOrigin.Begin);
                return blurImageStream;
            }
            catch (Exception)
            {

                return new MemoryStream();
            }
            
        }
    }


    public class TextBlur:ImageHelper
    {
         
        public IList<Line> Cordinates { get; set; }
        public TextBlur(Stream image):base(image)
        {

        }

        public override Stream Blur()
        {
            try
            {

                var blurTextStream = new MemoryStream();
                var image = new Image<Color, uint>(ImageStream);

                foreach (var line in Cordinates)
                {
                    var i = line.Text.ToString().IndexOf('●');


                    var lineModified = line.Text.Trim();
                    if (i != -1)
                    {
                        lineModified = lineModified.Remove(i, 1).ToString();
                    }
                    var j = lineModified.ToString().IndexOf('°');
                    if (j != -1)
                    {
                        lineModified = lineModified.Remove(j, 1).ToString();
                    }

                    var regFornumberPlateLineOne = new Regex(@"^[!@#$&-¥=,?<>*●.°·]?[一-龯]{1,4}(\s?)[0-9]{1,4}[!@#$&-¥=,?<>*●.°·]?$");


                    var regFornumberPlateLineTwo = new Regex(@"^[(ぁ-んァ-ン]?[ぁ-んァ-ン!@#$&-¥=,?<>*·±]?(\s?)[0-9]{1,3}(-)[0-9|]{1,3}$");
                    // var regFornumberPlateLineBackupOne = new Regex(@"^[0-9]{1,3}$");

                    bool numberPlateLineOne = regFornumberPlateLineOne.IsMatch(lineModified);

                    bool numberPlateLineTwo = regFornumberPlateLineTwo.IsMatch(lineModified);

                    if (numberPlateLineOne || numberPlateLineTwo)
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






                }
                image.Save(blurTextStream);
                blurTextStream.Seek(0, SeekOrigin.Begin);
                return blurTextStream;
            }
            catch (Exception )
            {
                
              //  log.LogInformation(ex.Message);
                return new MemoryStream();
                // throw;
            }
        }
    }
}
