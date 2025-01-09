using Newtonsoft.Json;

namespace TechTestBackend.Models
{
    public class TokenModel
    {
        //public string access_token { get; set; }
        //public string token_type { get; set; }
        //public int expires_in { get; set; }
        //public string scope { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public DateTime Created { get; set; }= DateTime.UtcNow;
        public bool IsValid
        {
            get { return Created.AddSeconds(ExpiresIn - 500) > DateTime.UtcNow; }
        }
    }
}
