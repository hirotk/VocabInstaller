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

            var questions = new Question[] {
                new Question {
                    Id = 1,
                    UserId = 2,
                    Word = "Why don't you try it?",
                    Meaning = "m1",
                    CreatedAt = DateTime.Parse("2014/01/01")
                },
                new Question {
                    Id = 2,
                    UserId = 2,
                    Word = "Even though it's difficult, it's worth trying.",
                    Meaning = "m2",
                    CreatedAt = DateTime.Parse("2014/01/02")
                },
                new Question {
                    Id = 3,
                    UserId = 2,
                    Word = "What a wonderful day!",
                    Meaning = "m3",
                    CreatedAt = DateTime.Parse("2014/01/03")
                },
                new Question {
                    Id = 4,
                    UserId = 2,
                    Word = "It's only 1.2$.",
                    Meaning = "m4",
                    CreatedAt = DateTime.Parse("2014/01/04")
                },
                new Question {
                    Id = 5,
                    UserId = 2,
                    Word = "2 * 3 + 6 / 2 = 9",
                    Meaning = "m5",
                    CreatedAt = DateTime.Parse("2014/01/05")
                },
                new Question {
                    Id = 6,
                    UserId = 3,
                    Word = "word1",
                    Meaning = "meaning1",
                    CreatedAt = DateTime.Parse("2014/01/06")
                },
                new Question {
                    Id = 7,
                    UserId = 3,
                    Word = "word2",
                    Meaning = "meaning2",
                    CreatedAt = DateTime.Parse("2014/01/07")
                }
            }.OrderByDescending(q => q.CreatedAt)
            .AsQueryable();

            mockRepository.Setup(m => m.Questions).Returns(questions);

            mockRepository.Setup(m => m.DeleteQuestion(It.IsAny<int>()))
                .Callback((int id) => mockRepository.Object.Questions.ToList().Remove(questions.ToList().Find(q => q.Id == id)))
                .Returns((int id) => questions.ToList().Find(q => q.Id == id));

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
        public void SaveTest() {
            // Arrange
            var controller = new FunctionController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Save() as ViewResult;

            var resultPost = controller.SaveConfirmed() as FileResult;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual(resultPost.ContentType, "text/csv");            
            Assert.AreEqual(resultPost.FileDownloadName.Substring(0, 6), "ViDat_");
        }

        [TestMethod]
        public void LoadTest() {
            // Arrange
            var controller = new FunctionController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            string filePath = Path.GetFullPath(@"../../Files\ViDat_20140905_010203.csv");
            var fileStream = new FileStream(filePath, FileMode.Open);

            uploadedFile
                .Setup(f => f.InputStream)
                .Returns(fileStream);

            // Act
            var resultGet = controller.Load() as ViewResult;
            var resultPost = controller.LoadConfirmed(
                csvFile:uploadedFile.Object, overwrite:false) as ViewResult;
            var model = resultPost.Model as List<Question>;
            model = model.OrderBy(m => m.CreatedAt).ToList();

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual(model.Count, 7 + 5);
            Assert.AreEqual(model.First().Word, "Why don't you try it?");
            Assert.AreEqual(model.Last().Word, "word4");
        }

        [TestMethod]
        public void InitializeTest() {
            // Arrange
            var controller = new FunctionController(mockRepository.Object);
            controller.ControllerContext = ctrlContext.Object;

            // Act
            var resultGet = controller.Initialize() as ViewResult;
            var resultPost = controller.InitializeConfirmed() as ViewResult;
            var model = resultPost.Model as List<Question>;
            var result = resultPost.ViewBag.Result as String;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual(result, "The database was initialized");
            Assert.AreEqual(model.Count(), 0);
        }
    }
}
