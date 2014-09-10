using System.Linq;

namespace VocabInstaller.Models {
    public class ViRepository : IViRepository{
        private ViDbContext context = new ViDbContext();

//        public IQueryable<UserProfile> UserProfiles {
//            get { return context.UserProfiles; }
//        }

        public IQueryable<Card> Cards {
            get { return context.Cards; }
        }

        public void SaveCard(Card card) {
            card.Question = card.Question.Trim();
            card.Answer = card.Answer.Trim();

            if (card.Id == 0) {
                context.Cards.Add(card);
            } else {
                Card dbEntry = context.Cards.Find(card.Id);
                if (dbEntry != null) {
                    dbEntry.Question = card.Question;
                    dbEntry.Answer = card.Answer;
                    dbEntry.Note = card.Note;
                    dbEntry.CreatedAt = card.CreatedAt;
                    dbEntry.ReviewedAt = card.ReviewedAt;
                    dbEntry.ReviewLevel = card.ReviewLevel;
                }
            }
            context.SaveChanges();
        }

        public Card DeleteCard(int cardId) {
            Card dbEntry = context.Cards.Find(cardId);
            if (dbEntry != null) {
                context.Cards.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }

        public void Dispose() {
            context.Dispose();
        }
    }
}
