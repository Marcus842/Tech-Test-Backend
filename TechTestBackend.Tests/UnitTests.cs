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
using TechTestBackend.Tests.Services;

namespace TechTestBackend.Tests
{
    [TestClass]
    public class UnitTests
    {
        private readonly ISpotifyHttpService _spotify_http_service;
        private readonly SpotifyController _spotify_controller;
        private ILogger<SpotifyHttpService> _http_service_logger;
        private ILogger<SpotifyController> _controller_logger;
        private IOptions<SpotifyConfiguration> _options;
        private SongstorageContext _storage_context;

        public UnitTests()
        {
            InitateServicesForTests();

            _spotify_http_service = new SpotifyHttpService(_options, _http_service_logger);

            _spotify_controller = new SpotifyController(_controller_logger, new SpotifyHttpMockService(), _storage_context);
        }

        private void InitateServicesForTests()
        {
            var base_configuration = new ConfigurationBuilder()
                                        .AddUserSecrets<SpotifyHttpService>()
                                        .Build();

            var in_memory_settings = new Dictionary<string, string>{{ "Spotify:BaseUrl", "https://api.spotify.com/v1/" }};

            var final_configuration = new ConfigurationBuilder()
                .AddConfiguration(base_configuration)
                .AddInMemoryCollection(in_memory_settings)
                .Build();

            var spotify_config = new SpotifyConfiguration();
            final_configuration.GetSection("Spotify").Bind(spotify_config);
            _options = Options.Create(spotify_config);

            var services = new ServiceCollection();

            services.AddDbContextFactory<SongstorageContext>(options =>
                options.UseInMemoryDatabase("Songstorage"));

            services.AddLogging();
            var service_provider = services.BuildServiceProvider();

            var db_context_factory = service_provider.GetRequiredService<IDbContextFactory<SongstorageContext>>();

            _storage_context = db_context_factory.CreateDbContext();

            var factory = service_provider.GetService<ILoggerFactory>();

            _http_service_logger = factory.CreateLogger<SpotifyHttpService>();
            _controller_logger = factory.CreateLogger<SpotifyController>();
        }

        [TestMethod]
        public void TestTrackNotFound()
        {
            var song = _spotify_http_service.GetTrack("5eJ314ozT4CTPlyjdsG777");
            Assert.IsTrue(song.Id == null);
        }

        [TestMethod]
        public void TestTrackFound()
        {
            var song = _spotify_http_service.GetTrack("6jvqpOz4CrGUIk7d5iaI7i");//might fail if track is removed
            Assert.IsTrue(song != null);
        }

        [TestMethod]
        public void TestTracksFound()
        {
            var songs = _spotify_http_service.GetTracks("kent");//might fail if tracks with name is removed
            Assert.IsTrue(songs != null && songs.Count() > 0);
        }

        [TestMethod]
        public void TestSearchTracks()
        {
            var dummydata = new DummyData();

            var action_result = _spotify_controller.SearchTracks("kent");
            var ok_result = action_result as OkObjectResult;
            Assert.IsNotNull(ok_result);

            var songs = ok_result.Value as Spotifysong[];
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

            var songs_from_dummydata = dummydata.GetSongs();
            var song_to_compare_with = songs_from_dummydata[1];
            Assert.IsTrue(song.Id == song_to_compare_with.Id && song.Name == song_to_compare_with.Name);
        }

        [TestMethod]
        public void RemoveLikedSongThatDoesNotExistInSpotify()
        {
            var action_result = _spotify_controller.RemoveLike("5YCKObb1A7YIeOKzXhREwz");
            var statuscode_result = action_result as StatusCodeResult;
            Assert.IsNotNull(statuscode_result);

            Assert.IsTrue(statuscode_result.StatusCode == 400);
        }

        [TestMethod]
        public void RemoveLikedAndListSongs()
        {
            var action_result = _spotify_controller.RemoveLike("5eJ314ozT4CTPlyjdsGq78");
            var ok_result = action_result as OkResult;
            Assert.IsNotNull(ok_result);

            var songs = ListLikedSongs();

            Assert.IsTrue(songs.Count() == 0);
        }

        private void LikeSongThatDoesNotExist()
        {
            var action_result = _spotify_controller.Like("5YCKObb1A7YIeOKzXhREwz");
            var statuscode_result = action_result as StatusCodeResult;
            Assert.IsNotNull(statuscode_result);

            Assert.IsTrue(statuscode_result.StatusCode == 400);
        }

        private void LikeSongThatExist()
        {
            var action_result = _spotify_controller.Like("5eJ314ozT4CTPlyjdsGq78");
            var ok_result = action_result as OkResult;
            Assert.IsNotNull(ok_result);
        }

        private List<Spotifysong> ListLikedSongs()
        {
            var action_result = _spotify_controller.ListLiked();
            var ok_result = action_result as OkObjectResult;
            Assert.IsNotNull(ok_result);

            var songs = ok_result.Value as List<Spotifysong>;
            Assert.IsNotNull(songs);

            return songs;
        }
    }


}

