using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VocabInstaller;
using VocabInstaller.Controllers;
using VocabInstaller.Models;
using VocabInstaller.ViewModels;

namespace VocabInstaller.Tests.Controllers {
    [TestClass]
    public class HomeControllerTest {
        private Mock<IViRepository> mockRepository = new Mock<IViRepository>();

        [TestInitialize]
        public void BeginTestMethod() {
            mockRepository.Setup(m => m.Questions).Returns(new Question[] {
                new Question {Id = 1, UserId = 2,
                    Word = "w1", Meaning = "m1",
                    RegisteredDate = DateTime.Parse("2014/01/01")},
                new Question {Id = 2, UserId = 2,
                    Word = "w2", Meaning = "m2",
                    RegisteredDate = DateTime.Parse("2014/01/02")},
                new Question {Id = 3, UserId = 2,
                    Word = "w3", Meaning = "m3",
                    RegisteredDate = DateTime.Parse("2014/01/03")},
                new Question {Id = 4, UserId = 2,
                    Word = "w4", Meaning = "m4",
                    RegisteredDate = DateTime.Parse("2014/01/04")},
                new Question {Id = 5, UserId = 2,
                    Word = "w5", Meaning = "m5",
                    RegisteredDate = DateTime.Parse("2014/01/05")}
            }.OrderByDescending(q => q.RegisteredDate)
            .AsQueryable());
        }

        [TestMethod]
        public void IndexTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);

            // Act
            var result = controller.Index() as ViewResult;
            var questions = ((HomeViewModel)result.Model).Questions.ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(questions.Length, 5);
            Assert.AreEqual("w5", questions[0].Word);
            Assert.AreEqual("w4", questions[1].Word);
            Assert.AreEqual("w3", questions[2].Word);
            Assert.AreEqual("w2", questions[3].Word);
            Assert.AreEqual("w1", questions[4].Word);
        }

        [TestMethod]
        public void CreateTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);

            // Act
            var resultGet = controller.Create() as ViewResult;
            var question = resultGet.Model as Question;
            question.Word = "w6";
            question.Meaning = "m6";
            question.RegisteredDate = DateTime.Parse("2014/01/06");

            var resultPost = controller.Create(question) as RedirectToRouteResult;
            mockRepository.Verify(m => m.SaveQuestion(question));
            
            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("Create", resultPost.RouteValues["action"]);
        }

        [TestMethod]
        public void EditTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
                        
            // Act
            var resultGet = controller.Edit(1) as ViewResult;
            var question = resultGet.Model as Question;
            var wordBefore = question.Word;
            question.Word = "w1+";

            var resultPost = controller.Edit(question) as ViewResult;
            mockRepository.Verify(m => m.SaveQuestion(question));

            var questionAfter = resultPost.Model as Question;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("w1", wordBefore);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual("w1+", questionAfter.Word);
        }

        [TestMethod]
        public void DeleteTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);

            // Act
            var resultGet = controller.Delete(1) as ViewResult;
            var question = resultGet.Model as Question;

            var resultPost = controller.DeleteConfirmed(question.Id) as RedirectToRouteResult;
            mockRepository.Verify(m => m.DeleteQuestion(question.Id));

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual(1, question.Id);
            Assert.AreEqual("Index", resultPost.RouteValues["action"]);
        }

        [TestMethod]
        public void AboutTest() {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ContactTest() {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
