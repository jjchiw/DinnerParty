using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Validation.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using Nancy.Validation;

namespace DinnerParty.Models.CustomAnnotations
{
    public class CustomDataAdapter : IDataAnnotationsValidatorAdapter
    {
        protected readonly ValidationAttribute attribute;

        public bool CanHandle(ValidationAttribute attribute)
        {
            return attribute.GetType() == typeof(MatchAttribute);
        }

        public IEnumerable<ModelValidationRule> GetRules(ValidationAttribute attribute, System.ComponentModel.PropertyDescriptor descriptor)
        {
            yield return new ModelValidationRule("custom", attribute.FormatErrorMessage,
                new[] { ((MatchAttribute)attribute).SourceProperty });
        }

        public IEnumerable<ModelValidationError> Validate(object instance, ValidationAttribute attribute, System.ComponentModel.PropertyDescriptor descriptor)
        {
            var context =
            new ValidationContext(instance, null, null)
            {
                MemberName = ((MatchAttribute)attribute).SourceProperty
            };

            var result = attribute.GetValidationResult(instance, context);

            if (result != null)
            {
                yield return new ModelValidationError(result.MemberNames, result.ErrorMessage);
            }

            yield break;

        }
    }
}