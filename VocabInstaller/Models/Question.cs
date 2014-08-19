using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VocabInstaller.Models {
    public class Question {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int UserId { get; set; }

        [Required]
        [StringLength(maximumLength: 128)]
        public string Word { get; set; }        
        
        [Required]
        [StringLength(maximumLength: 256)]
        public string Meaning { get; set; }

        [StringLength(maximumLength: 512)]
        public string Note { get; set; }

        public DateTime RegisteredDate { get; set; }
    }
}
