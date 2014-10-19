using System.Collections.Generic;
using System.Data;
using System.Net.Mime;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Web.Mvc;
using VocabInstaller.Controllers;
using VocabInstaller.Models;
using VocabInstaller.Tests.Helpers;
using System.IO;

namespace VocabInstaller.Tests.Controllers {
    [TestClass]
    public class FunctionControllerTest {
        private Mock<ControllerContext> ctrlContext = new Mock<ControllerContext>();
        private Mock<IViRepository> mockRepository = new Mock<IViRepository>();
        private Mock<HttpPostedFileBase> uploadedFile = new Mock<HttpPostedFileBase>();

        [TestInitialize]
        public void BeginTestMethod() {
            TestHelper.SetUser(ctrlContext, userId: 1, userRole: "Administrator");

            var cards = new Card[] {
                new Card {
                    Id = 1,
                    UserId = 2,
                    Question = "Why don't you try it?",
                    Answer = "a1",
                    CreatedAt = DateTime.Parse("2014/01/01")
                },
                new Card {
                    Id = 2,
                    UserId = 2,
                    Question = "Even though it's difficult, it's worth trying.",
                    Answer = "a2",
                    CreatedAt = DateTime.Parse("2014/01/02")
                },
                new Card {
                    Id = 3,
                    UserId = 2,
                    Question = "What a wonderful day!",
                    Answer = "a3",
                    CreatedAt = DateTime.Parse("2014/01/03")
                },
                new Card {
                    Id = 4,
                    UserId = 2,
                    Question = "It's only 1.2$.",
                    Answer = "a4",
                    CreatedAt = DateTime.Parse("2014/01/04")
                },
                new Card {
                    Id = 5,
                    UserId = 2,
                    Question = "2 * 3 + 6 / 2 = 9",
                    Answer = "a5",
                    CreatedAt = DateTime.Parse("2014/01/05")
                },
                new Card {
                    Id = 6,
                    UserId = 3,
                    Question = "question1",
                    Answer = "answer1",
                    CreatedAt = DateTime.Parse("2014/01/06")
                },
                new Card {
                    Id = 7,
                    UserId = 3,
                    Question = "question2",
                    Answer = "answer2",
                    CreatedAt = DateTime.Parse("2014/01/07")
                }
            }.OrderByDescending(c => c.CreatedAt)
            .AsQueryable();

            mockRepository.Setup(m => m.Cards).Returns(cards);

            mockRepository.Setup(m => m.DeleteCard(It.IsAny<int>()))
                .Callback((int id) => mockRepository.Object.Cards.ToList().Remove(cards.ToList().Find(c => c.Id == id)))
                .Returns((int id) => cards.ToList().Find(c => c.Id == id));

        }

        [TestMethod]
        public void IndexTest() {
            // Arrange
            var controller = new FunctionController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ExportTest() {
            // Arrange
            var controller = new FunctionController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Export() as ViewResult;

            var resultPost = controller.ExportConfirmed() as FileResult;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual(resultPost.ContentType, "text/csv");            
            Assert.AreEqual(resultPost.FileDownloadName.Substring(0, 6), "ViDat_");
        }

        [TestMethod]
        public void ImportTest() {
            // Arrange
            var controller = new FunctionController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            string filePath = Path.GetFullPath(@"../../Files\ViDat_20140910_010203.csv");
            var fileStream = new FileStream(filePath, FileMode.Open);

            uploadedFile
                .Setup(f => f.InputStream)
                .Returns(fileStream);

            // Act
            var resultGet = controller.Import() as ViewResult;
            var resultPost = controller.ImportConfirmed(
                csvFile:uploadedFile.Object, overwrite:false) as ViewResult;
            var model = resultPost.Model as List<Card>;
            model = model.OrderBy(m => m.CreatedAt).ToList();

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual(model.Count, 7 + 5);
            Assert.AreEqual(model.First().Question, "Why don't you try it?");
            Assert.AreEqual(model.Last().Question, "question4");
        }

        [TestMethod]
        public void InitializeTest() {
            // Arrange
            var controller = new FunctionController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Initialize() as ViewResult;
            var resultPost = controller.InitializeConfirmed() as ViewResult;
            var model = resultPost.Model as List<Card>;
            var result = resultPost.ViewBag.Result as String;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual(result, "The database was initialized");
            Assert.AreEqual(model.Count(), mockRepository.Object.Cards.Count());
        }
    }
}
