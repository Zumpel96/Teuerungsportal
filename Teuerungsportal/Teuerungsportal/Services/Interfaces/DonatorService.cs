namespace Teuerungsportal.Services.Interfaces;

using Teuerungsportal.Models;

public interface DonatorService
{
    public Task<ICollection<Donator>> GetDonators();
}