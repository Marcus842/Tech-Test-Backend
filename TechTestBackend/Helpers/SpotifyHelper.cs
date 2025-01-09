using TechTestBackend.Models;
using TechTestBackend.Services;

namespace TechTestBackend.Helpers;

public static class SpotifyHelper
{
    public static ISpotifyHttpService SpotifyService { get; internal set; }

    public static Soptifysong[] GetTracks(string name)
    {
        var songs= SpotifyService.GetTracks(name);
        return songs;
    }

    public static Soptifysong GetTrack(string id)
    {
        var song = SpotifyService.GetTrack(id);

        return song;
    }
}