using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public class ReviewViewModel : AbstractViewModel {
        public ReviewViewModel(int itemsPerPage, int pageSkip)
            : base(itemsPerPage, pageSkip) { }

        public ReviewViewModel() { }

        public Card ViewCard { get; set; }
        public DateTime? QuestionedAt { get; set; }
        public TimeSpan AnswerTime { get; set; }

        public string ReviewMode { get; set; }
        
        public string MyAnswer { get; set; }
        public bool IsPerfect { get; set; }
        public int? MissIndex { get; set; }

    }
}