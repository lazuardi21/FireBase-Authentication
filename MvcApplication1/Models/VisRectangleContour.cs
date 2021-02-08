using NationalInstruments.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcApplication1.Models
{
    public class VisRectangleContour
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public VisRectangleContour()
        {

        }
        public VisRectangleContour(RectangleContour rect)
        {
            this.Top = rect.Top;
            this.Left = rect.Left;
            this.Width = rect.Width;
            this.Height = rect.Height;
        }
    }
}
