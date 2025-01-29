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
        private readonly HttpClient _httpClient;
        private readonly SpotifyConfiguration _configuration;
        private readonly ILogger<SpotifyHttpService> _logger;
        private TokenModel _token;
        private readonly string _baseUrl;

        public SpotifyHttpService(IOptions<SpotifyConfiguration> options, ILogger<SpotifyHttpService> logger, HttpClient http_client)
        {
            _httpClient = http_client;
            _configuration = options.Value;
            _logger = logger;

            _baseUrl = _configuration.BaseUrl;
        }

        private void CreateOrRenewTokenIfNeeded()
        {
            if (_token == null || !_token.IsValid)
            {
                var client_id = _configuration.CliendID;
                var client_secret = _configuration.CliendSecret;
                var encoding = Encoding.ASCII.GetBytes($"{client_id}:{client_secret}");
                var base64 = Convert.ToBase64String(encoding);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

                var nameValueCollection = new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") };
                var formUrlEncodedContent = new FormUrlEncodedContent(nameValueCollection);
                var response = _httpClient.PostAsync("https://accounts.spotify.com/api/token", formUrlEncodedContent).Result;
                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response);
                    return;
                }
                var content = response.Content.ReadAsStringAsync().Result;
                _token = JsonConvert.DeserializeObject<TokenModel>(content);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken.ToString());
            }
        }

        public Spotifysong[] GetTracks(string name)
        {
            CreateOrRenewTokenIfNeeded();

            var requestUrl = _baseUrl + "search?q=" + name + "&type=track";
            var response = _httpClient.GetAsync(requestUrl).Result;

            if (!response.IsSuccessStatusCode)
            {
                var errorCode = HandleErrorResponse(response);
                if (errorCode == 404)
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

            var requestUrl = _baseUrl + "tracks/" + id + "/";
            var response = _httpClient.GetAsync(requestUrl).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorCode = HandleErrorResponse(response);
                if (errorCode == 404)
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
            var errorMessage = $"Unsuccessful call to spotify API. Error code: {error.StatusCode} Error: {error.Error}";
            _logger.LogError(errorMessage);
            if (error?.StatusCode == 404)
            {
                return error.StatusCode;
            }
            throw new Exception(errorMessage);
        }
    }
}
