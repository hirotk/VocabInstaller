using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using VocabInstaller.Models;
using VocabInstaller.Helpers;
using System.Web;
using System.IO;

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

        //
        // GET: /Function/Load
        public ActionResult Load() {
            return View();
        }

        // POST: /Function/Load
        [HttpPost, ActionName("Load")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult LoadConfirmed(HttpPostedFileBase csvFile = null, bool overwrite = false) {
            var questions = repository.Questions;

            bool isAdmin = false;
            int userId = -1;
            string userRole = (string)(Session["UserRole"] ?? this.GetUserRole());
            if (userRole == "Administrator") {
                isAdmin = true;
            } else if (userRole == "User") {
                userId = (int)(Session["UserId"] ?? this.GetUserId());
                questions = questions.Where(q => q.UserId == userId);
            }

            if (csvFile == null) { throw new FileNotFoundException("csvFile should not be null"); }

            var sb = new StringBuilder();

            using (var sr = new StreamReader(csvFile.InputStream)) {
                bool multi = false;
                while (!sr.EndOfStream) {
                    var line = sr.ReadLine();
                    var ary = string.Format("{0}\n", line).ToCharArray();
                    var dstAry = new char[ary.Length];
                    int j = 0;
                    foreach (char c in ary) {
                        if (!multi && c == '"') { multi = true; continue; }
                        if (multi && c == '\n') { dstAry[j++] = '\f'; continue; }
                        if (multi && c == '"') { multi = false; continue; }
                        dstAry[j++] = c;
                    }
                    var s = new string(dstAry);
                    s = s.TrimEnd('\0');
                    sb.Append(s);
                }
                if (multi) { throw new Exception("The multi line field has an error."); }
            }

            var records = sb.ToString().TrimEnd().Split('\n');
            var questionList = questions.ToList();

            questionList.AddRange(records.Select(record => record.Split('\t'))
                    .Select(fields => new Question() {
                        Id = overwrite ? int.Parse(fields[0]) : 0,
                        UserId = isAdmin ? int.Parse(fields[1]) : userId,
                        Word = fields[2],
                        Meaning = fields[3],
                        Note = fields[4].Replace('\f', '\n'),
                        CreatedAt = DateTime.Parse(fields[5]),
                        ReviewedAt = DateTime.Parse(fields[6]),
                        ReviewLevel = int.Parse(fields[7])
                    }));

            foreach (var q in questionList) {
                create(q);
            }

            return View(questionList);
        }

        private void create([Bind(Include =
            "Id, UserId, Word, Meaning, Note, CreatedAt, ReviewedAt, ReviewLevel")] 
            Question question) {

            if (ModelState.IsValid) {
                repository.SaveQuestion(question);
            }
        }

        // GET: /Function/Initialize
        public ActionResult Initialize() {
            return View();
        }

        // POST: /Function/Initialize
        [HttpPost, ActionName("Initialize")]
        [ValidateAntiForgeryToken]
        public ActionResult InitializeConfirmed() {
            var questions = repository.Questions;

            string userRole = (string)(Session["UserRole"] ?? this.GetUserRole());
            if (userRole == "User") {
                int userId = (int)(Session["UserId"] ?? this.GetUserId());
                questions = questions.Where(q => q.UserId == userId);
            }

            var questionList = questions.ToList();
            var deletedList = new List<Question>();
            foreach (var q in questionList) {
                var deleted = repository.DeleteQuestion(q.Id);
                deletedList.Add(deleted);
            }

            ViewBag.Result = "The database was initialized";
            deletedList.ForEach(d => questionList.Remove(d));

            return View(questionList);
        }

    }
}
