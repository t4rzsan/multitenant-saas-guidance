// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Configuration;

namespace Tailspin.Surveys.Web.Services
{
    /// <summary>
    /// This is the client for Tailspin.Surveys.WebAPI QuestionController
    /// Note: If we used Swagger for the API definition, we could generate the client.
    /// (see Azure API Apps) 
    /// Note the MVC6 version of Swashbuckler is called "Ahoy" and is still in beta: https://github.com/domaindrivendev/Ahoy
    /// 
    /// All methods set the user's access token in the Bearer authorization header. It is done authomatically by Microsoft.Identity.Web library. 
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

        public async Task<QuestionDTO> GetQuestionAsync(int id)
        {
            return await downstreamWebApi.CallWebApiForUserAsync<QuestionDTO>(_serviceName,
                     options =>
                    {
                        options.HttpMethod = HttpMethod.Get;
                        options.RelativePath = $"questions/{id}";
                    });
        }

        public async Task<QuestionDTO> CreateQuestionAsync(QuestionDTO question)
        {
            return await downstreamWebApi.PostForUserAsync<QuestionDTO, QuestionDTO>(_serviceName, $"surveys/{question.SurveyId}/questions", question);
        }

        public async Task<QuestionDTO> UpdateQuestionAsync(QuestionDTO question)
        {
            return await downstreamWebApi.PutForUserAsync<QuestionDTO, QuestionDTO>(_serviceName, $"questions/{question.Id}", question);
        }

        public async Task DeleteQuestionAsync(int id)
        {
            await downstreamWebApi.CallWebApiForUserAsync(_serviceName,
                  options =>
                  {
                      options.HttpMethod = HttpMethod.Delete;
                      options.RelativePath = $"questions/{id}";
                  });
        }
    }
}
