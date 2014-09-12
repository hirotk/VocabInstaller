using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using VocabInstaller.Models;

namespace VocabInstaller.Validations {
    public class Unique : ValidationAttribute {
        ViRepository repository = new ViRepository();

        public Unique(string propertyName, bool userEach = false, bool editMode = false)
            : base() {
            PropertyName = propertyName;
            UserEach = userEach;
            EditMode = editMode;            
        }

        public string PropertyName { get; set; }
        public bool UserEach { get; set; }
        public bool EditMode { get; set; }

        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext) {
            if (value == null) {
                return new ValidationResult("The object value argument should not be null");
            }
            string property = value.ToString().Trim();

            IEnumerable<Card> cards = repository.Cards;

            var container = validationContext.ObjectInstance.GetType();
            if (UserEach) {
                var field = container.GetProperty("UserId");
                int userId = (int)field.GetValue(validationContext.ObjectInstance, index:null);
                cards = cards.Where(c => c.UserId == userId);
            }

            int id = -1;
            if (EditMode) {
                var field = container.GetProperty("Id");
                id = (int)field.GetValue(validationContext.ObjectInstance, index:null);
            }

            foreach (var c in cards) {
                string s = typeof(Card)
                    .InvokeMember(PropertyName, BindingFlags.GetProperty, 
                        binder:null, target:c, args:null).ToString();
                if (s == property) {
                    if (EditMode && c.Id == id) { continue; }
                    return new ValidationResult(errorMessage:null);
                }
            }

            return ValidationResult.Success;            
        }
    }
}
