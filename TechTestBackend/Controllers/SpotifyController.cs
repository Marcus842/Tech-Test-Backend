using Microsoft.AspNetCore.Mvc;
using TechTestBackend.Models;
using TechTestBackend.Helpers;
using TechTestBackend.Services;

namespace TechTestBackend.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    private readonly ILogger<SpotifyController> _logger;
    private readonly ISpotifyHttpService _spotifyService;

    public SpotifyController(ILogger<SpotifyController> logger, ISpotifyHttpService spotifyService)
    {
        _logger = logger;
        _spotifyService = spotifyService;
        SpotifyHelper.SpotifyService = _spotifyService;
    }

    [HttpGet]
    [Route("searchTracks")]
    public IActionResult SearchTracks(string name)
    {
        try
        {
            object trak = SpotifyHelper.GetTracks(name);

            _logger.LogDebug($"Successfully got trak: {trak}");
            return Ok(trak);
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
            var storage = HttpContext.RequestServices.GetService(typeof(SongstorageContext)) as SongstorageContext;

            var track = SpotifyHelper.GetTrack(id); //check if trak exists
            if (track.Id == null || SpotifyId(id) == false)
            {
                _logger.LogWarning("Trak does not exist");
                return StatusCode(400);
            }

            var song = new Soptifysong(); //create new song
            song.Id = id;
            song.Name = track.Name;


            if (!SongExists(id, storage))
            {
                storage.Songs.Add(song);

                storage.SaveChanges();
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
            var storage = HttpContext.RequestServices.GetService(typeof(SongstorageContext)) as SongstorageContext;

            var track = SpotifyHelper.GetTrack(id);
            if (track.Id == null || SpotifyId(id) == false)
            {
                _logger.LogWarning("Trak does not exist");
                return StatusCode(400); // bad request wrong id not existing in spotify
            }


            if (SongExists(id, storage))
            {
                var song = storage.Songs.First(e => e.Id == id);

                storage.Songs.Remove(song);
                storage.SaveChanges();
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
        List<Soptifysong> songs = new List<Soptifysong>();
        try
        {
            var storage = HttpContext.RequestServices.GetService(typeof(SongstorageContext)) as SongstorageContext;

            int songsnumber = storage.Songs.Count();

            if (songsnumber > 0)
            {
                for (int i = 0; i <= songsnumber - 1; i++)
                {
                    var song = storage.Songs.ToList()[i];
                    string songid = song.Id;

                    var track = SpotifyHelper.GetTrack(songid);
                    if (track.Id == null)
                    {
                        if (SongExists(songid, storage))
                        {
                            storage.Songs.Remove(song);
                            storage.SaveChanges();
                            _logger.LogDebug($"Successfully removed song from storage: {song}");
                        }
                    }
                }
            }
            songs = storage.Songs.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error listing liked songs");
            return Ok();
        }


        return Ok(songs);
    }

    private bool SongExists(string id, SongstorageContext storage)
    {
        return storage.Songs.Any() && storage.Songs.FirstOrDefault(e => e.Id == id) != null;
    }

    private static bool SpotifyId(object id)
    {
        return id.ToString().Length == 22;
    }
}