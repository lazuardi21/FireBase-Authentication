using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Models
{
    public class RegViewModel
    {
        public string id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string username { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }

        [Display(Name = "Address")]
        public string address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

        [DataType(DataType.Text)]

        [Display(Name = "Phone")]
        public string phone { get; set; }
    }
}
