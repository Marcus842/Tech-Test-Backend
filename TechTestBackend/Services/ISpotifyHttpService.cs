using TechTestBackend.Models;

namespace TechTestBackend.Services
{
    public interface ISpotifyHttpService
    {
        Spotifysong[] GetTracks(string name);
        Spotifysong GetTrack(string id);
    }
}
