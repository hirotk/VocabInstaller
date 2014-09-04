using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VocabInstaller.Controllers;
using VocabInstaller.Models;
using VocabInstaller.ViewModels;
using VocabInstaller.Tests.Helpers;

namespace VocabInstaller.Tests.Controllers {
    [TestClass]
    public class HomeControllerTest {
        private Mock<ControllerContext> ctrlContext = new Mock<ControllerContext>();
        private Mock<IViRepository> mockRepository = new Mock<IViRepository>();        

        [TestInitialize]
        public void BeginTestMethod() {
            TestHelper.SetUser(ctrlContext, userId: 2);

            mockRepository.Setup(m => m.Questions).Returns(new Question[] {
                new Question {Id = 1, UserId = 2,
                    Word = "Why don't you try it?", Meaning = "m1",
                    CreatedAt = DateTime.Parse("2014/01/01")},
                new Question {Id = 2, UserId = 2,
                    Word = "Even though it's difficult, it's worth trying.", Meaning = "m2",
                    CreatedAt = DateTime.Parse("2014/01/02")},
                new Question {Id = 3, UserId = 2,
                    Word = "What a wonderful day!", Meaning = "m3",
                    CreatedAt = DateTime.Parse("2014/01/03")},
                new Question {Id = 4, UserId = 2,
                    Word = "It's only 1.2$.", Meaning = "m4",
                    CreatedAt = DateTime.Parse("2014/01/04")},
                new Question {Id = 5, UserId = 2,
                    Word = "2 * 3 + 6 / 2 = 9", Meaning = "m5",
                    CreatedAt = DateTime.Parse("2014/01/05")},
                new Question {Id = 6, UserId = 3,
                    Word = "w6", Meaning = "m6",
                    CreatedAt = DateTime.Parse("2014/01/06")},
                new Question {Id = 7, UserId = 3,
                    Word = "w7", Meaning = "m7",
                    CreatedAt = DateTime.Parse("2014/01/07")}
            }.OrderByDescending(q => q.CreatedAt)
            .AsQueryable());
        }

        [TestMethod]
        public void IndexTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var result = controller.Index(page:0, itemsPerPage:3) as ViewResult;
            var questions = ((HomeViewModel)result.Model).Questions.ToArray();
            var viewQuestions = ((HomeViewModel)result.Model).ViewQuestions.ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(questions.Length, 5);
            Assert.AreEqual(5, questions[0].Id);
            Assert.AreEqual(4, questions[1].Id);
            Assert.AreEqual(3, questions[2].Id);
            Assert.AreEqual(2, questions[3].Id);
            Assert.AreEqual(1, questions[4].Id);

            Assert.AreEqual(viewQuestions.Length, 3);
            Assert.AreEqual(5, questions[0].Id);
            Assert.AreEqual(4, questions[1].Id);
            Assert.AreEqual(3, questions[2].Id);
        }

        [TestMethod]
        public void CreateTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Create() as ViewResult;
            var question = resultGet.Model as Question;
            question.Word = "w6";
            question.Meaning = "m6";
            question.CreatedAt = DateTime.Parse("2014/01/06");

