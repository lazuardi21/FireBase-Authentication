using NationalInstruments.Vision.Analysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MvcApplication1.Models
{
    public class VisGrainType
    {
        public string Name { get; set; }
        public double ScoreClassification { get; set; }
        public double ScoreIdentification { get; set; }
        
        //public VisRectangleContour Shape { get; set; }

        public VisGrainType()
        {

        }
    }

    public class VisGrainTypeCollection
    {
        public List<VisGrainType> Items { get; set; }
        public string Message { get; set; }
        public VisGrainTypeCollection()
        {
            Items = new List<VisGrainType>();
            Message = "";
        }
        public VisGrainTypeCollection(Collection<ClassifierReport> reports)
        {
            Items = new List<VisGrainType>();
            foreach (ClassifierReport r in reports)
            {
                VisGrainType v = new VisGrainType { Name = r.BestClassName, ScoreClassification = r.ClassificationScore, ScoreIdentification = r.IdentificationScore };
                Items.Add(v);
            }
            Message = "";
        }
    }

}
