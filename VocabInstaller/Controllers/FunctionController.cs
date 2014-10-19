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
        // GET: /Function/Export
        public ActionResult Export() {
            return View();
        }

        //
        // POST: /Function/Export
        [HttpPost, ActionName("Export")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ExportConfirmed() {
            var cards = repository.Cards;

            string userRole = (string)(Session["UserRole"] ?? this.GetUserRole());
            if (userRole == "User") {
                int userId = (int)(Session["UserId"] ?? this.GetUserId());
                cards = cards.Where(c => c.UserId == userId);
            }

            var sb = new StringBuilder();
            cards.OrderBy(c => c.Id).ToList().ForEach(c => sb.Append(
                string.Format("{0}\t{1}\t{2}\t{3}\t\"{4}\"\t{5}\t{6}\t{7}\r\n",
                c.Id, c.UserId,
                (c.Question ?? string.Empty).Replace('"', '”'),
                (c.Answer ?? string.Empty).Replace('"', '”'),
                (c.Note ?? string.Empty).Replace('"', '”'),
                c.CreatedAt, c.ReviewedAt, c.ReviewLevel)
            ));

            string fileName = string.Format("ViDat_{0}.csv",
                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", fileName);
        }

        //
        // GET: /Function/Import
        public ActionResult Import() {
            return View();
        }

        // POST: /Function/Import
        [HttpPost, ActionName("Import")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ImportConfirmed(HttpPostedFileBase csvFile = null, bool overwrite = false) {
            var cards = repository.Cards;

            bool isAdmin = false;
            int userId = -1;
            string userRole = (string)(Session["UserRole"] ?? this.GetUserRole());
            if (userRole == "Administrator") {
                isAdmin = true;
            } else if (userRole == "User") {
                userId = (int)(Session["UserId"] ?? this.GetUserId());
                cards = cards.Where(c => c.UserId == userId);
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
            var cardList = cards.OrderByDescending(c => c.CreatedAt).ToList();

            cardList.AddRange(records.Select(record => record.Split('\t'))
                    .Select(fields => new Card() {
                        Id = overwrite ? int.Parse(fields[0]) : 0,
                        UserId = isAdmin ? int.Parse(fields[1]) : userId,
                        Question = fields[2],
                        Answer = fields[3],
                        Note = fields[4].Replace('\f', '\n'),
                        CreatedAt = String.IsNullOrEmpty(fields[5]) ? DateTime.Now : DateTime.Parse(fields[5]),
                        ReviewedAt = String.IsNullOrEmpty(fields[6]) ? DateTime.Now : DateTime.Parse(fields[6]),
                        ReviewLevel = int.Parse(fields[7])
                    }));

            foreach (var c in cardList) {
                create(c);
            }

            return View(cardList);
        }

        private void create([Bind(Include =
            "Id, UserId, Question, Answer, Note, CreatedAt, ReviewedAt, ReviewLevel")] 
            Card card) {

            if (ModelState.IsValid) {
                repository.SaveCard(card);
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
            var cards = repository.Cards;

            string userRole = (string)(Session["UserRole"] ?? this.GetUserRole());
            if (userRole == "User") {
                int userId = (int)(Session["UserId"] ?? this.GetUserId());
                cards = cards.Where(c => c.UserId == userId);
            }

            var cardList = cards.ToList();
            var deletedList = new List<Card>();
            foreach (var c in cardList) {
                var deleted = repository.DeleteCard(c.Id);
                deletedList.Add(deleted);
            }

            ViewBag.Result = "The database was initialized";

            return View(deletedList);
        }

    }
}
