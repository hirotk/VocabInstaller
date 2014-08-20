using System;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace VocabInstaller.Helpers {
    public static class ControllerHelper {
        public static int GetUserId(this Controller controller) {
            if (controller.User == null) {
                throw new Exception("User Account Error");
            }
            var userName = controller.User.Identity.Name;
            return WebSecurity.GetUserId(userName);
        }
    }
}
