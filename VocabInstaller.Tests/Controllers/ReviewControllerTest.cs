using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VocabInstaller;
using VocabInstaller.Models;
using VocabInstaller.Controllers;
using VocabInstaller.ViewModels;
using VocabInstaller.Tests.Helpers;

namespace VocabInstaller.Tests.Controllers {
    [TestClass]
    public class ReviewControllerTest {
        private Mock<ControllerContext> ctrlContext = new Mock<ControllerContext>();
        private Mock<IViRepository> mockRepository = new Mock<IViRepository>();        

        [TestInitialize]
        public void BeginTestMethod() {
            TestHelper.SetUserId(ctrlContext, userId: 2);

            mockRepository.Setup(m => m.Questions).Returns(new Question[] {
                new Question {Id = 1, UserId = 2,
                    Word = "w1", Meaning = "m1",
                    ReviewedAt = DateTime.Parse("2014/01/01")},
                new Question {Id = 2, UserId = 2,
                    Word = "w2", Meaning = "m2",
                    ReviewedAt = DateTime.Parse("2014/01/02")},
                new Question {Id = 3, UserId = 2,
                    Word = "w3", Meaning = "m3",
                    ReviewedAt = DateTime.Parse("2014/01/03")},
                new Question {Id = 4, UserId = 2,
                    Word = "w4", Meaning = "m4",
                    ReviewedAt = DateTime.Parse("2014/01/04")},
                new Question {Id = 5, UserId = 2,
                    Word = "w5", Meaning = "m5",
                    ReviewedAt = DateTime.Parse("2014/01/05")},
                new Question {Id = 6, UserId = 3,
                    Word = "w6", Meaning = "m6",
                    ReviewedAt = DateTime.Parse("2014/01/06")},
                new Question {Id = 7, UserId = 3,
                    Word = "w7", Meaning = "m7",
                    ReviewedAt = DateTime.Parse("2014/01/07")}
            }.OrderByDescending(q => q.CreatedAt)
            .AsQueryable());
        }

        [TestMethod]
        public void IndexTest() {
            // Arrange
            var controller = new ReviewController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var result = controller.Index(page: 0) as ViewResult;
            var questions = ((ReviewViewModel)result.Model).Questions.ToArray();
            var viewQuestions = ((ReviewViewModel)result.Model).ViewQuestions.ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(questions.Length, 5);
            Assert.AreEqual(1, questions[0].Id);
            Assert.AreEqual(2, questions[1].Id);
            Assert.AreEqual(3, questions[2].Id);
            Assert.AreEqual(4, questions[3].Id);
            Assert.AreEqual(5, questions[4].Id);

            Assert.AreEqual(viewQuestions.Length, 1);
            Assert.AreEqual(1, questions[0].Id);
        }

        [TestMethod]
        public void AnswerTest() {
            // Arrange
            var controller = new ReviewController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;
            int id = 1, page = 0;

            // Act
            var resultGet = controller.Answer(id, page) as ViewResult;
            var question = resultGet.Model as Question;

            var resultPost = controller.Answer(id, page, "Perfect") as RedirectToRouteResult;
            mockRepository.Verify(m => m.SaveQuestion(question));

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("w1", question.Word);
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
            var questions = ((IQueryable<Question>)result.Model).ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(questions.Length, 5);
        }
    }
}
