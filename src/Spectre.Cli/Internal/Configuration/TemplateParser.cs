using System.Collections.Generic;
using Spectre.Cli.Exceptions;

namespace Spectre.Cli.Internal
{
    internal static class TemplateParser
    {
        public sealed class ArgumentResult
        {
            public string Value { get; set; }
            public bool Required { get; set; }

            public ArgumentResult(string value, bool required)
            {
                Value = value;
                Required = required;
            }
        }

        public sealed class OptionResult
        {
            public List<string> LongNames { get; set; }
            public List<string> ShortNames { get; set; }
            public string? Value { get; set; }
            public bool ValueIsOptional { get; set; }

            public OptionResult()
            {
                ShortNames = new List<string>();
                LongNames = new List<string>();
            }
        }

        public static ArgumentResult ParseArgumentTemplate(string template)
        {
            var valueName = default(string);
            var required = false;
            foreach (var token in TemplateTokenizer.Tokenize(template))
            {
                if (token.TokenKind == TemplateToken.Kind.ShortName ||
                    token.TokenKind == TemplateToken.Kind.LongName)
                {
                    throw TemplateException.ArgumentCannotContainOptions(template, token);
                }

                if (token.TokenKind == TemplateToken.Kind.OptionalValue ||
                    token.TokenKind == TemplateToken.Kind.RequiredValue)
                {
                    if (!string.IsNullOrWhiteSpace(valueName))
                    {
                        throw TemplateException.MultipleValuesAreNotSupported(template, token);
                    }

                    if (string.IsNullOrWhiteSpace(token.Value))
                    {
                        throw TemplateException.ValuesMustHaveName(template, token);
                    }

                    valueName = token.Value;
                    required = token.TokenKind == TemplateToken.Kind.RequiredValue;
                }
            }

            if (valueName == null)
            {
                throw TemplateException.ArgumentsMustHaveValueName(template);
            }

            return new ArgumentResult(valueName, required);
        }

        public static OptionResult ParseOptionTemplate(string template)
        {
            var result = new OptionResult();

            foreach (var token in TemplateTokenizer.Tokenize(template))
            {
                if (token.TokenKind == TemplateToken.Kind.LongName || token.TokenKind == TemplateToken.Kind.ShortName)
                {
                    if (string.IsNullOrWhiteSpace(token.Value))
                    {
                        throw TemplateException.OptionsMustHaveName(template, token);
                    }

                    if (char.IsDigit(token.Value[0]))
                    {
                        throw TemplateException.OptionNamesCannotStartWithDigit(template, token);
                    }

                    foreach (var character in token.Value)
                    {
                        if (!char.IsLetterOrDigit(character) && character != '-' && character != '_')
                        {
                            throw TemplateException.InvalidCharacterInOptionName(template, token, character);
                        }
                    }
                }

                if (token.TokenKind == TemplateToken.Kind.LongName)
                {
                    if (token.Value.Length == 1)
                    {
                        throw TemplateException.LongOptionMustHaveMoreThanOneCharacter(template, token);
                    }

                    result.LongNames.Add(token.Value);
                }

                if (token.TokenKind == TemplateToken.Kind.ShortName)
                {
                    if (token.Value.Length > 1)
                    {
                        throw TemplateException.ShortOptionMustOnlyBeOneCharacter(template, token);
                    }

                    result.ShortNames.Add(token.Value);
                }

                if (token.TokenKind == TemplateToken.Kind.RequiredValue ||
                    token.TokenKind == TemplateToken.Kind.OptionalValue)
                {
                    if (!string.IsNullOrWhiteSpace(result.Value))
                    {
                        throw TemplateException.MultipleOptionValuesAreNotSupported(template, token);
                    }

                    foreach (var character in token.Value)
                    {
                        if (!char.IsLetterOrDigit(character) &&
                            character != '=' && character != '-' && character != '_')
                        {
                            throw TemplateException.InvalidCharacterInValueName(template, token, character);
                        }
                    }

                    result.Value = token.Value.ToUpperInvariant();
                    result.ValueIsOptional = token.TokenKind == TemplateToken.Kind.OptionalValue;
                }
            }

            if (result.LongNames.Count == 0 &&
                result.ShortNames.Count == 0)
            {
                throw TemplateException.MissingLongAndShortName(template, null);
            }

            return result;
        }
    }
}
