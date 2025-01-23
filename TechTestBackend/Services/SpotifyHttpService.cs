using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TechTestBackend.Configuration;
using TechTestBackend.Models;

namespace TechTestBackend.Services
{
    public class SpotifyHttpService : ISpotifyHttpService
    {
        private readonly HttpClient _http_client;
        private readonly SpotifyConfiguration _configuration;
        private readonly ILogger<SpotifyHttpService> _logger;
        private TokenModel _token;
        private string _base_url;

        public SpotifyHttpService(IOptions<SpotifyConfiguration> options, ILogger<SpotifyHttpService> logger, HttpClient http_client)
        {
            _http_client = http_client;
            _configuration = options.Value;
            _logger = logger;

            _base_url = _configuration.BaseUrl;
        }

        private void CreateOrRenewTokenIfNeeded()
        {
            if (_token == null || !_token.IsValid)
            {
                var client_id = _configuration.CliendID;
                var client_secret = _configuration.CliendSecret;
                var encoding = Encoding.ASCII.GetBytes($"{client_id}:{client_secret}");
                var base64 = Convert.ToBase64String(encoding);
                _http_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

                var name_value_collection = new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") };
                var form_url_encoded_content = new FormUrlEncodedContent(name_value_collection);
                var response = _http_client.PostAsync("https://accounts.spotify.com/api/token", form_url_encoded_content).Result;
                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response);
                    return;
                }
                var content = response.Content.ReadAsStringAsync().Result;
                _token = JsonConvert.DeserializeObject<TokenModel>(content);

                _http_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken.ToString());
            }
        }

        public Spotifysong[] GetTracks(string name)
        {
            CreateOrRenewTokenIfNeeded();

            var request_url = _base_url + "search?q=" + name + "&type=track";
            var response = _http_client.GetAsync(request_url).Result;

            if (!response.IsSuccessStatusCode)
            {
                var error_status = HandleErrorResponse(response);
                if (error_status == 404)
                {
                    return Array.Empty<Spotifysong>();
                }
            }
            var content = response.Content.ReadAsStringAsync().Result;
            var objects = JsonConvert.DeserializeObject<SpotifyTracksResponseModel>(content);
            var songs = objects.Tracks.Items;

            return songs;
        }

        public Spotifysong GetTrack(string id)
        {
            CreateOrRenewTokenIfNeeded();

            var request_url = _base_url + "tracks/" + id + "/";
            var response = _http_client.GetAsync(request_url).Result;
            if (!response.IsSuccessStatusCode)
            {
                var error_status = HandleErrorResponse(response);
                if (error_status == 404)
                {
                    return new Spotifysong();
                }
            }
            var objects = response.Content.ReadAsStringAsync().Result;

            var song = JsonConvert.DeserializeObject<Spotifysong>(objects);

            return song;
        }

        private int HandleErrorResponse(HttpResponseMessage response)
        {
            var error = new ErrorResponseModel();
            try
            {
                var content = response.Content.ReadAsStringAsync().Result;
                error = JsonConvert.DeserializeObject<ErrorResponseModel>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing spotify API error response");
            }
            finally
            {
                if (error != null)
                {
                    error.StatusCode = (int)response.StatusCode;
                }
                else
                {
                    error = new ErrorResponseModel() { StatusCode = (int)response.StatusCode };
                }
            }
            var error_message = $"Unsuccessful call to spotify API. Error code: {error.StatusCode} Error: {error.Error}";
            _logger.LogError(error_message);
            if (error?.StatusCode == 404)
            {
                return error.StatusCode;
            }
            throw new Exception(error_message);
        }
    }
}
