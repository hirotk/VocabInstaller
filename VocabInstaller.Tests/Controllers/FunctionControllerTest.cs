using System.Net.Mime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Web.Mvc;
using VocabInstaller.Controllers;
using VocabInstaller.Models;
using VocabInstaller.Tests.Helpers;

namespace VocabInstaller.Tests.Controllers {
    [TestClass]
    public class FunctionControllerTest {
        private Mock<ControllerContext> ctrlContext = new Mock<ControllerContext>();
        private Mock<IViRepository> mockRepository = new Mock<IViRepository>();

        [TestInitialize]
        public void BeginTestMethod() {
            TestHelper.SetUser(ctrlContext, userId: 1, userRole: "Administrator");

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
            var resultGet = controller.Index() as ViewResult;

            var resultPost = controller.SaveConfirmed() as FileResult;

            // Assert
            Assert.IsNotNull(resultGet);
            Assert.IsNotNull(resultPost);
            Assert.AreEqual(resultPost.ContentType, "text/csv");            
            Assert.AreEqual(resultPost.FileDownloadName.Substring(0, 6), "ViDat_");
        }

    }
}
