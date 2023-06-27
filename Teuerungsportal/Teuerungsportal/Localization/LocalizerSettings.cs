namespace Teuerungsportal.Localization;

public static class LocalizerSettings
{
    public const string NeutralCulture = "de-DE";

    public static readonly string[] SupportedCultures = { NeutralCulture, "en-US" };

    public static readonly(string, string)[] SupportedCulturesWithName = new[]
                                                                         {
                                                                             ("English", "en-US"),
                                                                             ("Deutsch", NeutralCulture),
                                                                         };
}