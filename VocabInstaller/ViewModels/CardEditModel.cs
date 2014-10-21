using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using VocabInstaller.Models;
using VocabInstaller.Validations;

namespace VocabInstaller.ViewModels {
    public class CardEditModel {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int UserId { get; set; }

        [Required]
        [StringLength(maximumLength: 256)]
        [Unique("Question", userEach: true, editMode: true, ErrorMessage = "The question already exists.")]
        public string Question { get; set; }

        [Required]
        [StringLength(maximumLength: 256)]
        public string Answer { get; set; }

        [StringLength(maximumLength: 512)]
        public string Note { get; set; }

//        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}", ApplyFormatInEditMode = true)]
//        public DateTime CreatedAt { get; set; }
        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created Time")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan CreatedTime { get; set; }

//        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}", ApplyFormatInEditMode = true)]
//        public DateTime ReviewedAt { get; set; }
        [Display(Name = "Reviewed Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime ReviewedDate { get; set; }

        [Display(Name = "Reviewed Time")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan ReviewedTime { get; set; }

        [Display(Name = "Review Level")]
        [Range(0, 5, ErrorMessage = "Please enter a revew lebel between 0 and 5")]
        public int ReviewLevel { get; set; }

        public Card CardInstance {
            get {
                var card = new Card() {
                    Id = this.Id,
                    UserId = this.UserId,
                    Question = this.Question,
                    Answer = this.Answer,
                    Note = this.Note,
                    CreatedAt = this.CreatedDate.Add(this.CreatedTime),
                    ReviewedAt = this.ReviewedDate.Add(this.ReviewedTime),
                    ReviewLevel = this.ReviewLevel
                };
                return card;
            }
        }
    }
}
