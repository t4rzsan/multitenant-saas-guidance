// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Configuration;
using Tailspin.Surveys.Web.Models;

namespace Tailspin.Surveys.Web.Services
{
    /// <summary>
    /// This is the client for Tailspin.Surveys.WebAPI SurveyController
    /// Note: If we used Swagger for the API definition, we could generate the client.
    /// (see Azure API Apps) 
    /// Note the MVC6 version of Swashbuckler is called "Ahoy" and is still in beta: https://github.com/domaindrivendev/Ahoy
    ///
    /// All methods except GetPublishedSurveysAsync set the user's access token in the Bearer authorization header 
    /// to allow the WebAPI to run on behalf of the signed in user.
    /// </summary>
    public class SurveyService : ISurveyService
    {
        private readonly string _serviceName;
        private readonly IDownstreamWebApi _downstreamWebApi;
        private readonly HttpClient _httpClient;


        public SurveyService(HttpClientService factory, IDownstreamWebApi downstreamWebApi, IOptions<ConfigurationOptions> configOptions)
        {
            _httpClient = factory.GetHttpClient();
            _serviceName = configOptions.Value.SurveyApi.Name;
            _downstreamWebApi = downstreamWebApi;
        }

        public async Task<SurveyDTO> GetSurveyAsync(int id)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync<SurveyDTO>(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Get;
                        options.RelativePath = $"surveys/{id}";
                    });
        }

        public async Task<UserSurveysDTO> GetSurveysForUserAsync(int userId)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync<UserSurveysDTO>(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Get;
                        options.RelativePath = $"users/{userId}/surveys";
                    });
        }

        public async Task<TenantSurveysDTO> GetSurveysForTenantAsync(int tenantId)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync<TenantSurveysDTO>(_serviceName,
                   options =>
                   {
                       options.HttpMethod = HttpMethod.Get;
                       options.RelativePath = $"tenants/{tenantId}/surveys";
                   });
        }
        public async Task<ApiResult<IEnumerable<SurveyDTO>>> GetPublishedSurveysAsync()
        {
            var path = "/surveys/published";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<IEnumerable<SurveyDTO>>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<SurveyDTO> CreateSurveyAsync(SurveyDTO survey)
        {
            return await _downstreamWebApi.PostForUserAsync<SurveyDTO, SurveyDTO>(_serviceName, "surveys", survey);
        }

        public async Task<SurveyDTO> UpdateSurveyAsync(SurveyDTO survey)
        {
            return await _downstreamWebApi.PutForUserAsync<SurveyDTO, SurveyDTO>(_serviceName, $"surveys/{survey.Id}", survey);
        }

        public async Task<SurveyDTO> DeleteSurveyAsync(int id)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync<SurveyDTO>(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Delete;
                        options.RelativePath = $"surveys/{id}";
                    });
        }
        public async Task<SurveyDTO> PublishSurveyAsync(int id)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync<SurveyDTO>(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Put;
                        options.RelativePath = $"surveys/{id}/publish";
                    });
        }
        public async Task<SurveyDTO> UnPublishSurveyAsync(int id)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync<SurveyDTO>(_serviceName,
                  options =>
                  {
                      options.HttpMethod = HttpMethod.Put;
                      options.RelativePath = $"surveys/{id}/unpublish";
                  });
        }

        public async Task<ContributorsDTO> GetSurveyContributorsAsync(int id)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync<ContributorsDTO>(_serviceName,
                options =>
                {
                    options.HttpMethod = HttpMethod.Get;
                    options.RelativePath = $"surveys/{id}/contributors";
                });
        }

        public async Task ProcessPendingContributorRequestsAsync()
        {
            await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                  options =>
                  {
                      options.HttpMethod = HttpMethod.Post;
                      options.RelativePath = "/surveys/processpendingcontributorrequests";
                  });
        }

        public async Task AddContributorRequestAsync(ContributorRequest contributorRequest)
        {
            string jsonContributor = JsonConvert.SerializeObject(contributorRequest);
            StringContent content = new StringContent(jsonContributor, Encoding.UTF8, "application/json");
            await _downstreamWebApi.PostForUserAsync<object, ContributorRequest>(_serviceName, $"surveys/{contributorRequest.SurveyId}/contributorrequests", contributorRequest);
        }
    }
}
