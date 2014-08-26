using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public abstract class AbstractViewModel {
        protected AbstractViewModel(int itemsPerPage, int pageSkip) {
            this.ItemsPerPage = itemsPerPage;
            this.PageSkip = pageSkip;
            Page = 0;
        }

        public int ItemsPerPage { get; set; }
        public int PageSkip { get; set; }
        public int ItemNum { get; set; }

        private int page;
        public int Page {
            get {
                return page < 0 ? 0 : page > this.LastPage ? this.LastPage : page;
            }
            set { page = value; }
        }

        public int LastPage {
            get {
                return (ItemNum > 0) ? (ItemNum - 1) / ItemsPerPage : 0;
            }
        }

        public bool HasPrevPage(int n) {
            return (0 <= Page - n) ? true : false;
        }

        public bool HasNextPage(int n) {
            return ((Page + n) * ItemsPerPage < ItemNum) ? true : false;
        }

        private IQueryable<Question> questions;
        public IQueryable<Question> Questions {
            get { return questions; }
            set {
                questions = value;
                ItemNum = questions == null ? 0 : questions.Count();
            }
        }

        public IQueryable<Question> ViewQuestions { get; set; }

        public IQueryable<Question> GetQuestionsInPage(int? page = null) {
            int pg = page ?? Page;
            return Questions.Skip(pg * ItemsPerPage).Take(ItemsPerPage);
        }
    }
}
