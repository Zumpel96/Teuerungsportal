namespace Teuerungsportal.Models;

using Shared.DatabaseObjects;

public class Announcement
{
    public string ContentDe { get; set; } = string.Empty;

    public string ContentEn { get; set; } = string.Empty;

    public Announcement()
    {
    }

    public Announcement(AnnouncementDbo dbo)
    {
        this.ContentDe = dbo.ContentDe;
        this.ContentEn = dbo.ContentEn;
    }
}