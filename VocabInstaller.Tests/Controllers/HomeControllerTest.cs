using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VocabInstaller;
using VocabInstaller.Controllers;

namespace VocabInstaller.Tests.Controllers {
    [TestClass]
    public class HomeControllerTest {
        [TestMethod]
        public void IndexTest() {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
//            Assert.AreEqual("Modify this template to jump-start your ASP.NET MVC application.", result.ViewBag.Message);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateTest() {
            // Arrange
            var controller = new HomeController();

            // Act
            ViewResult result = controller.Create() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EditTest() {
            // Arrange
            var controller = new HomeController();

            // Act
            ViewResult result = controller.Edit(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteTest() {
            // Arrange
            var controller = new HomeController();

            // Act
            ViewResult result = controller.Delete(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
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
