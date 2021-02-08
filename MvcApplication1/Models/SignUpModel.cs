using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Models
{
    public class SignUpModel
    {
        [Key]
        public string id { get; set; }

        public string token { get; set; }
        public string phone { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string name { get; set; }


        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string email { get; set; }


        [DataType(DataType.EmailAddress)]
        [Display(Name = "Address")]
        public string address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

        public DateTime datetime { get; set; }
    }
}
