using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using VocabInstaller.Models;
using VocabInstaller.Helpers;

namespace VocabInstaller.Controllers {
//    [Authorize(Roles = "Administrator, User")]
    [Authorize(Roles = "Administrator")]
    public class FunctionController : Controller {
        private IViRepository repository;

        // Default Constructor
        public FunctionController() : this(new ViRepository()) { }

        public FunctionController(IViRepository repository) {
            this.repository = repository;
        }

        //
        // GET: /Function/
        public ActionResult Index() {
            return View();
        }

        //
        // GET: /Function/Save
        public ActionResult Save() {
            return View();
        }

        //
        // POST: /Function/Save
        [HttpPost, ActionName("Save")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveConfirmed() {            
            var questions = repository.Questions;

            string userRole = (string)(Session["UserRole"] ?? this.GetUserRole());
            if (userRole == "User") {
                int userId = (int)(Session["UserId"] ?? this.GetUserId());
                questions = questions.Where(q => q.UserId == userId);
            }

            var sb = new StringBuilder();
            questions.OrderBy(q => q.Id).ToList().ForEach(q => sb.Append(
                string.Format("{0}\t{1}\t{2}\t{3}\t\"{4}\"\t{5}\t{6}\t{7}\r\n",
                q.Id, q.UserId,
                (q.Word ?? string.Empty).Replace('"', '”'),
                (q.Meaning ?? string.Empty).Replace('"', '”'),
                (q.Note ?? string.Empty).Replace('"', '”'),
                q.CreatedAt, q.ReviewedAt, q.ReviewLevel)
            ));

            string fileName = string.Format("ViDat_{0}.csv", 
                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", fileName);
        }

    }
}
