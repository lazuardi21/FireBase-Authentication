using NationalInstruments.Vision.Analysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MvcApplication1.Models
{
    public class VisGrainSize
    {
        public string Name { get; set; }
        public double ScoreClassification { get; set; }
        public double ScoreIdentification { get; set; }

        //[JsonIgnore]
        //public VisRectangleContour Shape { get; set; }
        public VisGrainSize()
        {

        }
    }

    public class VisGrainSizeCollection
    {
        public List<VisGrainSize> Items { get; set; }
        public string Message { get; set; }
        public VisGrainSizeCollection()
        {
            Items = new List<VisGrainSize>();
            Message = "";
        }
        public VisGrainSizeCollection(Collection<ClassifierReport> reports)
        {
            Items = new List<VisGrainSize>();
            foreach (ClassifierReport r in reports)
            {
                VisGrainSize v = new VisGrainSize { Name = r.BestClassName, ScoreClassification = r.ClassificationScore, ScoreIdentification = r.IdentificationScore };
                Items.Add(v);
            }
            Message = "";
        }
    }
}