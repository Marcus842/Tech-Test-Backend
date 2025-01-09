using TechTestBackend.Models;

namespace TechTestBackend.Services
{
    public interface ISpotifyHttpService
    {
        Soptifysong[] GetTracks(string name);
        Soptifysong GetTrack(string id);
    }
}
