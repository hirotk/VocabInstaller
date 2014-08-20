using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocabInstaller.Models {
    public interface IViRepository : IDisposable {
        IQueryable<Question> Questions { get; }

        void SaveQuestion(Question question);
        Question DeleteQuestion(int questionId);
    }
}
