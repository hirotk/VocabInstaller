using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public class HomeViewModel {
        public HomeViewModel(int page=0, int itemsPerPage=1, int pageSkip=2) {
            Page = page;
            ItemsPerPage = itemsPerPage;
            PageSkip = pageSkip;
        }

        public HomeViewModel(List<Question> questions, 
            int page=0, int itemsPerPage=1, int pageSkip=2) 
            : this(page, itemsPerPage, pageSkip) {
            this.Questions = questions;
        }

        private List<Question> questions;
        public List<Question> Questions {
            get { return questions; }
            set {
                ItemNum = value.Count;
                questions = value;
            }
        }

        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int ItemNum { get; set; }
        public int PageSkip { get; set; }
        
        public int LastPage {
            get {
                return (ItemNum > 0) ? (ItemNum - 1) / ItemsPerPage : 0;
            }
        }

        public List<Question> GetQuestions(int? page = null) {
            int pg = page ?? Page;
            return Questions.Skip(pg * ItemsPerPage).Take(ItemsPerPage).ToList();
        }

        public bool HasPrevPage(int n) {
            return (0 <= Page - n) ? true : false;
        }
        public bool HasNextPage(int n) {
            return ((Page + n) * ItemsPerPage < ItemNum) ? true : false;
        }
    }
}
