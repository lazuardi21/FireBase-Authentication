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

namespace MvcApplication1.Models.GrainLight
{
    public class VisCalculation
    {
        public VisCalculation()
        {
            
        }

        public VisCalculation(VisionImage image)
        {
            //VisionImage visImage = new VisionImage();

            ProcessImage(image);
        }


        private Collection<ClassifierReport> vaClassifierReports;
        public VisGrainTypeCollection GrainResultsType { get; set; }

        //private Collection<ClassifierReport> vaClassifierReportsSize;
        public VisGrainSizeCollection GrainResultsSize { get; set; }
        public VisGrainDataCollection GrainResults { get; set; }
        public List<VisRectangleContour> ListShape { get; set; }

        private ParticleMeasurementsReport vaParticleReport;
        private ParticleMeasurementsReport vaParticleReportCalibrated;

        //public static Collection<ClassifierReport> vaClassifierReports;
        private Collection<ClassifierReport> vaClassifierReports2;
		

        private Collection<ClassifierReport> IVA_Classify(VisionImage image, Roi roi, string classifierFilePath)
        {
            //retval = new VisGrainTypeCollection();
            // Create a binary image that will contain the segmented image.
            using (VisionImage binaryImage = new VisionImage(ImageType.U8, 7))
            {
                ParticleClassifierSession vaClassifier = new ParticleClassifierSession();
                bool fileExists = System.IO.File.Exists(classifierFilePath);
                vaClassifier.ReadFile(classifierFilePath);

                // Segments the image.
                Functions.IVA_Classification_Segmentation(image, binaryImage, roi, vaClassifier.PreprocessingOptions);

                // Get the ROIs of all individual particles.
                Collection<Roi> rois = Functions.IVA_Classification_Extract_Particles(image, binaryImage);

                //  Allocates the classifier reports for all objects in the image.
                Collection<ClassifierReport> classifierReports = new Collection<ClassifierReport>();

                List<VisGrainType> listGrainType = new List<VisGrainType>();
                ListShape = new List<VisRectangleContour>();
                // Classifies the object located in the given ROIs.
                for (int i = 0; i < rois.Count; ++i)
                {
                    //RectangleContour rect = binaryImage=new VisionImage()
                    object obj1 = rois[i][0].Shape;
                    RectangleContour rc = (RectangleContour)rois[i][0].Shape;
                    VisRectangleContour rect = new VisRectangleContour(rc);
                    ClassifierReport report = vaClassifier.Classify(image, rois[i]);
                    VisGrainType grainType = new VisGrainType { Name = report.BestClassName, ScoreClassification = report.ClassificationScore, ScoreIdentification = report.IdentificationScore };
                    listGrainType.Add(grainType);
                    classifierReports.Add(report);

                    ListShape.Add(rect);
                }

                GrainResultsType = new VisGrainTypeCollection();
                GrainResultsType.Items = listGrainType;

                return classifierReports;
            }
        }

        private Collection<ClassifierReport> IVA_ClassifySize(VisionImage image, Roi roi, string classifierFilePath)
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

                GrainResultsSize = new VisGrainSizeCollection();
                GrainResultsSize.Items = listGrain;

