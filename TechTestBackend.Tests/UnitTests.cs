using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TechTestBackend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechTestBackend.Configuration;
using TechTestBackend.Controllers;
using Microsoft.AspNetCore.Mvc;
using TechTestBackend.Models;
using TechTestBackend.Tests.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace TechTestBackend.Tests
{
    [TestClass]
    public class UnitTests
    {
        private readonly ISpotifyHttpService _spotifyHttpService;
        private readonly SpotifyController _spotifyController;
        private ILogger<SpotifyHttpService> _httpServiceLogger;
        private ILogger<SpotifyController> _controllerLogger;
        private IOptions<SpotifyConfiguration> _options;
        private SongstorageContext _storageContext;

        public UnitTests()
        {
            InitateServicesForTests();

            _spotifyHttpService = new SpotifyHttpService(_options, _httpServiceLogger);

            _spotifyController = new SpotifyController(_controllerLogger, new SpotifyHttpMockService(), _storageContext);
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

            var spotifyConfig = new SpotifyConfiguration();
            finalConfiguration.GetSection("Spotify").Bind(spotifyConfig);
            _options = Options.Create(spotifyConfig);

            var services = new ServiceCollection();

            services.AddDbContextFactory<SongstorageContext>(options =>
                options.UseInMemoryDatabase("Songstorage"));

            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();

            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<SongstorageContext>>();

            _storageContext = dbContextFactory.CreateDbContext();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            _httpServiceLogger = factory.CreateLogger<SpotifyHttpService>();
            _controllerLogger = factory.CreateLogger<SpotifyController>();
        }

        [TestMethod]
        public void TestTrakNotFound()
        {
            var song = _spotifyHttpService.GetTrack("5eJ314ozT4CTPlyjdsG777");
            Assert.IsTrue(song.Id == null);
        }

        [TestMethod]
        public void TestTrakFound()
        {
            var song = _spotifyHttpService.GetTrack("6jvqpOz4CrGUIk7d5iaI7i");//might fail if trak is removed
            Assert.IsTrue(song != null);
        }

        [TestMethod]
        public void TestTraksFound()
        {
            var songs = _spotifyHttpService.GetTracks("kent");//might fail if traks with name is removed
            Assert.IsTrue(songs != null && songs.Count() > 0);
        }

        [TestMethod]
        public void TestSearchTracks()
        {
            var dummyData = new DummyData();

            object actionResult = _spotifyController.SearchTracks("kent");
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var songs = okResult.Value as Spotifysong[];
            Assert.IsNotNull(songs);

            Assert.IsTrue(dummyData.ListsAreEqual(songs));
        }

        [TestMethod]
        public void LikeAndListLikedSongs()
        {
            LikeSongThatDoesNotExist();
            LikeSongThatExist();
            var songs = ListLikedSongs();

            Assert.IsTrue(songs.Count() == 1);

            var song = songs[0];
            var dummyData = new DummyData();

            var songsFromDummydata = dummyData.GetSongs();
            var songToCompareWith = songsFromDummydata[1];
            Assert.IsTrue(song.Id == songToCompareWith.Id && song.Name == songToCompareWith.Name);
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

