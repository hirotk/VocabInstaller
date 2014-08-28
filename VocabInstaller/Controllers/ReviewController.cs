using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
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


        //
        // GET: /Review/5
        public ActionResult Answer(int id, int page) {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var question = repository.Questions
                .Where(q => q.UserId == userId && q.Id == id).SingleOrDefault();

            if (question == null) {
                return HttpNotFound();
            }

            ViewBag.Page = page;

            return View(question);
        }

        //
        // POST: /Review/Answer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Answer(int id, int page, string answer) {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var question = repository.Questions
                .Where(q => q.UserId == userId && q.Id == id).SingleOrDefault();

            if (question == null) {
                throw new Exception("User Account Error");
            }

            if (answer == "Perfect") {
                if (question.ReviewLevel < 5) {
                    question.ReviewLevel += 1;
                }
            } else {
                if (question.ReviewLevel > 0) {
                    question.ReviewLevel -= 1;
                }
            }

            question.ReviewedAt = DateTime.Now;

            if (ModelState.IsValid) {
                repository.SaveQuestion(question);
            }

            return RedirectToAction("Index", "Review", new { page = page });
        }

        //
        // GET: /Status/
        public ActionResult Status() {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());
            var questions = repository.Questions
                .Where(q => q.UserId == userId);

            return View(questions);
        }

        public ActionResult DisplayGraph(string revLvStr, int width = 400, int height = 300) {
            string[] ary = revLvStr.Split(',');
            int[] revLv = new int[ary.Length];
            for (int i = 0; i < ary.Length; i++) {
                revLv[i] = int.Parse(ary[i]);
            }

            // Draw a chart
            var chart = new Chart();
            chart.Width = width;
            chart.Height = height;
            chart.BackColor = Color.WhiteSmoke;

            chart.ChartAreas.Add("Main");
            chart.ChartAreas["Main"].BackColor = Color.Azure;

            var AxisX = chart.ChartAreas["Main"].AxisX;
            AxisX.LineColor = Color.Red;
            AxisX.Title = "Review Level";

            var AxisY = chart.ChartAreas["Main"].AxisY;
            AxisY.LineColor = Color.Blue;
            AxisY.Title = "Number of Items";

            chart.Series.Add("ReviewStatus");
            var reviewStatus = chart.Series["ReviewStatus"];
            reviewStatus.ChartArea = "Main";
            reviewStatus.ChartType = SeriesChartType.Bar;
            reviewStatus.Color = Color.Orange;

            string[] xValues = { "Lv1", "Lv2", "Lv3", "Lv4", "Lv5" };
            int[] yValues = { revLv[0], revLv[1], revLv[2], revLv[3], revLv[4] };

            reviewStatus.Points.DataBindXY(xValues, yValues);

            // Output the chart to a png image
            var imgStream = new MemoryStream();
            chart.SaveImage(imgStream, ChartImageFormat.Png);
            imgStream.Seek(0, SeekOrigin.Begin);

            return File(imgStream, "image/png");
        }

        protected override void Dispose(bool disposing) {
            repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
