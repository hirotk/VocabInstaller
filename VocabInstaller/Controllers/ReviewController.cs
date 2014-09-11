using System;
using System.Linq;
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
        public ReviewController() : this(new ViRepository()) {}

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
                Cards = repository.Cards.Where(c => c.UserId == userId),
                Page = page
            };

            viewModel.Cards = viewModel.Cards.Where(c =>
                (c.ReviewLevel == 0 && c.ReviewedAt < dt0) ||
                (c.ReviewLevel == 1 && c.ReviewedAt < dt1) ||
                (c.ReviewLevel == 2 && c.ReviewedAt < dt2) ||
                (c.ReviewLevel == 3 && c.ReviewedAt < dt3) ||
                (c.ReviewLevel == 4 && c.ReviewedAt < dt4)
                );

            viewModel.Cards = viewModel.Cards
                .OrderBy(c => c.ReviewLevel).ThenBy(c => c.ReviewedAt);

            viewModel.ViewCards = viewModel.GetCardsInPage(page);

            return View(viewModel);
        }


        //
        // GET: /Review/5
        public ActionResult Answer(int id, int page) {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var card = repository.Cards
                .Where(c => c.UserId == userId && c.Id == id).SingleOrDefault();

            if (card == null) {
                return HttpNotFound();
            }

            ViewBag.Page = page;

            return View(card);
        }

        //
        // POST: /Review/Answer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Answer(int id, int page, string answer) {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());

            var card = repository.Cards
                .Where(c => c.UserId == userId && c.Id == id).SingleOrDefault();

            if (card == null) {
                throw new Exception("User Account Error");
            }

            if (answer == "Perfect") {
                if (card.ReviewLevel < 5) {
                    card.ReviewLevel += 1;
                }
            } else {
                if (card.ReviewLevel > 0) {
                    card.ReviewLevel -= 1;
                }
            }

            card.ReviewedAt = DateTime.Now;

            if (ModelState.IsValid) {
                repository.SaveCard(card);
            }

            return RedirectToAction("Index", "Review", new { page = page });
        }

        //
        // GET: /Status/
        public ActionResult Status() {
            int userId = (int)(Session["UserId"] ?? this.GetUserId());
            var cards = repository.Cards
                .Where(c => c.UserId == userId);

            return View(cards);
        }

        public ActionResult DrawGraph(string revLvStr, int width = 400, int height = 300) {
            string[] ary = revLvStr.Split(',');
            int[] revLv = new int[ary.Length];
            for (int i = 0; i < ary.Length; i++) {
                revLv[i] = int.Parse(ary[i]);
            }

            // Draw a chart
            var chart = new Chart();
            chart.Width = width;
            chart.Height = height;
            chart.BackColor = Color.Gainsboro;
            chart.BackSecondaryColor = Color.LightGray;
            chart.BackGradientStyle = GradientStyle.TopBottom;
            chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            chart.BorderSkin.PageColor = Color.Transparent;

            chart.ChartAreas.Add("Main");
            chart.ChartAreas["Main"].BackColor = Color.WhiteSmoke;
            chart.ChartAreas["Main"].BorderColor = Color.Gray;

            var axisX = chart.ChartAreas["Main"].AxisX;
            axisX.LineColor = Color.LightGray;
            axisX.MajorGrid.LineColor = Color.DarkGray;
            axisX.Title = "Review Level";
            axisX.TitleFont = new Font(FontFamily.GenericSerif, 12.0f, FontStyle.Bold);

            var axisY = chart.ChartAreas["Main"].AxisY;
            axisY.LineColor = Color.LightGray;
            axisY.MajorGrid.LineColor = Color.DarkGray;
            axisY.Title = "Number of Items";
            axisY.TitleFont = new Font(FontFamily.GenericSerif, 12.0f, FontStyle.Bold);

            chart.Series.Add("ReviewStatus");
            var reviewStatus = chart.Series["ReviewStatus"];
            reviewStatus.ChartArea = "Main";
            reviewStatus.ChartType = SeriesChartType.Bar;
            reviewStatus.MarkerStyle = MarkerStyle.Circle;
            reviewStatus.MarkerColor = Color.DimGray;
            reviewStatus.IsValueShownAsLabel = true; 
            reviewStatus.LabelFormat = "#0";

            string[] xValues = { "Lv0", "Lv1", "Lv2", "Lv3", "Lv4" };
            int[] yValues = { revLv[0], revLv[1], revLv[2], revLv[3], revLv[4] };

            reviewStatus.Points.DataBindXY(xValues, yValues);
            reviewStatus.Points[0].Color = Color.Red;
            reviewStatus.Points[0].BackSecondaryColor = Color.FromArgb(192, 255, 192, 192);
            reviewStatus.Points[1].Color = Color.Magenta;
            reviewStatus.Points[1].BackSecondaryColor = Color.FromArgb(192, 255, 192, 255);
            reviewStatus.Points[2].Color = Color.Orange;
            reviewStatus.Points[2].BackSecondaryColor = Color.FromArgb(192, 255, 224, 192);
            reviewStatus.Points[3].Color = Color.Gold;
            reviewStatus.Points[3].BackSecondaryColor = Color.FromArgb(192, 255, 245, 192);
            reviewStatus.Points[4].Color = Color.Turquoise;
            reviewStatus.Points[4].BackSecondaryColor = Color.FromArgb(192, 192, 255, 250);
            reviewStatus.BackGradientStyle = GradientStyle.HorizontalCenter;

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
