using System;
using System.ComponentModel.DataAnnotations;
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

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime ReviewedAt { get; set; }

        [Display(Name = "Review Level")]
        [Range(0, 5, ErrorMessage = "Please enter a revew lebel between 0 and 5")]
        public int ReviewLevel { get; set; }
    }
}
