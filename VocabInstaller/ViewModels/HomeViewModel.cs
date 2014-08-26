using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using VocabInstaller.Models;

namespace VocabInstaller.ViewModels {
    public class HomeViewModel : AbstractViewModel {
        public HomeViewModel(int itemsPerPage, int pageSkip) 
            : base (itemsPerPage, pageSkip) {
        }

        public string Search { get; set; }

        private IQueryable<Question> filter(
            IQueryable<Question> models, Func<Question, string[]> searchFields, string search) {

            if (!String.IsNullOrEmpty(search)) {
                search = HttpUtility.HtmlEncode(search)
                    .Replace("&#39;", "'").Replace("&amp;", "&");

                var compMatchPtn = new Regex(
                    @"&quot;(?<cmKey>(?!&quot;)[\w\s\*\.\?',\!\+\-=:/@#%\&\$~]+)&quot;");

                var matches = compMatchPtn.Matches(search);
                foreach (var m in matches) {
                    var key = m.ToString();
                    search = search.Replace(key, Regex.Replace(key, @"\s+", "[sp]"));
                }

                string[] keywords = Regex.Split(search, @"\s+");
                for (int i = 0; i < keywords.Length; i++) {
                    keywords[i] = keywords[i].Replace("[sp]", " ");
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
                        key = key.Replace(@"**", "[star]");
                        key = key.Replace("*", @"[^\s]+");
                        key = key.Replace("[star]", @"\*");
                        isCompMatch = true;
                    }

                    var inputList = isOrSearch ? models.ToList() : filteredlList;

                    var outputList = isCompMatch
                            ? inputList.Where(q => {
                                foreach (var s in searchFields(q)) {
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
                                    q => {
                                        foreach (var s in searchFields(q)) {
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

        public IQueryable<Question> Filter(string search = null) {
            if (search != null) { Search = search; }
            Func<Question, string[]> searchFields = q =>
                new string[] { q.Word, q.Meaning, q.Note };
            var questions = this.filter(Questions, searchFields, Search);
            this.ItemNum = questions.Count();

            if (this.Page > this.LastPage) { this.Page = this.LastPage; }

            return questions;
        }

    }
}
