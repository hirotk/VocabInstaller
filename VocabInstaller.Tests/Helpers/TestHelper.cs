using System.Web;
using System.Web.Mvc;
using Moq;

namespace VocabInstaller.Tests.Helpers {
    public class TestHelper {
        public static void SetUser(Mock<ControllerContext> ctrlContext, int userId, string userRole = "User") {
            var httpContext = new Mock<HttpContextBase>();
            var session = new Mock<HttpSessionStateBase>();
            session.Setup(s => s["UserId"]).Returns(userId);
            session.Setup(s => s["UserRole"]).Returns(userRole);
            httpContext.Setup(hc => hc.Session).Returns(session.Object);
            ctrlContext.Setup(cc => cc.HttpContext).Returns(httpContext.Object);
//            ctrlContext.Setup(cc => cc.HttpContext.User.Identity.Name).Returns(userName);
        }
    }
}
