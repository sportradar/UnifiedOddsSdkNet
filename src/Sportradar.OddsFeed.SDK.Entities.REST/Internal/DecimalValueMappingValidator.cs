/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IMappingValidator"/> which validates the decimal part of the provided value against a predefined value
    /// </summary>
    /// <seealso cref="IMappingValidator" />
    public class DecimalValueMappingValidator : IMappingValidator
    {
        /// <summary>
        /// The name of the specifier in the valid for attribute
        /// </summary>
        private readonly string _specifierName;

        /// <summary>
        /// A value specifying the allowed value of the decimal part
        /// </summary>
        private readonly decimal _allowedDecimalValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificValueMappingValidator"/> class.
        /// </summary>
        /// <param name="specifierName">The name of the specifier in the valid for attribute.</param>
        /// <param name="allowedDecimalValue">A value specifying the allowed value of the decimal part.</param>
        public DecimalValueMappingValidator(string specifierName, decimal allowedDecimalValue)
        {
            if (string.IsNullOrEmpty(specifierName))
            {
                throw new ArgumentNullException(nameof(specifierName));
            }

            _specifierName = specifierName;
            _allowedDecimalValue = allowedDecimalValue;
        }

        /// <summary>
        /// Validate the provided specifiers against current instance.
        /// </summary>
        /// <param name="specifiers">The <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing market specifiers.</param>
        /// <returns>True if the mapping associated with the current instance can be used to map associated market; False otherwise</returns>
        /// <exception cref="InvalidOperationException">Validation cannot be performed against the provided specifiers</exception>
        public bool Validate(IReadOnlyDictionary<string, string> specifiers)
        {
            if (!specifiers.TryGetValue(_specifierName, out var specifierValue))
            {
                throw new InvalidOperationException($"Required specifier[{_specifierName}] does not exist in the provided specifiers");
            }

            if (!decimal.TryParse(specifierValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidOperationException($"The value of the specifier {_specifierName}={specifierValue} is not a string representation of a decimal value");
            }

            var roundedValue = Math.Floor(value);
            return value - roundedValue == _allowedDecimalValue;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{_specifierName}~{_allowedDecimalValue}";
        }
    }
}
