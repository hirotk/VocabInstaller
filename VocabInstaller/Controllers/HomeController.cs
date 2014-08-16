using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VocabInstaller.Models;

namespace VocabInstaller.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
//            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            var questions = new List<Question>(){
                new Question(){Id=1, Word="word1", Meaning="meaning1"},
                new Question(){Id=2, Word="word2", Meaning="meaning2"},
                new Question(){Id=3, Word="word3", Meaning="meaning3"},
                new Question(){Id=4, Word="word4", Meaning="meaning4"},
                new Question(){Id=5, Word="word5", Meaning="meaning5"},
            };

            return View(questions);
        }

        public ActionResult About() {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
