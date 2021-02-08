using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using NationalInstruments.Vision.Analysis;

namespace MvcApplication1.Models
{
    public class VisGrainData
    {
        public VisGrainType GrainType {get;set;}
        public VisGrainSize GrainSize {get;set;}
        public VisRectangleContour Shape { get; set; }
        public VisGrainData()
        {

        }

        public VisGrainData(VisGrainType grainType, VisGrainSize grainSize)
        {
            GrainType = grainType;
            GrainSize = grainSize;
        }

        public VisGrainData(VisGrainType grainType, VisGrainSize grainSize, VisRectangleContour rect)
        {
            GrainType = grainType;
            GrainSize = grainSize;
            Shape = rect;
        }

    }

    public class VisGrainDataCollection
    {
        public List<VisGrainData> Items { get; set; }
        public string Message { get; set; }
        public VisGrainDataCollection()
        {
            Items = new List<VisGrainData>();
            Message = "";
        }
        public VisGrainDataCollection(Collection<ClassifierReport> reportType, Collection<ClassifierReport> reportSize, List<VisRectangleContour> listRect)
        {
            Items = new List<VisGrainData>();
            int countSize = reportSize.Count;
            int countType = reportType.Count;
            int count = 0;

            if (countSize>countType)
            {
                count = countSize;
            } 
            else
            {
                count = countType;
            }

            for (int i = 0; i < count; i++)
            {
                ClassifierReport rSize = reportSize[i];
                ClassifierReport rType = reportType[i];
                VisGrainSize grainSize = new VisGrainSize { Name = rSize.BestClassName, ScoreClassification = rSize.ClassificationScore, ScoreIdentification = rSize.IdentificationScore };
                VisGrainType grainType = new VisGrainType { Name = rType.BestClassName, ScoreClassification = rType.ClassificationScore, ScoreIdentification = rType.IdentificationScore };
                VisGrainData grainData = new VisGrainData(grainType, grainSize, listRect[i]);
                Items.Add(grainData);
            }

            Message = "";
        }

    }
}