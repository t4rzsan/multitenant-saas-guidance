// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Tailspin.Surveys.Common;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Configuration;
using Tailspin.Surveys.Web.Models;

namespace Tailspin.Surveys.Web.Services
{
    /// <summary>
    /// This is the client for Tailspin.Surveys.WebAPI QuestionController
    /// Note: If we used Swagger for the API definition, we could generate the client.
    /// (see Azure API Apps) 
    /// Note the MVC6 version of Swashbuckler is called "Ahoy" and is still in beta: https://github.com/domaindrivendev/Ahoy
    /// 
    /// All methods set the user's access token in the Bearer authorization header 
    /// to allow the WebAPI to run on behalf of the signed in user.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly HttpClient _httpClient;
        private readonly CancellationToken _cancellationToken;
        private readonly string[] _scopes;

       public QuestionService(HttpClientService factory, IHttpContextAccessor httpContextAccessor, ITokenAcquisition tokenAcquisition, IOptions<ConfigurationOptions> configOptions)
        {
            _tokenAcquisition = tokenAcquisition;
            _httpClient = factory.GetHttpClient();
            _cancellationToken = httpContextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
            _scopes = configOptions.Value.SurveyApi.Scope.Split(';');
        }

        public async Task<ApiResult<QuestionDTO>> GetQuestionAsync(int id)
        {
            var path = $"/questions/{id}";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Get, path, null,
                        await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes), _cancellationToken);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<QuestionDTO>> CreateQuestionAsync(QuestionDTO question)
        {
            var path = $"/surveys/{question.SurveyId}/questions";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Post, path, question,
                       await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes), _cancellationToken);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<QuestionDTO>> UpdateQuestionAsync(QuestionDTO question)
        {
            var path = $"/questions/{question.Id}";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Put, path, question,
                       await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes), _cancellationToken);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult> DeleteQuestionAsync(int id)
        {
            var path = $"/questions/{id}";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Delete, path, null,
                        await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes), _cancellationToken);
            return new ApiResult { Response = response };
        }
    }
}
