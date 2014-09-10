using System.Linq;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public abstract class AbstractViewModel {
        protected AbstractViewModel(int itemsPerPage, int pageSkip) {
            this.ItemsPerPage = itemsPerPage;
            this.PageSkip = pageSkip;
            Page = 0;
        }

        public int ItemsPerPage { get; set; }
        public int PageSkip { get; set; }
        public int ItemNum { get; set; }

        private int _page;
        public int Page {
            get {
                return _page < 0 ? 0 : _page > this.LastPage ? this.LastPage : _page;
            }
            set { _page = value; }
        }

        public int LastPage {
            get {
                return (ItemNum > 0) ? (ItemNum - 1) / ItemsPerPage : 0;
            }
        }

        public bool HasPrevPage(int n) {
            return 0 <= Page - n;
        }

        public bool HasNextPage(int n) {
            return (Page + n) * ItemsPerPage < ItemNum;
        }

        private IQueryable<Card> cards;
        public IQueryable<Card> Cards {
            get { return cards; }
            set {
                cards = value;
                ItemNum = cards == null ? 0 : cards.Count();
            }
        }

        public IQueryable<Card> ViewCards { get; set; }

        public IQueryable<Card> GetCardsInPage(int? page = null) {
            int pg = page ?? Page;
            return Cards.Skip(pg * ItemsPerPage).Take(ItemsPerPage);
        }
    }
}
