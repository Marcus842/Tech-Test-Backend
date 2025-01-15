using System.Linq;
using TechTestBackend.Models;
using TechTestBackend.Services;
using TechTestBackend.Tests.Data;

namespace TechTestBackend.Tests
{
    internal class SpotifyHttpMockService : ISpotifyHttpService
    {
        private Spotifysong[] songs;

        public SpotifyHttpMockService()
        {
            var dummyData = new DummyData();
            songs=dummyData.GetSongs();
        }

        public Spotifysong GetTrack(string id)
        {
            if(id== "5YCKObb1A7YIeOKzXhREwz")
                return new Spotifysong();

            var track=songs.Where(s=>s.Id==id).FirstOrDefault();
            return track;
        }

        public Spotifysong[] GetTracks(string name)
        {
            return songs.ToArray();
        }
    }
}
