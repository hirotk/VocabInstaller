using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VocabInstaller.Models;
using VocabInstaller.ViewModels;

namespace VocabInstaller.Helpers {
    public static class ViewModelHelper {
        private interface IExpression<T> {
            void Interpret(Stack<T> stack);
        }

        private class OrFilter : IExpression<List<Card>> {
            public void Interpret(Stack<List<Card>> stack) {
                var listA = stack.Pop();
                var listB = stack.Pop();
                listA.AddRange(listB);
                var listAorB = listA.Distinct().ToList();
                stack.Push(listAorB);
            }
        }

        private class AndFilter : IExpression<List<Card>> {
            public void Interpret(Stack<List<Card>> stack) {
                var listA = stack.Pop();
                var listB = stack.Pop();
                var listAandB = listA.Where(listB.Contains).ToList();
                stack.Push(listAandB);
            }
        }

        private class ReversedPolishMachine {
            private SearchEngine se;

            public ReversedPolishMachine(Func<Card, string[]> searchFields,
                List<Card> populationList) {
                se = new SearchEngine(searchFields, populationList);
            }

            private static string rpnEscape(string expression) {
                expression = expression.Replace(@"""", "\0") // begin and end quots for compMatch search
                    .Replace("\\\0", @""""); // quots included in search words

                var compMatchPtn = new Regex(@"\0(?<cmKey>[^\0]+)\0");
                var matches = compMatchPtn.Matches(expression);
                foreach (var m in matches) {
                    var key = m.ToString();

                    var escKey = Regex.Replace(key, @"\s+", "_sp_")
                        .Replace("(", "_opPts_").Replace(")", "_clPts_");

                    expression = expression.Replace(key, escKey);
                }
                return expression;
            }

            private static string rpnUnescape(string expression) {
                expression = expression.Replace("_sp_", " ")
                    .Replace("_opPts_", "(").Replace("_clPts_", ")");
                return expression;
            }

            private static string rpnConvert(string expression) {
                expression = Regex.Replace(expression, @"(?<ope>[\(\)])", " ${ope} ");
                expression = Regex.Replace(expression, @"\s+or\s+", " | ");
                expression = Regex.Replace(expression, @"(?<key1>[^\(\s&|]+)\s+(?<key2>[^\)\s&|]+)", "${key1} & ${key2}");
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

            public void Search(string search) {
                search = rpnEscape(search);
                var code = rpnConvert(search);
                Execute(code);
            }

            public void Execute(string code) {
                var talken = code.Split(' ');

                foreach (var t in talken) {
                    var tkn = rpnUnescape(t);
                    se.Process(tkn);
                }
            }

            public List<Card> GetResult() {
                return se.Stack.Pop();
            }
        }

        private class SearchEngine {
            private readonly Func<Card, string[]> searchFields;
            private readonly List<Card> populationList;

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
                        uniFilter(stack, talken, populationList, searchFields);
                        break;
                }
            }

            private Stack<List<Card>> stack = new Stack<List<Card>>();
            public Stack<List<Card>> Stack {
                get { return stack; }
            }

            private static readonly OrFilter orFilter = new OrFilter();
            private static readonly AndFilter andFilter = new AndFilter();

            private static void uniFilter(Stack<List<Card>> stack, string word,
                List<Card> cardList, Func<Card, string[]> searchFields) {

                bool isCompMatch = false;
                if (compMatchPtn.IsMatch(word)) {
                    // Remove quots for compmatch '\0'
                    word = compMatchPtn.Match(word).Groups["cmKey"].Value;

                    // Escape star
                    word = word.Replace(@"\*", "_star_");
                    
                    // Escape sybols
                    word = Regex.Replace(word,
                        @"(?<symbol>[\.\?\!\+\-\^\$\(\)\[\]\\])", @"\${symbol}");

                    // Convert wildcard
                    word = word.Replace("*", @"[^\s]+");
                    
                    // Unescape star
                    word = word.Replace("_star_", @"\*");
                    
                    isCompMatch = true;
                }

                var list = filterCards(word, cardList, searchFields, isCompMatch);

                stack.Push(list);
            }

            private static List<Card> filterCards(string word, 
                List<Card> cardList, Func<Card, string[]> searchFields, bool isCompMatch = false) {
                return cardList.Where(c => {
                    foreach (var s in searchFields(c)) {
                        if (string.IsNullOrEmpty(s)) { continue; }

                        if (isCompMatch) {
                            if (Regex.IsMatch(s, string.Format(
                                @"(^|\s+){0}(\s+|$)", word))) {
                                return true;
                            }
                        } else {
                            if (s.ToLower().Contains(word.ToLower())) {
                                return true;
                            }
                        }
                    }
                    return false;
                }).ToList();
            }
        }

        public static IQueryable<Card> FilterCards(this AbstractViewModel viewModel,
            Func<Card, string[]> searchFields, string search) {

            var cards = viewModel.Cards;

            if (String.IsNullOrEmpty(search)) { return cards; }

            var rpm = new ReversedPolishMachine(searchFields, cards.ToList());

            rpm.Search(search);

            return rpm.GetResult().AsQueryable();
        }
    }
}
