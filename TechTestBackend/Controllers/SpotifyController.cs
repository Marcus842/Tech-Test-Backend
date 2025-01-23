using Microsoft.AspNetCore.Mvc;
using TechTestBackend.Models;
using TechTestBackend.Services;

namespace TechTestBackend.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    private readonly ILogger<SpotifyController> _logger;
    private readonly ISpotifyHttpService _spotify_service;
    private readonly SongstorageContext _storage;

    public SpotifyController(ILogger<SpotifyController> logger, ISpotifyHttpService spotify_service, SongstorageContext storage)
    {
        _logger = logger;
        _spotify_service = spotify_service;
        _storage = storage;
    }

    [HttpGet]
    [Route("searchTracks")]
    public IActionResult SearchTracks(string name)
    {
        try
        {
            object track = _spotify_service.GetTracks(name);

            _logger.LogDebug($"Successfully got track: {track}");
            return Ok(track);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching tracks");
            // this is the best practice for not leaking error details
            return NotFound();
        }
    }

    [HttpPost]
    [Route("like")]
    public IActionResult Like(string id)
    {
        try
        {
            var track = _spotify_service.GetTrack(id); //check if track exists
            if (track.Id == null || SpotifyId(id) == false)
            {
                _logger.LogWarning("Track does not exist");
                return StatusCode(400);
            }

            var song = new Spotifysong();
            song.Id = id;
            song.Name = track.Name;


            if (!SongExists(id))
            {
                _storage.Songs.Add(song);

                _storage.SaveChanges();
                _logger.LogDebug($"Successfully added song to storage: {song}");
            }
            else
            {
                _logger.LogDebug($"Successfully already added song to storage: {song}");
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error adding song with id: {id} to liked songs");

            // not sure if this is the best way to handle this
            return Ok();
        }

        return Ok();
    }

    [HttpPost]
    [Route("removeLike")]
    public IActionResult RemoveLike(string id)
    {
        try
        {
            var track = _spotify_service.GetTrack(id);
            if (track.Id == null || SpotifyId(id) == false)
            {
                _logger.LogWarning("Track does not exist");
                return StatusCode(400); // bad request wrong id not existing in spotify
            }


            if (SongExists(id))
            {
                var song = _storage.Songs.First(e => e.Id == id);

                _storage.Songs.Remove(song);
                _storage.SaveChanges();
                _logger.LogDebug($"Successfully removed song from storage: {song}");
            }
            else
            {
                _logger.LogWarning($"Song with id: {id} does not exist in storage");
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error removing song with id: {id} from liked songs");
            return Ok();
        }

        return Ok();
    }

    [HttpGet]
    [Route("listLiked")]
    public IActionResult ListLiked()
    {
        List<Spotifysong> songs = new List<Spotifysong>();
        try
        {
            int songsnumber = _storage.Songs.Count();

            if (songsnumber > 0)
            {
                for (int i = 0; i <= songsnumber - 1; i++)
                {
                    var song = _storage.Songs.ToList()[i];
                    string songid = song.Id;

                    var track = _spotify_service.GetTrack(songid);
                    if (track.Id == null)
                    {
                        if (SongExists(songid))
                        {
                            _storage.Songs.Remove(song);
                            _storage.SaveChanges();
                            _logger.LogDebug($"Successfully removed song from storage: {song}");
                        }
                    }
                }
            }
            songs = _storage.Songs.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error listing liked songs");
            return Ok();
        }


        return Ok(songs);
    }

    private bool SongExists(string id)
    {
        return _storage.Songs.Any() && _storage.Songs.FirstOrDefault(e => e.Id == id) != null;
    }

    private static bool SpotifyId(object id)
    {
        return id.ToString().Length == 22;
    }
}