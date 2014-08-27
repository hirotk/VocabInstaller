using System.Web;
using System.Web.Mvc;
using Moq;

namespace VocabInstaller.Tests.Helpers {
    public class TestHelper {
        public static void SetUserId(Mock<ControllerContext> ctrlContext, int userId) {
            var httpContext = new Mock<HttpContextBase>();
            var session = new Mock<HttpSessionStateBase>();
            session.Setup(s => s["UserId"]).Returns(userId);
            httpContext.Setup(hc => hc.Session).Returns(session.Object);
            ctrlContext.Setup(cc => cc.HttpContext).Returns(httpContext.Object);
        }
    }
}
