#nullable disable

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MovieProject.Enums;
using MovieProject.Models.Settings;
using MovieProject.Models.TmDb;
using MovieProject.Services.Interfaces;
using System.Runtime.Serialization.Json;


namespace MovieProject.Services
{
    public class TmDbMovieService : IRemoteMovieService
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _config;

        public TmDbMovieService(IOptions<AppSettings> appSettings, IHttpClientFactory httpClient, IConfiguration config)
        {
            _appSettings = appSettings.Value;
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<ActorDetail> ActorDetailAsync(int id)
        {
            //Setup a default return object
            ActorDetail actorDetail = new();

            //Assemble the full request uri string
            var query = $"{_appSettings.TmDbSettings.BaseUrl}/person/{id}";
            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _config["TmDbApiKey"] },
                { "language", _appSettings.TmDbSettings.QueryOptions.Language}
            };
            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            //Create a client and execute the request
            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await client.SendAsync(request);

            //Return the ActorDetail object
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                var dcjs = new DataContractJsonSerializer(typeof(ActorDetail));
                actorDetail = (ActorDetail)dcjs.ReadObject(responseStream);
            }

            return actorDetail;
        }

        public async Task<MovieDetail> MovieDetailAsync(int id)
        {
            //Setup default return object
            MovieDetail movieDetail = new();

            //Assemble the request
            var query = $"{_appSettings.TmDbSettings.BaseUrl}/movie/{id}";
            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _config["TmDbApiKey"] },
                { "language", _appSettings.TmDbSettings.QueryOptions.Language},
                { "append_to_response", _appSettings.TmDbSettings.QueryOptions.AppendToResponse}
            };
            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            //Create client and execute request
            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await client.SendAsync(request);

            //Deserialize into Moviedetail 
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync(); //Stream??
                var dcjs = new DataContractJsonSerializer(typeof(MovieDetail));
                movieDetail = dcjs.ReadObject(responseStream) as MovieDetail;
            }
            return movieDetail;
        }

        public async Task<MovieSearch> SearchMoviesAsync(MovieCategory category, int count)
        {

            //Setup a default instance of MovieSearch
            MovieSearch movieSearch = new();

            //Assemble the full request uri string
            var query = $"{_appSettings.TmDbSettings.BaseUrl}/movie/{category}";

            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _config["TmDbApiKey"] },
                { "language", _appSettings.TmDbSettings.QueryOptions.Language },
                { "page", _appSettings.TmDbSettings.QueryOptions.Page }
            };

            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            //Create a client and execute the request
            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await client.SendAsync(request);

            //Return MovieSearch object
            if (response.IsSuccessStatusCode)
            {
                var dcjs = new DataContractJsonSerializer(typeof(MovieSearch));
                using var responseStream = await response.Content.ReadAsStreamAsync(); //manage memory more agressively that otherwise managed by garbage collection
                movieSearch = (MovieSearch)dcjs.ReadObject(responseStream);
                movieSearch.results = movieSearch.results.Take(count).ToArray(); //limit results taken in

                //configure the image path for each result
                movieSearch.results.ToList().ForEach(r => r.poster_path = $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieProjectSettings.DefaultPosterSize}/{r.poster_path}");
            }

            return movieSearch;
        }
    }
}
