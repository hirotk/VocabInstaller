using System.Web.Mvc;
using VocabInstaller.Models;

namespace VocabInstaller.Controllers {
    [Authorize(Roles = "Administrator")]
    public class FunctionController : Controller {
        private IViRepository repository;

        // Default Constructor
        public FunctionController() : this(new ViRepository()) { }

        public FunctionController(IViRepository repository) {
            this.repository = repository;
        }

        //
        // GET: /Function/
        public ActionResult Index() {
            return View();
        }

        //
        // GET: /Function/Save
        public ActionResult Save() {
            return View();
        }

        //
        // POST: /Function/Save
        [HttpPost, ActionName("Save")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveConfirmed() {
            return null;
        }

    }
}
