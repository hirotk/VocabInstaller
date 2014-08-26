using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public class ReviewViewModel : AbstractViewModel {
        public ReviewViewModel(int itemsPerPage, int pageSkip)
            : base(itemsPerPage, pageSkip) { }
        
    }
}