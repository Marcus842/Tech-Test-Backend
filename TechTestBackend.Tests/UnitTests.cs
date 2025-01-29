using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TechTestBackend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Logging;
using TechTestBackend.Controllers;
using Microsoft.AspNetCore.Mvc;
using TechTestBackend.Models;
using TechTestBackend.Tests.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TechTestBackend.Tests.Services;

namespace TechTestBackend.Tests
{
    [TestClass]
    public class UnitTests
    {
        private readonly SpotifyController _spotifyController;
        private ILogger<SpotifyController> _spotifyControllerLogger;
        private SongstorageContext _songStorageContext;

        public UnitTests()
        {
            InitateServicesForTests();

            _spotifyController = new SpotifyController(_spotifyControllerLogger, new SpotifyHttpMockService(), _songStorageContext);
        }

        private void InitateServicesForTests()
        {
            var baseConfiguration = new ConfigurationBuilder()
                                        .AddUserSecrets<SpotifyHttpService>()
                                        .Build();

            var inMemorySettings = new Dictionary<string, string>{{ "Spotify:BaseUrl", "https://api.spotify.com/v1/" }};

            var finalConfiguration = new ConfigurationBuilder()
                .AddConfiguration(baseConfiguration)
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var services = new ServiceCollection();

            services.AddDbContextFactory<SongstorageContext>(options =>
                options.UseInMemoryDatabase("Songstorage"));

            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();

            var storage = serviceProvider.GetRequiredService<IDbContextFactory<SongstorageContext>>();

            _songStorageContext = storage.CreateDbContext();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            _spotifyControllerLogger = factory.CreateLogger<SpotifyController>();
        }

        [TestMethod]
        public void TestSearchTracks()
        {
            var dummydata = new DummyData();

            var actionResult = _spotifyController.SearchTracks("kent");
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var songs = okResult.Value as Spotifysong[];
            Assert.IsNotNull(songs);

            Assert.IsTrue(dummydata.ListsAreEqual(songs));
        }

        [TestMethod]
        public void LikeAndListLikedSongs()
        {
            LikeSongThatDoesNotExist();
            LikeSongThatExist();
            var songs = ListLikedSongs();

            Assert.IsTrue(songs.Count() == 1);

            var song = songs[0];
            var dummydata = new DummyData();

            var dummyDataSongs = dummydata.GetSongs();
            var expectedSong = dummyDataSongs[1];
            Assert.IsTrue(song.Id == expectedSong.Id && song.Name == expectedSong.Name);
        }

        [TestMethod]
        public void RemoveLikedSongThatDoesNotExistInSpotify()
        {
            var actionResult = _spotifyController.RemoveLike("5YCKObb1A7YIeOKzXhREwz");
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);

            Assert.IsTrue(statusCodeResult.StatusCode == 400);
        }

        [TestMethod]
        public void RemoveLikedAndListSongs()
        {
            var actionResult = _spotifyController.RemoveLike("5eJ314ozT4CTPlyjdsGq78");
            var okResult = actionResult as OkResult;
            Assert.IsNotNull(okResult);

            var songs = ListLikedSongs();

            Assert.IsTrue(songs.Count() == 0);
        }

        private void LikeSongThatDoesNotExist()
        {
            var actionResult = _spotifyController.Like("5YCKObb1A7YIeOKzXhREwz");
            var statusCodeResult = actionResult as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);

            Assert.IsTrue(statusCodeResult.StatusCode == 400);
        }

        private void LikeSongThatExist()
        {
            var actionResult = _spotifyController.Like("5eJ314ozT4CTPlyjdsGq78");
            var okResult = actionResult as OkResult;
            Assert.IsNotNull(okResult);
        }

        private List<Spotifysong> ListLikedSongs()
        {
            var actionResult = _spotifyController.ListLiked();
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var songs = okResult.Value as List<Spotifysong>;
            Assert.IsNotNull(songs);

            return songs;
        }
    }


}

