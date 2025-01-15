using System;
using System.Linq;
using TechTestBackend.Models;

namespace TechTestBackend.Tests.Data
{
    public class DummyData
    {
        private Spotifysong[] _songs;

        public DummyData()
        {
            SetDummyData();
        }

        public Spotifysong[] GetSongs()=>_songs;

        public bool ListsAreEqual(Spotifysong[] songs)
        { 
            if(_songs.Count()!=songs.Count())
            {
                return false;
            }
            return !_songs.Where((s, i) =>
                        !(s.Id == songs[i].Id && s.Name == songs[i].Name)
                        ).Any();
        }
        private void SetDummyData()
        {
            _songs = new Spotifysong[4];
            _songs[0]=new Spotifysong()
            {
                Id = "5YCKObb1A7YIeOKzXhREwz",
                Name = "Pärlor"
            };
            _songs[1]=new Spotifysong()
            {
                Id = "5eJ314ozT4CTPlyjdsGq78",
                Name = "Utan dina andetag"
            };
            _songs[2]=new Spotifysong()
            {
                Id = "1yU51sZHtxvZHlni32lgxM",
                Name = "Dom andra"
            };
            _songs[3]=new Spotifysong()
            {
                Id = "6jvqpOz4CrGUIk7d5iaI7i",
                Name = "Kärleken väntar"
            };
        }
    }
}
