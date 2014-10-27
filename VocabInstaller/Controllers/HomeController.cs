using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VocabInstaller.Helpers;
using VocabInstaller.Models;
using VocabInstaller.ViewModels;

namespace VocabInstaller.Controllers {
    [Authorize]
    public class HomeController : Controller {
        private IViRepository repository;

        // Default Constructor
        public HomeController() : this(new ViRepository()) {
        }

        public HomeController(IViRepository repository) {
            this.repository = repository;
        }

        public ActionResult Index(int page = 0, int itemsPerPage = 10, string search = null) {
            var userRole = Session["UserRole"] as string;
            if (userRole == null) {
                Session["UserRole"] = this.GetUserRole();
            }

            int userId ;
            var uid = Session["UserId"] as int?;
            if (uid == null) {
                userId = this.GetUserId();
                Session["UserId"] = userId;
            } else {
                userId = (int)uid;
            }
            
            var viewModel = new HomeViewModel(itemsPerPage, pageSkip: 5) {
                Cards = repository.Cards.Where(c => c.UserId == userId),
                Page = page
            };

            if (search != null) {
                viewModel.Cards = viewModel.Filter(search);
            }

            if (page > viewModel.LastPage) {
                viewModel.Page = viewModel.LastPage;
            }

            viewModel.Cards = viewModel.Cards
                .OrderByDescending(c => c.CreatedAt);

            viewModel.ViewCards = viewModel.GetCardsInPage();
            return View(viewModel);
        }

        // GET: /Home/Create
        public ActionResult Create() {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var cardCreateModel = new CardCreateModel() {
                UserId = userId,
                CreatedAt = DateTime.Now,
                ReviewedAt = DateTime.Now,
                ReviewLevel = 0
            };

            return View(cardCreateModel);
        }

        // POST: /Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Id, UserId, Question, Answer, Note, CreatedAt, ReviewedAt, ReviewLevel")] 
            CardCreateModel cardCreateModel) {

            int userId = (int)(Session["UserId"] ?? this.GetUserId());
            if (cardCreateModel.UserId != userId) {
                throw new Exception("User Account Error");
            }

            cardCreateModel.CreatedAt = DateTime.Now;
            cardCreateModel.ReviewedAt = DateTime.Now;

            if (ModelState.IsValid) {
                repository.SaveCard(cardCreateModel.CardInstance);
                return RedirectToAction("Create");
            }

            return View(cardCreateModel);
        }

        private static List<SelectListItem> reviewLevelList = null;

        private static List<SelectListItem> createReviewLevelList() {
            if (reviewLevelList == null) {
                reviewLevelList = new List<SelectListItem>();
                int i;
                for (i = 0; i < 5; i++) {
                    reviewLevelList.Add(new SelectListItem() {
                        Value = i.ToString("D"), Text = string.Format("Review Level {0}", i + 1)
                    });
                }
                reviewLevelList.Add(new SelectListItem() {
                    Value = i.ToString("D"), Text = string.Format("Completed")
                });
            }
            return reviewLevelList;
        }

        // GET: /Home/Edit/5
        public ActionResult Edit(int? id, 
            int page = 0, string search = null,
            string fromController = "Home", string fromAction = "Index",
            [Bind(Include = "ItemsPerPage, PageSkip, ReviewMode, MyAnswer, AnswerTime, Blank, BlankAnswer")]
            ReviewViewModel reviewViewModel = null) {
            
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var card = repository.Cards.SingleOrDefault(c => c.UserId == userId && c.Id == id);

            if (card == null) {
                return HttpNotFound();
            }

            ViewBag.Page = page;
            ViewBag.Search = search;

            ViewBag.FromController = fromController;
            ViewBag.FromAction = fromAction;

            ViewBag.ReviewViewModel = reviewViewModel;

            ViewBag.ReviewLevelList = createReviewLevelList();

            var cardEditModel = new CardEditModel() {
                Id = card.Id,
                UserId = card.UserId,
                Question = card.Question,
                Answer = card.Answer,
                Note = card.Note,
                CreatedDate = card.CreatedAt.Date,
                CreatedTime = card.CreatedAt.TimeOfDay,
                ReviewedDate = card.ReviewedAt.Date,
                ReviewedTime = card.ReviewedAt.TimeOfDay,
                ReviewLevel = card.ReviewLevel
            };

            return View(cardEditModel);
        }

        // POST: /Home/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Id, UserId, Question, Answer, Note, CreatedDate, CreatedTime, ReviewedDate, ReviewedTime, ReviewLevel")] 
            CardEditModel cardEditModel,
            int page = 0, string search = null,
            string fromController = "Home", string fromAction = "Index",
            [Bind(Include = "ItemsPerPage, PageSkip, ReviewMode, MyAnswer, AnswerTime, Blank, BlankAnswer")]
            ReviewViewModel reviewViewModel = null) {

            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            if (cardEditModel.UserId != userId) {
                throw new Exception("User Account Error");
            }

            if (ModelState.IsValid) {
                repository.SaveCard(cardEditModel.CardInstance);
            }

            ViewBag.Page = page;
            ViewBag.Search = search;

            ViewBag.FromController = fromController;
            ViewBag.FromAction = fromAction;

            ViewBag.ReviewViewModel = reviewViewModel;

            ViewBag.ReviewLevelList = createReviewLevelList();

            return View(cardEditModel);
        }

        // GET: /Home/Delete/5
        public ActionResult Delete(int? id, int page = 0, string search = null) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var card = repository.Cards.SingleOrDefault(c => c.UserId == userId && c.Id == id);

            if (card == null) {
                return HttpNotFound();
            }

            ViewBag.Page = page;
            ViewBag.Search = search;

            return View(card);
        }

        // POST: /Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int page = 0, string search = null) {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());
            
            var card = repository.Cards.SingleOrDefault(c => c.UserId == userId && c.Id == id);

            if (card == null) {
                throw new Exception("User Account Error");
            }
            
            repository.DeleteCard(id);

            return RedirectToAction("Index", new { page = page, search = search });
        }

        public ActionResult About() {
            return View();
        }

        protected override void Dispose(bool disposing) {
            repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
