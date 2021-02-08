using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NationalInstruments.Vision.WindowsForms;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Analysis;
using System.Collections.ObjectModel;
using System.Drawing;
//using Vision_Assistant.Utilities;

namespace MvcApplication1.Models.GrainSimple
{
    public class VisCalculationSize
    {
        public VisCalculationSize()
        {

        }

        public VisCalculationSize(VisionImage image)
        {
            //VisionImage visImage = new VisionImage();

            ProcessImage(image);
        }


        private static Collection<ClassifierReport> vaClassifierReports;
        public VisGrainSizeCollection GrainResults { get; set; }


        private Collection<ClassifierReport> IVA_Classify(VisionImage image, Roi roi, string classifierFilePath)
        {

            // Create a binary image that will contain the segmented image.
            using (VisionImage binaryImage = new VisionImage(ImageType.U8, 7))
            {
                ParticleClassifierSession vaClassifier = new ParticleClassifierSession(classifierFilePath);

                // Segments the image.
                Functions.IVA_Classification_Segmentation(image, binaryImage, roi, vaClassifier.PreprocessingOptions);

                // Get the ROIs of all individual particles.
                Collection<Roi> rois = Functions.IVA_Classification_Extract_Particles(image, binaryImage);

                //  Allocates the classifier reports for all objects in the image.
                Collection<ClassifierReport> classifierReports = new Collection<ClassifierReport>();

                List<VisGrainSize> listGrain = new List<VisGrainSize>();
                // Classifies the object located in the given ROIs.
                for (int i = 0; i < rois.Count; ++i)
                {
                    //RectangleContour rect = binaryImage=new VisionImage()
                    object obj1 = rois[i][0].Shape;
                    RectangleContour rc = (RectangleContour)rois[i][0].Shape;
                    VisRectangleContour rect = new VisRectangleContour(rc);
                    ClassifierReport report = vaClassifier.Classify(image, rois[i]);
                    VisGrainSize grain = new VisGrainSize { Name = report.BestClassName, ScoreClassification = report.ClassificationScore, ScoreIdentification = report.IdentificationScore };
                    listGrain.Add(grain);
                    classifierReports.Add(report);
                }

                GrainResults = new VisGrainSizeCollection();
                GrainResults.Items = listGrain;

                return classifierReports;
            }
        }


        public PaletteType ProcessImage(VisionImage image)
        {
            // Initialize the IVA_Data structure to pass results and coordinate systems.
            IVA_Data ivaData = new IVA_Data(7, 0);

            // Extract Color Plane
            using (VisionImage plane = new VisionImage(ImageType.U8, 7))
            {
                // Extract the red color plane and copy it to the main image.
                Algorithms.ExtractColorPlanes(image, ColorMode.Rgb, plane, null, null);
                Algorithms.Copy(plane, image);
            }

            // Filters: Convolution - Applies a linear filter to an image by convolving the image with a filtering kernel.
            double[] vaCoefficients = { 1, 2, 4, 2, 1, 2, 4, 8, 4, 2, 4, 8, 16, 8, 4, 2, 4, 8, 4, 2, 1, 2, 4, 2, 1 };
            Algorithms.Convolute(image, image, new Kernel(5, 5, vaCoefficients));

            // Filters: Convolution - Applies a linear filter to an image by convolving the image with a filtering kernel.
            double[] vaCoefficients2 = { -1, -1, -1, -1, 10, -1, -1, -1, -1 };
            Algorithms.Convolute(image, image, new Kernel(3, 3, vaCoefficients2));

            // Automatic Threshold
            Algorithms.AutoThreshold(image, image, 2, ThresholdMethod.Clustering);

            // Basic Morphology - Applies morphological transformations to binary images.
            int[] vaCoefficients3 = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            StructuringElement vaStructElem = new StructuringElement(7, 7, vaCoefficients3);
            vaStructElem.Shape = StructuringElementShape.Hexagon;
            // Applies morphological transformations
            Algorithms.Morphology(image, image, MorphologyMethod.Open, vaStructElem);

            // Lookup Table: Equalize
            // Calculates the histogram of the image and redistributes pixel values
            // accross the desired range to maintain the same pixel value distribution.
            Range equalizeRange = new Range(0, 255);
            if (image.Type != ImageType.U8)
            {
                equalizeRange.Maximum = 0;
            }
            Algorithms.Equalize(image, image, null, equalizeRange, null);

            // Creates a new, empty region of interest.
            Roi roi = new Roi();
            // Creates a new RectangleContour using the given values.
            RectangleContour vaRect = new RectangleContour(0, 0, image.Width, image.Height);
            roi.Add(vaRect);
            // Classifies all the objects located in the given ROI.
            string vaClassifierFilePath = "C:\\DATA\\#hasilscan2\\Ukuran Classifier.clf";
            vaClassifierReports = IVA_Classify(image, roi, vaClassifierFilePath);

            roi.Dispose();

            // Dispose the IVA_Data structure.
            ivaData.Dispose();

            // Return the palette type of the final image.
            return PaletteType.Binary;

        }
    }
}