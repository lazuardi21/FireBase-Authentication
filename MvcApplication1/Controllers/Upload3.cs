using MvcApplication1.Models;
using MvcApplication1.Models.GrainLight;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Analysis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace MvcApplication1.Controllers
{
    [Authorize]
    public class Upload3Controller : ApiController
    {
        public string Get()
        {
            return "Welcome To Web API dude...";  
        }
        public HttpResponseMessage Post()
        {
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;

            var form = httpRequest.Form;
            if (form.Count==2)
            {
                string param1 = form.Get(0);
                string param2 = form.Get(1);
            }

            VisCalculation calc = new VisCalculation();
            //VisCalculationSize calcSize = new VisCalculationSize();

            if (httpRequest.Files.Count > 0)
            {

                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    //string myFile = Server.MapPath("images") + "\" + FileUpload1.PostedFile.FileName;
                    string fileName = Path.GetFileName(postedFile.FileName);
                    string foderDate = DateTime.Now.ToString("yyyyMMdd");
                    
                    var filePath = HttpContext.Current.Server.MapPath("~/images/" + foderDate + "/" + fileName); //postedFile.FileName);
                    string fullPath = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    postedFile.SaveAs(filePath);

                    //Image image = Image.FromFile(HttpContext.Current.Server.MapPath("~/images/" + foderDate + "/" + fileName));

                    FileInformation fileinfo = Algorithms.GetFileInformation(filePath);
                    VisionImage image = new VisionImage();
                    image.Type = fileinfo.ImageType;
                    image.ReadFile(filePath);
                    //calculation grain type
                    calc = new VisCalculation(image);
                    //calculation grain size
                    //calcSize = new VisCalculationSize(image);

                    docfiles.Add(filePath);
                }
                //result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                result = Request.CreateResponse(HttpStatusCode.OK, calc.GrainResults);
            }
            else
            {
                //result = Request.CreateResponse(HttpStatusCode.BadRequest);
                VisGrainTypeCollection cols = new VisGrainTypeCollection();
                cols.Message = "BadRequest";
                result = Request.CreateResponse(HttpStatusCode.OK, cols);
            }
            return result;
        } 
    }
}