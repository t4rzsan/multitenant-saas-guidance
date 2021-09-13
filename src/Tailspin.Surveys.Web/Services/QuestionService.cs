// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
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
        private readonly IDownstreamWebApi downstreamWebApi;
        private readonly string _serviceName;

        public QuestionService(IDownstreamWebApi downstreamWebApi, IOptions<ConfigurationOptions> configOptions)
        {
            this.downstreamWebApi = downstreamWebApi;
            _serviceName = configOptions.Value.SurveyApi.Name;
        }

        public async Task<ApiResult<QuestionDTO>> GetQuestionAsync(int id)
        {
            var response = await downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                     options =>
                    {
                        options.HttpMethod = HttpMethod.Get;
                        options.RelativePath = $"questions/{id}";
                    });
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<QuestionDTO>> CreateQuestionAsync(QuestionDTO question)
        {
            string jsonQuestion = JsonConvert.SerializeObject(question);
            StringContent content = new StringContent(jsonQuestion, Encoding.UTF8, "application/json");
            var response = await downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                    options =>
                    {
                        options.HttpMethod = HttpMethod.Post;
                        options.RelativePath = $"surveys/{question.SurveyId}/questions";
                    }, null, content);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<QuestionDTO>> UpdateQuestionAsync(QuestionDTO question)
        {
            string jsonQuestion = JsonConvert.SerializeObject(question);
            StringContent content = new StringContent(jsonQuestion, Encoding.UTF8, "application/json");
            var response = await downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                  options =>
                  {
                      options.HttpMethod = HttpMethod.Put;
                      options.RelativePath = $"questions/{question.Id}";
                  }, null, content);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult> DeleteQuestionAsync(int id)
        {
            var response = await downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                  options =>
                  {
                      options.HttpMethod = HttpMethod.Delete;
                      options.RelativePath = $"questions/{id}";
                  });
            return new ApiResult { Response = response };
        }
    }
}
