using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VocabInstaller.Models;
using VocabInstaller.Helpers;
using VocabInstaller.ViewModels;

namespace VocabInstaller.Controllers {
    public class ReviewController : Controller {
        private IViRepository repository;

        // Default Constructor
        public ReviewController() : this(new ViRepository()) { ;}

        public ReviewController(IViRepository repository) {
            this.repository = repository;
        }

        //
        // GET: /Review/
        public ActionResult Index(int page = 0) {
#if DEBUG
            DateTime dt0 = DateTime.Now.AddMilliseconds(-6),
                           dt1 = DateTime.Now.AddMilliseconds(-12),
                           dt2 = DateTime.Now.AddSeconds(-3),
                           dt3 = DateTime.Now.AddSeconds(-7),
                           dt4 = DateTime.Now.AddSeconds(-14);
#else
            DateTime dt0 = DateTime.Now.AddHours(-6),
                           dt1 = DateTime.Now.AddHours(-12),
                           dt2 = DateTime.Now.AddDays(-3),
                           dt3 = DateTime.Now.AddDays(-7),
                           dt4 = DateTime.Now.AddDays(-14);
#endif

            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var viewModel = new ReviewViewModel(itemsPerPage:1, pageSkip:2) {
                Questions = repository.Questions.Where(q => q.UserId == userId),
                Page = page
            };

            viewModel.Questions = viewModel.Questions.Where(q =>
                (q.ReviewLevel == 0 && q.ReviewedAt < dt0) ||
                (q.ReviewLevel == 1 && q.ReviewedAt < dt1) ||
                (q.ReviewLevel == 2 && q.ReviewedAt < dt2) ||
                (q.ReviewLevel == 3 && q.ReviewedAt < dt3) ||
                (q.ReviewLevel == 4 && q.ReviewedAt < dt4)
                );

            viewModel.Questions = viewModel.Questions
                .OrderBy(q => q.ReviewLevel).ThenBy(q => q.ReviewedAt);

            viewModel.ViewQuestions = viewModel.GetQuestionsInPage(page);

            return View(viewModel);
        }

    }
}
