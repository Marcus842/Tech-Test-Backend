namespace TechTestBackend.Models
{
    public class SpotifyTracksResponseModel
    {
        public TracksModel Tracks { get; set; }
    }

    public class TracksModel
    {
        public Spotifysong[] Items { get; set; }
    }
}
