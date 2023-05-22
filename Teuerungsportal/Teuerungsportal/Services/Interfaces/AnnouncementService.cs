namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface AnnouncementService
{
    public Task<Announcement?> GetAnnouncement();
}