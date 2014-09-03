using System;
using System.Linq;

namespace VocabInstaller.Models {
    public interface IViRepository : IDisposable {
        IQueryable<Question> Questions { get; }

        void SaveQuestion(Question question);
        Question DeleteQuestion(int questionId);
    }
}
