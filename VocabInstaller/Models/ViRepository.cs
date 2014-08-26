using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VocabInstaller.Models {
    public class ViRepository : IViRepository{
        private ViDbContext context = new ViDbContext();

        public IQueryable<Question> Questions {
            get { return context.Questions; }
        }

        public void SaveQuestion(Question question) {
            question.Word = question.Word.Trim();
            question.Meaning = question.Meaning.Trim();

            if (question.Id == 0) {
                context.Questions.Add(question);
            } else {
                Question dbEntry = context.Questions.Find(question.Id);
                if (dbEntry != null) {
                    dbEntry.Word = question.Word;
                    dbEntry.Meaning = question.Meaning;
                    dbEntry.Note = question.Note;
                    dbEntry.CreatedAt = question.CreatedAt;
                    dbEntry.ReviewedAt = question.ReviewedAt;
                    dbEntry.ReviewLevel = question.ReviewLevel;
                }
            }
            context.SaveChanges();
        }

        public Question DeleteQuestion(int questionId) {
            Question dbEntry = context.Questions.Find(questionId);
            if (dbEntry != null) {
                context.Questions.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }

        public void Dispose() {
            context.Dispose();
        }
    }
}
