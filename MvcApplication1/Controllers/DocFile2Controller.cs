using MvcApplication1.Models;
using MvcApplication1.Models.GrainLight;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Analysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace MvcApplication1.Controllers
{
    
    public class DocFile2Controller : ApiController
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

            VisCalculation calc = new VisCalculation();
            //VisCalculationSize calcSize = new VisCalculationSize();

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["GrainConnection"].ConnectionString;


            if (httpRequest.Files.Count > 0)
            {

                var docfiles = new List<string>();
                string fileName = "";
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    //string myFile = Server.MapPath("images") + "\" + FileUpload1.PostedFile.FileName;
                    fileName = Path.GetFileName(postedFile.FileName);
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
                //save to database
                string user = "";
                double lat = 1000;
                double lng = 1000;
                if (form.Count > 0)
                {

                    var items = form.AllKeys.SelectMany(form.GetValues, (k, v) => new {key = k, value = v});
                    foreach (var item in items)
                    {
                        //Console.WriteLine("{0} {1}", item.key, item.value);
                        if (item.key.Equals("USER_ID"))
                        {
                            user = item.value;
                        }
                        else if (item.key.Equals("LATITUDE"))
                        {
                            Double.TryParse(item.value, out lat);
                        }
                        else if (item.key.Equals("LONGITUDE"))
                        {
                            Double.TryParse(item.value, out lng);
                        }
                    }

                    /*
                    int pos = 0;
                    string param1 = "";
                    string param2 = "";
                    string param3 = "";
                    foreach (var item in items)
                    {
                        //Console.WriteLine("{0} {1}", item.key, item.value);
                        if (pos == 0)
                        {
                            //param1 = item.key;
                            param1 = item.value;
                        }
                        else if (pos == 1)
                        {
                            //param2 = item.key;
                            param2 = item.value;
                        }
                        else if (pos == 2)
                        {
                            //param3 = item.key;
                            param3 = item.value;
                        }
                        pos = pos + 1;
                    } */
                    
                    //string param1 = "form.Get(0)";
                    //string param2 = "form.Get(1)";
                    //string param3 = "form.Get(2)";

                    //if (!user.Equals("") && lat != 1000 && lng !=1000)
                    //{
                        SqlConnection con = new SqlConnection(connectionString);
                        try
                        {
                            con.Open();
                            string json = JsonConvert.SerializeObject(calc.GrainResults);
                            string sql = "INSERT INTO G_RESULT (DATE_STAMP, USER_ID, LATITUDE, LONGITUDE, FILE_NAME, RESULT) VALUES('" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss") + "','" + user + "','" + Convert.ToString(lat) + "','" + Convert.ToString(lng) + "','" + fileName + "','" + json + "');";
                            //string sql = "INSERT INTO G_TEST1 (PARAM1, PARAM2, PARAM3) VALUES('" + param1 + "','" + param2 + "','" + param3 + "');";
                            SqlCommand cmd = new SqlCommand(sql, con);
                            cmd.CommandType = System.Data.CommandType.Text;
                            int ret = cmd.ExecuteNonQuery();
                            //msg = "Success";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            //msg = ex.Message;
                        }
                    //}
                
                }


                result = Request.CreateResponse(HttpStatusCode.OK, calc.GrainResults);
                //result = Request.CreateResponse(HttpStatusCode.OK, user + ", " + Convert.ToString(lat) + ", " + Convert.ToString(lng));
            }
            else
            {
                //result = Request.CreateResponse(HttpStatusCode.BadRequest);
                VisGrainTypeCollection cols = new VisGrainTypeCollection();
                cols.Message = "BadRequest";
                //string msg = "";

                //cols.Message = msg; // connectionString;
                result = Request.CreateResponse(HttpStatusCode.OK, cols);
            }
            return result;
        }
    }
}