            var resultPost = controller.Create(question) as RedirectToRouteResult;
            mockRepository.Verify(m => m.SaveQuestion(question));
            
            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("Create", resultPost.RouteValues["action"]);
        }

        [TestMethod]
        public void CanNotCreateTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            TestHelper.SetUser(ctrlContext, userId: 3);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Create() as ActionResult;
            Exception ex = null;
            try {
                var question = new Question() { UserId = 2, Word = "w", Meaning = "m" };
                var resultPost = controller.Create(question) as ActionResult;
            } catch (Exception e) {
                ex = e;
            }

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("User Account Error", ex.Message);
        }

        [TestMethod]
        public void EditTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;
                        
            // Act
            var resultGet = controller.Edit(1) as ViewResult;
            var question = resultGet.Model as Question;
            var wordBefore = question.Word;
            question.Word = "Would you like to try it?";

            var resultPost = controller.Edit(question) as ViewResult;
            mockRepository.Verify(m => m.SaveQuestion(question));

            var questionAfter = resultPost.Model as Question;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("Why don't you try it?", wordBefore);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual("Would you like to try it?", questionAfter.Word);
        }

        [TestMethod]
        public void CanNotEditTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            TestHelper.SetUser(ctrlContext, userId: 3);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Edit(id:1) as ActionResult;            
            var question = new Question(){Id =1, UserId=2, Word="", Meaning=""};
            Exception ex = null;
            try {
                var resultPost = controller.Edit(question) as ActionResult;
            } catch(Exception e) {
                ex = e;
            }
            
            // Assert
            Assert.IsInstanceOfType(resultGet, typeof(HttpNotFoundResult));
            Assert.AreEqual("User Account Error", ex.Message);
        }

        [TestMethod]
        public void DeleteTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Delete(1) as ViewResult;
            var question = resultGet.Model as Question;

            var resultPost = controller.DeleteConfirmed(question.Id) as RedirectToRouteResult;
            mockRepository.Verify(m => m.DeleteQuestion(question.Id));

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual(1, question.Id);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual("Index", resultPost.RouteValues["action"]);
        }

        [TestMethod]
        public void CanNotDeleteTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            TestHelper.SetUser(ctrlContext, userId: 3);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Delete(id: 1) as ActionResult;
            Exception ex = null;
            try {
                var resultPost = controller.DeleteConfirmed(1) as ActionResult;
            } catch (Exception e) {
                ex = e;
            }

            // Assert
            Assert.IsInstanceOfType(resultGet, typeof(HttpNotFoundResult));
            Assert.AreEqual("User Account Error", ex.Message);
        }

        [TestMethod]
        public void CanPaginateTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var result = controller.Index(itemsPerPage: 3) as ViewResult;
            var viewModel = result.Model as HomeViewModel;
            Question[] questionArray = viewModel.GetQuestionsInPage(page:1).ToArray();

            // Assert
            Assert.IsTrue(questionArray.Length == 2);
            Assert.AreEqual(questionArray[0].Id, 2);
            Assert.AreEqual(questionArray[1].Id, 1);
        }

        [TestMethod]
        public void CanFilterTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Action
            var viewModel1 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage:10, search:"it? | It's")).Model);
            Question[] questions1 = viewModel1.Questions.ToArray();

            var viewModel2 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "try")).Model);
            Question[] questions2 = viewModel2.Questions.ToArray();

            var viewModel3 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "&quot;try&quot;")).Model);
            Question[] questions3 = viewModel3.Questions.ToArray();

            var viewModel4 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "! | $ | /")).Model);
            Question[] questions4 = viewModel4.Questions.ToArray();

            var viewModel5 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "&quot;won*ful&quot;")).Model);
            Question[] questions5 = viewModel5.Questions.ToArray();

            var viewModel6 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "&quot;a*ful&quot;")).Model);
            Question[] questions6 = viewModel6.Questions.ToArray();

            var viewModel7 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: @"&quot;**&quot;")).Model);
            Question[] questions7 = viewModel7.Questions.ToArray();

            // Assert
            Assert.AreEqual(questions1.Length, 3);
            Assert.IsTrue(questions1[0].Id == 4);
            Assert.IsTrue(questions1[1].Id == 2);
            Assert.IsTrue(questions1[2].Id == 1);

            Assert.AreEqual(questions2.Length, 2);
            Assert.IsTrue(questions2[0].Id == 2);
            Assert.IsTrue(questions2[1].Id == 1);

            Assert.AreEqual(questions3.Length, 1);
            Assert.IsTrue(questions3[0].Id == 1);

            Assert.AreEqual(questions4.Length, 3);
            Assert.IsTrue(questions4[0].Id == 5);
            Assert.IsTrue(questions4[1].Id == 4);
            Assert.IsTrue(questions4[2].Id == 3);

            Assert.AreEqual(questions5.Length, 1);
            Assert.IsTrue(questions5[0].Id == 3);

            Assert.AreEqual(questions6.Length, 0);

            Assert.AreEqual(questions7.Length, 1);
            Assert.IsTrue(questions7[0].Id == 5);
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
