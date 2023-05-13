namespace Teuerungsportal.Localization;

public static class LocalizerSettings
{
    public const string NeutralCulture = "en-US";

    public static readonly string[] SupportedCultures = { NeutralCulture, "de-DE" };

    public static readonly(string, string)[] SupportedCulturesWithName = new[]
                                                                         {
                                                                             ("English", NeutralCulture),
                                                                             ("Deutsch", "de-DE"),
                                                                         };
}