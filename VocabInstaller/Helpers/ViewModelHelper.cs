using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VocabInstaller.Models;
using VocabInstaller.ViewModels;

namespace VocabInstaller.Helpers {
    public static class ViewModelHelper {
        public interface IExpression<T> {
            void Interpret(Stack<T> stack);
        }

        public class OrFilter : IExpression<List<Card>> {
            public void Interpret(Stack<List<Card>> stack) {
                var listA = stack.Pop();
                var listB = stack.Pop();
                listA.AddRange(listB);
                var listAorB = listA.Distinct().ToList();
                stack.Push(listAorB);
            }
        }

        public class AndFilter : IExpression<List<Card>> {
            public void Interpret(Stack<List<Card>> stack) {
                var listA = stack.Pop();
                var listB = stack.Pop();
                var listAandB = new List<Card>();
                foreach (var card in listA) {
                    if (listB.Contains(card)) {
                        listAandB.Add(card);
                    }
                }
                stack.Push(listAandB);
            }
        }

        public class UniFilter : IExpression<List<Card>> {
            public UniFilter(Stack<List<Card>> stack, string word, Func<Card, string[]> searchFields,
                List<Card> cardList, bool isCompMatch = false) {
                var list = isCompMatch
                    ? filterCardsByCompMatch(word, searchFields, cardList)
                    : filterCards(word, searchFields, cardList);

                stack.Push(list);
            }

            private List<Card> filterCards(string word, Func<Card, string[]> searchFields,
                List<Card> list) {
                return list.Where(c => {
                    foreach (var s in searchFields(c)) {
                        if (string.IsNullOrEmpty(s)) { continue; }
                        if (s.ToLower().Contains(word.ToLower())) {
                            return true;
                        }
                    }
                    return false;
                }).ToList();
            }

            private List<Card> filterCardsByCompMatch(string word, Func<Card, string[]> searchFields,
                List<Card> list) {
                return list.Where(c => {
                    foreach (var s in searchFields(c)) {
                        if (string.IsNullOrEmpty(s)) {
                            continue;
                        }
                        if (Regex.IsMatch(s, string.Format(
                            @"(^|\s+){0}(\s+|$)", word))) {
                            return true;
                        }
                    }
                    return false;
                }).ToList();
            }

            public void Interpret(Stack<List<Card>> stack) { }
        }

        public class ReversedPolishMachine {
            private SearchEngine se;

            public ReversedPolishMachine(Func<Card, string[]> searchFields,
                List<Card> populationList) {
                se = new SearchEngine(searchFields, populationList);
            }

            public string Convert(string expression) {
                expression = Regex.Replace(expression, @"(?<ope>[\(\)&|])", " ${ope} ");
                expression = Regex.Replace(expression, @"\s+or\s+", " | ");
                expression = Regex.Replace(expression, @"(?<key1>[^&|\(\s]+)\s+(?<key2>[^&|\)\s]+)", "${key1} & ${key2}");
                expression = Regex.Replace(expression, @"\s{2,}", " ");
                var words = expression.Trim().Split(' ');

                var sb = new StringBuilder();
                var stack = new Stack<string>();
                foreach (var w in words) {
                    switch (w) {
                        case "(":
                            stack.Push(w);
                            break;
                        case ")":
                            while (true) {
                                var ope = stack.Pop();
                                if (ope == "(" || stack.Count == 0) break;
                                sb.Append(ope).Append(' ');
                            }
                            break;
                        case "&":
                            stack.Push(w);
                            break;
                        case "|":
                            var top = stack.Any() ? stack.Peek() : string.Empty;
                            if (top == "&") {
                                sb.Append(stack.Pop()).Append(' ');
                            }
                            stack.Push(w);
                            break;
                        default:
                            sb.Append(w).Append(' ');
                            break;
                    }
                }

                while (stack.Count > 0) {
                    var ope = stack.Pop();
                    if (ope == "(" || ope == ")") {
                        throw new Exception("The input expression is wrong.");
                    }
                    sb.Append(ope).Append(' ');
                }

                return sb.ToString().Trim();
            }

            public void Excute(string code) {
                var talken = code.Split(' ');

                foreach (var t in talken) {
                    se.Process(t);
                }
            }

            public List<Card> GetResult() {
                return se.Stack.Pop();
            }
        }

        public class SearchEngine {
            private Func<Card, string[]> searchFields;
            private List<Card> populationList;

            public SearchEngine(Func<Card, string[]> searchFields, List<Card> populationList) {
                this.searchFields = searchFields;
                this.populationList = populationList;
            }

            private static readonly Regex compMatchPtn
                = new Regex(@"\0(?<cmKey>[^\0]+)\0");

            public void Process(string talken) {
                switch (talken) {
                    case "|":
                        orFilter.Interpret(stack);
                        break;
                    case "&":
                        andFilter.Interpret(stack);
                        break;
                    default:
                        bool isCompMatch = false;
                        if (compMatchPtn.IsMatch(talken)) {
                            // Remove quots for compmatch '\0'
                            talken = compMatchPtn.Match(talken).Groups["cmKey"].Value;
                            talken = talken.Replace("_sp_", " ")
                                .Replace("_opPts_", "[(]").Replace("_clPts_", "[)]");

                            // Escape sybols
                            talken = Regex.Replace(talken,
                                @"(?<symbol>[\.\?\!\+\-\&\$])", @"\${symbol}");

                            // Escape wildcard char *
                            talken = talken.Replace(@"\*", "_star_");
                            talken = talken.Replace("*", @"[^\s]+");
                            talken = talken.Replace("_star_", @"\*");
                            isCompMatch = true;
                        }

                        new UniFilter(stack, talken, searchFields, populationList, isCompMatch);
                        break;
                }
            }

            private Stack<List<Card>> stack = new Stack<List<Card>>();
            public Stack<List<Card>> Stack {
                get { return stack; }
            }

            private OrFilter orFilter = new OrFilter();
            private AndFilter andFilter = new AndFilter();
        }

        public static IQueryable<Card> FilterCards(this AbstractViewModel viewModel,
            IQueryable<Card> models, Func<Card, string[]> searchFields, string search) {

            if (String.IsNullOrEmpty(search)) { return models; }

            search = search.Replace(@"""", "\0") // begin and end quots for compMatch search
                .Replace("\\\0", @""""); // quots included in search words

            var compMatchPtn = new Regex(@"\0(?<cmKey>[^\0]+)\0");

            var matches = compMatchPtn.Matches(search);
            foreach (var m in matches) {
                var key = m.ToString();

                var escKey = Regex.Replace(key, @"\s+", "_sp_");
                escKey = escKey.Replace("[", "[[").Replace("]", "]]")
                    .Replace("[[", "[[]").Replace("]]", "[]]")
                    .Replace("(", "_opPts_").Replace(")", "_clPts_");
                search = search.Replace(key, escKey);
            }

            var rpm = new ReversedPolishMachine(searchFields, models.ToList());

            var code = rpm.Convert(search);
            rpm.Excute(code);

            return rpm.GetResult().AsQueryable();
        }
    }
}