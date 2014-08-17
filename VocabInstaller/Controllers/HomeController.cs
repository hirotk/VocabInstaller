using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VocabInstaller.Models;
using VocabInstaller.ViewModels;

namespace VocabInstaller.Controllers {
    public class HomeController : Controller {
        private List<Question> questions;

        public HomeController() {
            questions = new List<Question>() {
                new Question(){Id=1, Word="word1", Meaning="meaning1", 
                    RegisteredDate=DateTime.Parse("2014/01/01 10:20:30")},
                new Question(){Id=2, Word="word2", Meaning="meaning2",
                    RegisteredDate=DateTime.Parse("2014/01/02 10:20:30")},
                new Question(){Id=3, Word="word3", Meaning="meaning3",
                    RegisteredDate=DateTime.Parse("2014/01/03 10:20:30")},
                new Question(){Id=4, Word="word4", Meaning="meaning4",
                    RegisteredDate=DateTime.Parse("2014/01/04 10:20:30")},
                new Question(){Id=5, Word="word5", Meaning="meaning5",
                    RegisteredDate=DateTime.Parse("2014/01/05 10:20:30")},
                new Question(){Id=6, Word="word6", Meaning="meaning6", 
                    RegisteredDate=DateTime.Parse("2014/01/06 10:20:30")},
                new Question(){Id=7, Word="word7", Meaning="meaning7",
                    RegisteredDate=DateTime.Parse("2014/01/07 10:20:30")},
                new Question(){Id=8, Word="word8", Meaning="meaning8",
                    RegisteredDate=DateTime.Parse("2014/01/08 10:20:30")},
                new Question(){Id=9, Word="word9", Meaning="meaning9",
                    RegisteredDate=DateTime.Parse("2014/01/09 10:20:30")},
                new Question(){Id=10, Word="word10", Meaning="meaning10",
                    RegisteredDate=DateTime.Parse("2014/01/10 10:20:30")},
            };        
        }

        public ActionResult Index(int page = 0, int itemsPerPage = 4) {
//            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            var viewModel = new HomeViewModel(questions, page, itemsPerPage);
            return View(viewModel);
        }

        public ActionResult About() {
//            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact() {
//            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
