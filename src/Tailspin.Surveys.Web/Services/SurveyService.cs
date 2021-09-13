// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        public async Task<ApiResult<SurveyDTO>> GetSurveyAsync(int id)
        {
            var path = $"/surveys/{id}";
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Get;
                        options.RelativePath = $"surveys/{id}";
                    });
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<UserSurveysDTO>> GetSurveysForUserAsync(int userId)
        {
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Get;
                        options.RelativePath = $"users/{userId}/surveys";
                    });
            return await ApiResult<UserSurveysDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<TenantSurveysDTO>> GetSurveysForTenantAsync(int tenantId)
        {
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                   options =>
                   {
                       options.HttpMethod = HttpMethod.Get;
                       options.RelativePath = $"tenants/{tenantId}/surveys";
                   });
            return await ApiResult<TenantSurveysDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<IEnumerable<SurveyDTO>>> GetPublishedSurveysAsync()
        {
            var path = "/surveys/published";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<IEnumerable<SurveyDTO>>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> CreateSurveyAsync(SurveyDTO survey)
        {
            string jsonSurvey = JsonConvert.SerializeObject(survey);
            StringContent content = new StringContent(jsonSurvey, Encoding.UTF8, "application/json");
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Post;
                        options.RelativePath = "surveys";
                    }, null, content);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> UpdateSurveyAsync(SurveyDTO survey)
        {
            string jsonSurvey = JsonConvert.SerializeObject(survey);
            StringContent content = new StringContent(jsonSurvey, Encoding.UTF8, "application/json");
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Put;
                        options.RelativePath = $"surveys/{survey.Id}";
                    }, null, content);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> DeleteSurveyAsync(int id)
        {
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Delete;
                        options.RelativePath = $"surveys/{id}";
                    });
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<SurveyDTO>> PublishSurveyAsync(int id)
        {
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                   options =>
                   {
                       options.HttpMethod = HttpMethod.Put;
                       options.RelativePath = $"surveys/{id}/publish";
                   });
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<SurveyDTO>> UnPublishSurveyAsync(int id)
        {
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                  options =>
                  {
                      options.HttpMethod = HttpMethod.Put;
                      options.RelativePath = $"surveys/{id}/unpublish";
                  });
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<ContributorsDTO>> GetSurveyContributorsAsync(int id)
        {
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                options =>
                {
                    options.HttpMethod = HttpMethod.Get;
                    options.RelativePath = $"surveys/{id}/contributors";
                });
            return await ApiResult<ContributorsDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult> ProcessPendingContributorRequestsAsync()
        {
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                  options =>
                  {
                      options.HttpMethod = HttpMethod.Post;
                      options.RelativePath = "/surveys/processpendingcontributorrequests";
                  });
            return new ApiResult { Response = response };
        }

        public async Task<ApiResult> AddContributorRequestAsync(ContributorRequest contributorRequest)
        {
            string jsonContributor = JsonConvert.SerializeObject(contributorRequest);
            StringContent content = new StringContent(jsonContributor, Encoding.UTF8, "application/json");
            var response = await _downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Post;
                        options.RelativePath = $"/surveys/{contributorRequest.SurveyId}/contributorrequests";
                    }, null, content);
            return new ApiResult { Response = response };
        }
    }
}
