using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using VocabInstaller.Models;
using VocabInstaller.Validations;

namespace VocabInstaller.ViewModels {
    public class CardCreateModel {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int UserId { get; set; }

        [Required]
        [StringLength(maximumLength: 128)]
        [Unique("Question", userEach: true, ErrorMessage = "The question already exists.")]
        public string Question { get; set; }

        [Required]
        [StringLength(maximumLength: 256)]
        public string Answer { get; set; }

        [StringLength(maximumLength: 512)]
        public string Note { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime ReviewedAt { get; set; }

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
                    CreatedAt = this.CreatedAt,
                    ReviewedAt = this.ReviewedAt,
                    ReviewLevel = this.ReviewLevel
                };
                return card;
            }
        }
    }
}
