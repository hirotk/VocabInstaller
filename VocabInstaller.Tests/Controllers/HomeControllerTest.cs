﻿using System;
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
    public class HomeControllerTest {
        private Mock<ControllerContext> ctrlContext = new Mock<ControllerContext>();
        private Mock<IViRepository> mockRepository = new Mock<IViRepository>();        

        [TestInitialize]
        public void BeginTestMethod() {
            TestHelper.SetUser(ctrlContext, userId: 2);

            mockRepository.Setup(m => m.Cards).Returns(new Card[] {
                new Card {Id = 1, UserId = 2,
                    Question = "Why don't you try it?", Answer = "< a1 >",
                    CreatedAt = DateTime.Parse("2014/01/01")},
                new Card {Id = 2, UserId = 2,
                    Question = "Even though it's difficult, it's worth trying.", Answer = "a2*a2",
                    CreatedAt = DateTime.Parse("2014/01/02")},
                new Card {Id = 3, UserId = 2,
                    Question = "What a wonderful day!", Answer = "[a3%] beauty",
                    CreatedAt = DateTime.Parse("2014/01/03")},
                new Card {Id = 4, UserId = 2,
                    Question = "It's only 1.2$.", Answer = "(a4\") beautiful",
                    CreatedAt = DateTime.Parse("2014/01/04")},
                new Card {Id = 5, UserId = 2,
                    Question = "2 * 3 + 6 / 2 = 9", Answer = "a5 grateful",
                    CreatedAt = DateTime.Parse("2014/01/05")},
                new Card {Id = 6, UserId = 3,

                    Question = "q6", Answer = "a6",
                    CreatedAt = DateTime.Parse("2014/01/06")},
                new Card {Id = 7, UserId = 3,
                    Question = "q7", Answer = "a7",
                    CreatedAt = DateTime.Parse("2014/01/07")}
            }.OrderByDescending(c => c.CreatedAt)
            .AsQueryable());
        }

        [TestMethod]
        public void IndexTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var result = controller.Index(page:0, itemsPerPage:3) as ViewResult;
            var cards = ((HomeViewModel)result.Model).Cards.ToArray();
            var viewCards = ((HomeViewModel)result.Model).ViewCards.ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(cards.Length, 5);
            Assert.AreEqual(5, cards[0].Id);
            Assert.AreEqual(4, cards[1].Id);
            Assert.AreEqual(3, cards[2].Id);
            Assert.AreEqual(2, cards[3].Id);
            Assert.AreEqual(1, cards[4].Id);

            Assert.AreEqual(viewCards.Length, 3);
            Assert.AreEqual(5, cards[0].Id);
            Assert.AreEqual(4, cards[1].Id);
            Assert.AreEqual(3, cards[2].Id);
        }

        [TestMethod]
        public void CreateTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Create() as ViewResult;
            var cardCreateModel = resultGet.Model as CardCreateModel;
            cardCreateModel.Question = "q6";
            cardCreateModel.Answer = "a6";
            cardCreateModel.CreatedAt = DateTime.Parse("2014/01/06");
            var resultPost = controller.Create(cardCreateModel) as RedirectToRouteResult;
            mockRepository.Verify(m => m.SaveCard(It.IsAny<Card>()));
            
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
                var cardCreateModel = new CardCreateModel() { UserId = 2, Question = "w", Answer = "m" };
                var resultPost = controller.Create(cardCreateModel) as ActionResult;
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
            var cardEditModel = resultGet.Model as CardEditModel;
            var questionBefore = cardEditModel.Question;
            cardEditModel.Question = "Would you like to try it?";

            var resultPost = controller.Edit(cardEditModel) as ViewResult;
            mockRepository.Verify(m => m.SaveCard(It.IsAny<Card>()));

            var cardEditModelAfter = resultPost.Model as CardEditModel;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual("Why don't you try it?", questionBefore);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual("Would you like to try it?", cardEditModelAfter.Question);
        }

        [TestMethod]
        public void CanNotEditTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            TestHelper.SetUser(ctrlContext, userId: 3);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Edit(id:1) as ActionResult;
            var cardEditModel = new CardEditModel() { Id = 1, UserId = 2, Question = "", Answer = "" };
            Exception ex = null;
            try {
                var resultPost = controller.Edit(cardEditModel) as ActionResult;
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
            var card = resultGet.Model as Card;

            var resultPost = controller.DeleteConfirmed(card.Id) as RedirectToRouteResult;
            mockRepository.Verify(m => m.DeleteCard(card.Id));

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.AreEqual(1, card.Id);
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
            Card[] cardArray = viewModel.GetCardsInPage(page:1).ToArray();

            // Assert
            Assert.IsTrue(cardArray.Length == 2);
            Assert.AreEqual(cardArray[0].Id, 2);
            Assert.AreEqual(cardArray[1].Id, 1);
        }

        [TestMethod]
        public void CanFilterTest() {
            // Arrange
            var controller = new HomeController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Action
            var viewModel1 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage:10, search:"it? | It's")).Model);
            Card[] cards1 = viewModel1.Cards.ToArray();

            var viewModel2 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "try")).Model);
            Card[] cards2 = viewModel2.Cards.ToArray();

            var viewModel3 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"try\"")).Model);
            Card[] cards3 = viewModel3.Cards.ToArray();

            var viewModel4 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "! | $ | /")).Model);
            Card[] cards4 = viewModel4.Cards.ToArray();

            var viewModel5 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"won*ful\"")).Model);
            Card[] cards5 = viewModel5.Cards.ToArray();

            var viewModel6 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"a*ful\"")).Model);
            Card[] cards6 = viewModel6.Cards.ToArray();

            var viewModel7 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"\\*\"")).Model);
            Card[] cards7 = viewModel7.Cards.ToArray();

            var viewModel8 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"< a1 >\"")).Model);
            Card[] cards8 = viewModel8.Cards.ToArray();

            var viewModel9 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"*\\**\"")).Model);
            Card[] cards9 = viewModel9.Cards.ToArray();

            var viewModel10 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"[a3%]\"")).Model);
            Card[] cards10 = viewModel10.Cards.ToArray();

            var viewModel11 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"(a4\\\")\"")).Model);
            Card[] cards11 = viewModel11.Cards.ToArray();

            var viewModel12 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "[a3%]")).Model);
            Card[] cards12 = viewModel12.Cards.ToArray();

            var viewModel13 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "(a4\\\")")).Model);
            Card[] cards13 = viewModel13.Cards.ToArray();

            var viewModel14 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "\"wo*\" | \"be*\"")).Model);
            Card[] cards14 = viewModel14.Cards.ToArray();

            var viewModel15 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "(\"wo*\" | \"be*\") & \"*ful\"")).Model);
            Card[] cards15 = viewModel15.Cards.ToArray();

            var viewModel16 = (HomeViewModel)(((ViewResult)controller.Index(
                itemsPerPage: 10, search: "(\"wo*\" or \"be*\")  (\"*ful\" or try)")).Model);
            Card[] cards16 = viewModel16.Cards.ToArray();


            // Assert
            Assert.AreEqual(cards1.Length, 3);
            Assert.IsTrue(cards1[0].Id == 4);
            Assert.IsTrue(cards1[1].Id == 2);
            Assert.IsTrue(cards1[2].Id == 1);

            Assert.AreEqual(cards2.Length, 2);
            Assert.IsTrue(cards2[0].Id == 2);
            Assert.IsTrue(cards2[1].Id == 1);

            Assert.AreEqual(cards3.Length, 1);
            Assert.IsTrue(cards3[0].Id == 1);

            Assert.AreEqual(cards4.Length, 3);
            Assert.IsTrue(cards4[0].Id == 5);
            Assert.IsTrue(cards4[1].Id == 4);
            Assert.IsTrue(cards4[2].Id == 3);

            Assert.AreEqual(cards5.Length, 1);
            Assert.IsTrue(cards5[0].Id == 3);

            Assert.AreEqual(cards6.Length, 0);

            Assert.AreEqual(cards7.Length, 1);
            Assert.IsTrue(cards7[0].Id == 5);

            Assert.AreEqual(cards8.Length, 1);
            Assert.IsTrue(cards8[0].Id == 1);

            Assert.AreEqual(cards9.Length, 1);
            Assert.IsTrue(cards9[0].Id == 2);

            Assert.AreEqual(cards10.Length, 1);
            Assert.IsTrue(cards10[0].Id == 3);

            Assert.AreEqual(cards11.Length, 1);
            Assert.IsTrue(cards11[0].Id == 4);

            Assert.AreEqual(cards12.Length, 1);
            Assert.IsTrue(cards12[0].Id == 3);

            Assert.AreEqual(cards13.Length, 1);
            Assert.IsTrue(cards13[0].Id == 4);

            Assert.AreEqual(cards14.Length, 3);
            Assert.IsTrue(cards14[0].Id == 4);
            Assert.IsTrue(cards14[1].Id == 3);
            Assert.IsTrue(cards14[2].Id == 2);

            Assert.AreEqual(cards15.Length, 2);
            Assert.IsTrue(cards15[0].Id == 4);
            Assert.IsTrue(cards15[1].Id == 3);

            Assert.AreEqual(cards16.Length, 3);
            Assert.IsTrue(cards16[0].Id == 4);
            Assert.IsTrue(cards16[1].Id == 3);
            Assert.IsTrue(cards16[2].Id == 2);
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
    }
}
