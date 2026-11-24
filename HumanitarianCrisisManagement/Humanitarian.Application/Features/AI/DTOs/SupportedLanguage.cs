using System.ComponentModel.DataAnnotations;

namespace Humanitarian.Application.Features.AI.DTOs;

public static class SupportedLanguages
{
    public const string English = "English";
    public const string Sinhala = "Sinhala";
    public const string Tamil = "Tamil";

    public static string Normalize(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return English;
        }

        return language.Trim().ToLowerInvariant() switch
        {
            "english" or "en" => English,
            "sinhala" or "si" => Sinhala,
            "tamil" or "ta" => Tamil,
            _ => language.Trim()
        };
    }

    public static bool IsSupported(string? language) =>
        string.IsNullOrWhiteSpace(language) ||
        Normalize(language) is English or Sinhala or Tamil;
}

public sealed class SupportedLanguageAttribute : ValidationAttribute
{
    public SupportedLanguageAttribute()
    {
        ErrorMessage = "Language must be English, Sinhala, Tamil, en, si, or ta.";
    }

    public override bool IsValid(object? value) =>
        value is null || SupportedLanguages.IsSupported(value.ToString());
}