                return classifierReports;
            }
        }

        private void IVA_Particle(VisionImage image,
                                        Connectivity connectivity,
                                        Collection<MeasurementType> pPixelMeasurements,
                                        Collection<MeasurementType> pCalibratedMeasurements,
                                        IVA_Data ivaData,
                                        int stepIndex,
                                        out ParticleMeasurementsReport partReport,
                                        out ParticleMeasurementsReport partReportCal)
        {

            // Computes the requested pixel measurements.
            if (pPixelMeasurements.Count != 0)
            {
                partReport = Algorithms.ParticleMeasurements(image, pPixelMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Pixel);
            }
            else
            {
                partReport = new ParticleMeasurementsReport();
            }

            // Computes the requested calibrated measurements.
            if (pCalibratedMeasurements.Count != 0)
            {
                partReportCal = Algorithms.ParticleMeasurements(image, pCalibratedMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Calibrated);
            }
            else
            {
                partReportCal = new ParticleMeasurementsReport();
            }

            // Computes the center of mass of each particle to log as results.
            ParticleMeasurementsReport centerOfMass;
            Collection<MeasurementType> centerOfMassMeasurements = new Collection<MeasurementType>();
            centerOfMassMeasurements.Add(MeasurementType.CenterOfMassX);
            centerOfMassMeasurements.Add(MeasurementType.CenterOfMassY);

            if ((image.InfoTypes & InfoTypes.Calibration) != 0)
            {
                centerOfMass = Algorithms.ParticleMeasurements(image, centerOfMassMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Both);
            }
            else
            {
                centerOfMass = Algorithms.ParticleMeasurements(image, centerOfMassMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Pixel);
            }

            // Delete all the results of this step (from a previous iteration)
            Functions.IVA_DisposeStepResults(ivaData, stepIndex);

            ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Object #", centerOfMass.PixelMeasurements.GetLength(0)));

            if (centerOfMass.PixelMeasurements.GetLength(0) > 0)
            {
                for (int i = 0; i < centerOfMass.PixelMeasurements.GetLength(0); ++i)
                {
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.X Position (Pix.)", i + 1), centerOfMass.PixelMeasurements[i, 0]));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.Y Position (Pix.)", i + 1), centerOfMass.PixelMeasurements[i, 1]));

                    // If the image is calibrated, also store the real world coordinates.
                    if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                    {
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.X Position (Calibrated)", i + 1), centerOfMass.CalibratedMeasurements[i, 0]));
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.Y Position (Calibrated)", i + 1), centerOfMass.CalibratedMeasurements[i, 1]));
                    }
                }
            }
        }

        public PaletteType ProcessImage_OLD(VisionImage image)
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
            //20200715
            //RectangleContour vaRect = new RectangleContour(49, 33, 667, 163);
            RectangleContour vaRect = new RectangleContour(0, 0, image.Width, image.Height);
            roi.Add(vaRect);
            // Classifies all the objects located in the given ROI.
            string vaClassifierFilePath = "C:\\DATA\\#hasilscan\\Particle Classifier.clf";
            vaClassifierReports = IVA_Classify(image, roi, vaClassifierFilePath);
            VisGrainTypeCollection colType = new VisGrainTypeCollection(vaClassifierReports);

            string vaClassifierFilePathSize = "C:\\DATA\\#hasilscan2\\Ukuran Classifier.clf";
            vaClassifierReports2 = IVA_ClassifySize(image, roi, vaClassifierFilePathSize);
            VisGrainSizeCollection colSize = new VisGrainSizeCollection(vaClassifierReports2);

            GrainResults = new VisGrainDataCollection(vaClassifierReports, vaClassifierReports2, ListShape);

            roi.Dispose();

            // Dispose the IVA_Data structure.
            ivaData.Dispose();

            // Return the palette type of the final image.
            return PaletteType.Binary;

        }

        public PaletteType ProcessImage(VisionImage image)
        {
            //vaParticleReportCalibrated = new ParticleMeasurementsReport();
            // Initialize the IVA_Data structure to pass results and coordinate systems.
			IVA_Data ivaData = new IVA_Data(15, 0);
			
			// Image Buffer: Push
			Functions.IVA_PushBuffer(ivaData, image, 0);
			
			// Operators: NOR Image
			Algorithms.Nor(image, Functions.IVA_GetBuffer(ivaData, 0), image);
			
			// Extract Color Plane
			using (VisionImage plane = new VisionImage(ImageType.U8, 7))
			{
				// Extract the red color plane and copy it to the main image.
				Algorithms.ExtractColorPlanes(image, ColorMode.Rgb, plane, null, null);
				Algorithms.Copy(plane, image);
			}
			
			// Thresholds an image into 2 classes by using local thresholding.
			LocalThresholdOptions vaLocalThresholdOptions = new LocalThresholdOptions();
			vaLocalThresholdOptions.DeviationWeight = 1;
			vaLocalThresholdOptions.Method = LocalThresholdMethod.BackgroundCorrection;
			vaLocalThresholdOptions.ParticleType = ParticleType.Dark;
			vaLocalThresholdOptions.ReplaceValue = 1;
			vaLocalThresholdOptions.WindowHeight = 35;
			vaLocalThresholdOptions.WindowWidth = 35;
			Algorithms.LocalThreshold(image, image, vaLocalThresholdOptions);
			
			// Basic Morphology - Applies morphological transformations to binary images.
			int[] vaCoefficients = {1, 1, 1, 1, 1, 1, 1, 1, 1};
			StructuringElement vaStructElem = new StructuringElement(3, 3, vaCoefficients);
			vaStructElem.Shape = StructuringElementShape.Square;
			// Applies morphological transformations
			Algorithms.Morphology(image, image, MorphologyMethod.GradientOut, vaStructElem);
			
			// Advanced Morphology: Fill Holes
			Algorithms.FillHoles(image, image, Connectivity.Connectivity8);
			
			// Advanced Morphology: Remove Objects
			int[] vaCoefficients2 = {1, 1, 1, 1, 1, 1, 1, 1, 1};
			StructuringElement vaStructElem2 = new StructuringElement(3, 3, vaCoefficients2);
			vaStructElem.Shape = StructuringElementShape.Hexagon;
			// Filters particles based on their size.
			Algorithms.RemoveParticle(image, image, 6, SizeToKeep.KeepLarge, Connectivity.Connectivity8, vaStructElem2);
			
			// Advanced Morphology: Remove Border Objects - Eliminates particles touching the border of the image.
			Algorithms.RejectBorder(image, image, Connectivity.Connectivity8);
			
			// Lookup Table: Equalize
			// Calculates the histogram of the image and redistributes pixel values
			// accross the desired range to maintain the same pixel value distribution.
			Range equalizeRange = new Range(0, 255);
			if (image.Type != ImageType.U8)
			{
				equalizeRange.Maximum = 0;
			}
			Algorithms.Equalize(image, image, null, equalizeRange, null);
			
			// Image Buffer: Push
			Functions.IVA_PushBuffer(ivaData, image, 1);
			
			// Particle Analysis - Computes the number of particles detected in a binary image and
			// returns the requested measurements about the particles.
			Collection<MeasurementType> vaPixelMeasurements = new Collection<MeasurementType>(new MeasurementType[]{MeasurementType.BoundingRectLeft, MeasurementType.BoundingRectTop, MeasurementType.BoundingRectRight, MeasurementType.BoundingRectBottom, MeasurementType.MaxFeretDiameter});
			Collection<MeasurementType> vaCalibratedMeasurements = new Collection<MeasurementType>(new MeasurementType[]{});
            //IVA_Particle(image, Connectivity.Connectivity4, null, null, null, 10, out vaParticleReport, out vaParticleReportCalibrated);
            IVA_Particle(image, Connectivity.Connectivity4, vaPixelMeasurements, vaCalibratedMeasurements, ivaData, 10, out vaParticleReport, out vaParticleReportCalibrated);
			
			// Image Buffer: Pop
			Algorithms.Copy(Functions.IVA_GetBuffer(ivaData, 1), image);
			
			// Creates a new, empty region of interest.
			Roi roi = new Roi();
			// Creates a new RectangleContour using the given values.
            RectangleContour vaRect = new RectangleContour(0, 0, image.Width, image.Height);
            roi.Add(vaRect);
            // Classifies all the objects located in the given ROI.
			string vaClassifierFilePath = "C:\\DATA\\#hasilscan3\\Particle Classifier.clf";
			vaClassifierReports = IVA_Classify(image, roi, vaClassifierFilePath);
            VisGrainTypeCollection colType = new VisGrainTypeCollection(vaClassifierReports);
			
			roi.Dispose();
            
            // Image Buffer: Pop
			Algorithms.Copy(Functions.IVA_GetBuffer(ivaData, 1), image);
			
			// Creates a new, empty region of interest.
			Roi roi2 = new Roi();
			// Creates a new RectangleContour using the given values.
            RectangleContour vaRect2 = new RectangleContour(0, 0, image.Width, image.Height);
            roi2.Add(vaRect2);
            // Classifies all the objects located in the given ROI.
			string vaClassifierFilePath2 = "C:\\DATA\\#hasilscan3\\Ukuran Classifier.clf";
			vaClassifierReports2 = IVA_Classify(image, roi2, vaClassifierFilePath2);
            VisGrainSizeCollection colSize = new VisGrainSizeCollection(vaClassifierReports2);

            GrainResults = new VisGrainDataCollection(vaClassifierReports, vaClassifierReports2, ListShape);
			
			roi2.Dispose();
            
            // Dispose the IVA_Data structure.
			ivaData.Dispose();
			
			// Return the palette type of the final image.
			return PaletteType.Binary;

        }
    }
}