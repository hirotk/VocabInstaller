using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VocabInstaller.Controllers;
using VocabInstaller.Models;
using VocabInstaller.Tests.Helpers;
using VocabInstaller.ViewModels;

namespace VocabInstaller.Tests.Controllers {
    [TestClass]
    public class ReviewControllerTest {
        private Mock<ControllerContext> ctrlContext = new Mock<ControllerContext>();
        private Mock<IViRepository> mockRepository = new Mock<IViRepository>();

        [TestInitialize]
        public void BeginTestMethod() {
            TestHelper.SetUser(ctrlContext, userId: 2);

            mockRepository.Setup(m => m.Cards).Returns(new Card[] {
                new Card {Id = 1, UserId = 2,
                    Question = "w1", Answer = "m1",
                    ReviewedAt = DateTime.Parse("2014/01/01")},
                new Card {Id = 2, UserId = 2,
                    Question = "w2", Answer = "m2",
                    ReviewedAt = DateTime.Parse("2014/01/02")},
                new Card {Id = 3, UserId = 2,
                    Question = "w3", Answer = "m3",
                    ReviewedAt = DateTime.Parse("2014/01/03")},
                new Card {Id = 4, UserId = 2,
                    Question = "w4", Answer = "m4",
                    ReviewedAt = DateTime.Parse("2014/01/04")},
                new Card {Id = 5, UserId = 2,
                    Question = "w5", Answer = "m5",
                    ReviewedAt = DateTime.Parse("2014/01/05")},
                new Card {Id = 6, UserId = 3,
                    Question = "w6", Answer = "m6",
                    ReviewedAt = DateTime.Parse("2014/01/06")},
                new Card {Id = 7, UserId = 3,
                    Question = "w7", Answer = "m7",
                    ReviewedAt = DateTime.Parse("2014/01/07")}
            }.OrderByDescending(c => c.CreatedAt)
            .AsQueryable());
        }

        [TestMethod]
        public void IndexTest() {
            // Arrange
            var controller = new ReviewController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var result = controller.Index(page: 0) as ViewResult;
            var cards = ((ReviewViewModel)result.Model).Cards.ToArray();
            var viewCard = ((ReviewViewModel)result.Model).ViewCard;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(cards.Length, 5);
            Assert.AreEqual(1, cards[0].Id);
            Assert.AreEqual(2, cards[1].Id);
            Assert.AreEqual(3, cards[2].Id);
            Assert.AreEqual(4, cards[3].Id);
            Assert.AreEqual(5, cards[4].Id);

            Assert.AreEqual(1, viewCard.Id);
        }

        [TestMethod]
        public void AnswerTest() {
            // Arrange
            var controller = new ReviewController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;
            int id = 1, page = 0;

            var result = controller.Index(page) as ViewResult;
            var viewModel = result.Model as ReviewViewModel;

            // Act
            var resultGet = controller.Answer(id, viewModel) as ViewResult;
            viewModel = resultGet.Model as ReviewViewModel;
            var card = viewModel.ViewCard;

            var resultPost = controller.Answer(id, page, "Perfect", viewModel.ReviewMode) as RedirectToRouteResult;
            mockRepository.Verify(m => m.SaveCard(card));

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("w1", card.Question);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual("Index", resultPost.RouteValues["action"]);
        }

        [TestMethod]
        public void AnswerTypingTest() {
            // Arrange
            var controller = new ReviewController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;
            int id = 1, page = 0;
            var result = controller.Index(page, "Typing") as ViewResult;
            var viewModel = result.Model as ReviewViewModel;
            viewModel.QuestionedAt = DateTime.Now;
            viewModel.MyAnswer = "w1";

            // Act
            var resultGet = controller.Answer(id, viewModel) as ViewResult;
            viewModel = resultGet.Model as ReviewViewModel;
            var card = viewModel.ViewCard;
            var evaluation = viewModel.IsPerfect ? "Perfect" : "Almost";
            var resultPost = controller.Answer(id, page, evaluation, viewModel.ReviewMode) as RedirectToRouteResult;
            mockRepository.Verify(m => m.SaveCard(card));

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("Perfect", evaluation);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual("Index", resultPost.RouteValues["action"]);
        }

        [TestMethod]
        public void AnswerBlankTest() {
            // Arrange
            var controller = new ReviewController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;
            int id = 1, page = 0;
            var result = controller.Index(page, "Blank") as ViewResult;
            var viewModel = result.Model as ReviewViewModel;
            viewModel.QuestionedAt = DateTime.Now;
            viewModel.MyAnswer = "w";
            viewModel.Blank = "_1";
            viewModel.BlankAnswer = 'w';

            // Act
            var resultGet = controller.Answer(id, viewModel) as ViewResult;
            var card = viewModel.ViewCard as Card;
            var evaluation = viewModel.IsPerfect ? "Perfect" : "Almost";

            var resultPost = controller.Answer(id, page, evaluation, viewModel.ReviewMode) as RedirectToRouteResult;
            mockRepository.Verify(m => m.SaveCard(card));

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("Perfect", evaluation);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual("Index", resultPost.RouteValues["action"]);
        }

        [TestMethod]
        public void StatusTest() {
            // Arrange
            var controller = new ReviewController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var result = controller.Status() as ViewResult;
            var cards = ((IQueryable<Card>)result.Model).ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(cards.Length, 5);
        }
    }
}
