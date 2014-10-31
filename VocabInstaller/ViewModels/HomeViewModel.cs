using System;
using System.Linq;
using System.Text.RegularExpressions;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public class HomeViewModel : AbstractViewModel {
        public HomeViewModel(int itemsPerPage, int pageSkip) 
            : base (itemsPerPage, pageSkip) {
        }

        public IQueryable<Card> ViewCards { get; set; }

        public string Search { get; set; }

        private IQueryable<Card> filter(
            IQueryable<Card> models, Func<Card, string[]> searchFields, string search) {

            if (!String.IsNullOrEmpty(search)) {
                search = search.Replace(@"""", "_qt_").Replace(@"\_qt_", @"""");

                var compMatchPtn = new Regex(
                    @"_qt_(?<cmKey>(?!_qt_)[\w\s\*\.\?'"",\!\+\-\\()\<\>\[\]=:/@#%\&\$~]+)_qt_");

                var matches = compMatchPtn.Matches(search);
                foreach (var m in matches) {
                    var key = m.ToString();
                    search = search.Replace(key, Regex.Replace(key, @"\s+", "_sp_"));
                    search = search.Replace("[", "[[").Replace("]", "]]").Replace("[[", "[[]").Replace("]]", "[]]")
                        .Replace("(", "[(]").Replace(")", "[)]");
                }

                string[] keywords = Regex.Split(search, @"\s+");
                for (int i = 0; i < keywords.Length; i++) {
                    keywords[i] = keywords[i].Replace("_sp_", " ");
                }

                var filteredlList = models.ToList();
                bool isOrSearch = false;

                foreach (var k in keywords) {
                    if (k == "|") {
                        isOrSearch = true;
                        continue;
                    }

                    var key = k;
                    bool isCompMatch = false;
                    if (compMatchPtn.IsMatch(key)) {
                        key = compMatchPtn.Match(key).Groups["cmKey"].Value;

                        key = Regex.Replace(key,
                            @"(?<symbol>[\.\?\!\+\-\&\$])", @"\${symbol}");
                        key = key.Replace(@"\*", "_star_");
                        key = key.Replace("*", @"[^\s]+");
                        key = key.Replace("_star_", @"\*");
                        isCompMatch = true;
                    }

                    var inputList = isOrSearch ? models.ToList() : filteredlList;

                    var outputList = isCompMatch
                            ? inputList.Where(c => {
                                foreach (var s in searchFields(c)) {
                                    if (s == null) { continue; }
                                    if (Regex.IsMatch(s, string.Format(
                                        @"(^|\s+){0}(\s+|$)", key))) {
                                        return true;
                                    }
                                }
                                return false;
                            }
                            ).ToList()
                            : inputList.Where(
                                    c => {
                                        foreach (var s in searchFields(c)) {
                                            if (s == null) { continue; }
                                            if (s.ToLower().Contains(k.ToLower())) {
                                                return true;
                                            }
                                        }
                                        return false;
                                    }
                            ).ToList();

                    if (isOrSearch) {
                        filteredlList.AddRange(outputList);
                    } else {
                        filteredlList = outputList;
                    }
                    isOrSearch = false;
                }
                return filteredlList.AsQueryable();
            }
            return models;
        }

        public IQueryable<Card> Filter(string search = null) {
            if (search != null) { Search = search; }
            Func<Card, string[]> searchFields = c =>
                new string[] { c.Question, c.Answer, c.Note };
            var cards = this.filter(Cards, searchFields, Search);
            this.ItemNum = cards.Count();

            if (this.Page > this.LastPage) { this.Page = this.LastPage; }

            return cards;
        }

    }
}
