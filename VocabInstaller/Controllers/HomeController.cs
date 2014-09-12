using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VocabInstaller.Models;
using VocabInstaller.ViewModels;
using VocabInstaller.Helpers;

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

        public ActionResult Index(int page = 0, int itemsPerPage = 4, string search = null) {
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
            
            var viewModel = new HomeViewModel(itemsPerPage, pageSkip: 2) {
                Cards = repository.Cards.Where(c => c.UserId == userId),
                Page = page
            };

            if (search != null) {
                viewModel.Cards = viewModel.Filter(search);
            }

            viewModel.Cards = viewModel.Cards
                .OrderByDescending(c => c.CreatedAt);

            viewModel.ViewCards = viewModel.GetCardsInPage(page);
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
        public ActionResult Create([Bind(Include =
            "Id, UserId, Question, Answer, Note, CreatedAt, ReviewedAt, ReviewLevel")] 
            CardCreateModel cardCreateModel) {

            int userId = (int)(Session["UserId"] ?? this.GetUserId());
            if (cardCreateModel.UserId != userId) {
                throw new Exception("User Account Error");
            }

            if (ModelState.IsValid) {
                repository.SaveCard(cardCreateModel.CardInstance);
                return RedirectToAction("Create");
            }

            return View(cardCreateModel);
        }

        // GET: /Home/Edit/5
        public ActionResult Edit(int? id, int page = 0, string search = null, string from = "Home") {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var card = repository.Cards
                .Where(c => c.UserId == userId && c.Id == id).SingleOrDefault();

            if (card == null) {
                return HttpNotFound();
            }

            ViewBag.Page = page;
            ViewBag.Search = search;
            ViewBag.From = from;

            return View(card);
        }

        // POST: /Home/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
            "Id, UserId, Question, Answer, Note, CreatedAt, ReviewedAt, ReviewLevel")] 
            Card card, int page = 0, string search = null, string from = "Home") {

            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            if (card.UserId != userId) {
                throw new Exception("User Account Error");
            }

            if (ModelState.IsValid) {
                repository.SaveCard(card);
            }

            ViewBag.Page = page;
            ViewBag.Search = search;
            ViewBag.From = from;

            return View(card);
        }

        // GET: /Home/Delete/5
        public ActionResult Delete(int? id, int page = 0, string search = null) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var card = repository.Cards
                .Where(c => c.UserId == userId && c.Id == id).SingleOrDefault();

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
            
            var card = repository.Cards
                .Where(c => c.UserId == userId && c.Id == id).SingleOrDefault();

            if (card == null) {
                throw new Exception("User Account Error");
            }
            
            repository.DeleteCard(id);

            return RedirectToAction("Index", new { page = page, search = search });
        }

        public ActionResult About() {
            return View();
        }

        public ActionResult Contact() {            
            return View();
        }

        protected override void Dispose(bool disposing) {
            repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
