﻿using Microsoft.Extensions.Options;
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
        private TokenModel token;
        private string base_url;

        public SpotifyHttpService(IOptions<SpotifyConfiguration> options, ILogger<SpotifyHttpService> logger) {
            _httpClient = new HttpClient();
            _configuration = options.Value;
            _logger = logger;

            base_url = _configuration.BaseUrl;
        }

        private void GetAuthorizationHeader()
        {
            if(token == null || !token.IsValid)
            {
                var client_id = _configuration.CliendID;
                var client_secret = _configuration.CliendSecret;
                var encoding = Encoding.ASCII.GetBytes($"{client_id}:{client_secret}");
                var base64 = Convert.ToBase64String(encoding);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

                var name_value_collection = new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") };
                var form_url_encoded_content = new FormUrlEncodedContent(name_value_collection);
                var response = _httpClient.PostAsync("https://accounts.spotify.com/api/token", form_url_encoded_content).Result;
                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response);
                }
                var content = response.Content.ReadAsStringAsync().Result;
                token = JsonConvert.DeserializeObject<TokenModel>(content);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken.ToString());
            }
        }

        public Soptifysong[] GetTracks(string name)
        {
            GetAuthorizationHeader();

            var request_uri = base_url + "search?q=" + name + "&type=track";
            var response = _httpClient.GetAsync(request_uri).Result;

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response);
            }
            var content = response.Content.ReadAsStringAsync().Result;
            dynamic objects = JsonConvert.DeserializeObject(content);

            var spotify_song_items = objects.tracks.items.ToString();
            var songs = JsonConvert.DeserializeObject<Soptifysong[]>(spotify_song_items);

            return songs;
        }

        public Soptifysong GetTrack(string id)
        {
            GetAuthorizationHeader();

            var request_uri = base_url + "tracks/" + id + "/";
            var response = _httpClient.GetAsync(request_uri).Result;
            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response);
            }
            dynamic objects = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

            var song = JsonConvert.DeserializeObject<Soptifysong>(objects.ToString());

            return song;
        }

        private void HandleErrorResponse(HttpResponseMessage response)
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
            throw new Exception(errorMessage);
        }
    }
}