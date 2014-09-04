using System;
using System.Web.Mvc;
using System.Web.Security;
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

        public static string[] GetUserRoles(this Controller controller) {
            if (controller.User == null) {
                throw new Exception("User Account Error");
            }
            var userName = controller.User.Identity.Name;
            return Roles.GetRolesForUser(userName);
        }

        public static string GetUserRole(this Controller controller) {
            return GetUserRoles(controller)[0];
        }
    }
}
