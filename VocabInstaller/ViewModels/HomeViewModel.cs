using System;
using System.Linq;
using VocabInstaller.Helpers;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public class HomeViewModel : AbstractViewModel {
        public HomeViewModel(int itemsPerPage, int pageSkip) 
            : base (itemsPerPage, pageSkip) {
        }

        public IQueryable<Card> ViewCards { get; set; }

        public string Search { get; set; }

        public IQueryable<Card> Filter(string search = null) {
            if (search != null) { Search = search; }
            Func<Card, string[]> searchFields = c =>
                new string[] { c.Question, c.Answer, c.Note };

            var cards = this.FilterCards(searchFields, Search);
            
            this.ItemNum = cards.Count();

            if (this.Page > this.LastPage) { this.Page = this.LastPage; }

            return cards;
        }

    }
}
