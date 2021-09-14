// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Security;
using Tailspin.Surveys.Web.Controllers;
using Tailspin.Surveys.Web.Models;
using Tailspin.Surveys.Web.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using FakeItEasy;

namespace MultiTentantSurveyAppTests
{
    public class SurveyControllerTests
    {
        private ISurveyService _surveyService;
        private ILogger<SurveyController> _logger;
        private IAuthorizationService _authorizationService;
        private SurveyController _target;

        public SurveyControllerTests()
        {
            _surveyService = A.Fake<ISurveyService>();
            _logger = A.Fake<ILogger<SurveyController>>();
            _authorizationService = A.Fake<IAuthorizationService>();

            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("SurveysDb"));

            _target = new SurveyController(_surveyService, _logger, _authorizationService);
            _target.TempData = A.Fake<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
        }

        [Fact]
        public async Task Index_GetsUserSurveys()
        {
            var resultUserSurveys = A.Fake<UserSurveysDTO>();
            A.CallTo(() => _surveyService.GetSurveysForUserAsync(54321)).Returns(Task.FromResult(resultUserSurveys));

            _target.ControllerContext = CreateActionContextWithUserPrincipal("54321", "unregistereduser@contoso.com");
            var result = await _target.Index();
            var view = (ViewResult)result;
            Assert.Same(view.ViewData.Model, resultUserSurveys);
        }

        [Fact]
        public async Task Contributors_ShowsContributorsForSurvey()
        {
            var contributors = new ContributorsDTO();
            A.CallTo(() => _surveyService.GetSurveyContributorsAsync(A<int>.Ignored)).Returns(Task.FromResult(contributors));

            var result = await _target.Contributors(12345);
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(contributors, viewResult.ViewData.Model);
        }

        [Fact]
        public async Task RequestContributor_SavesRequest()
        {
            var contributorRequestViewModel = new SurveyContributorRequestViewModel { SurveyId = 123, EmailAddress = "unregistereduser@contoso.com" };

             var invitations = new List<ContributorRequest>();
            A.CallTo(() => _surveyService.AddContributorRequestAsync(A<ContributorRequest>.Ignored)).Invokes((ContributorRequest r) => invitations.Add(r)).Returns(Task.CompletedTask);

            // RequestContributor looks for existing contributors
            var contributorsDto = new ContributorsDTO
            {
                Contributors = new List<UserDTO>(),
                Requests = new List<ContributorRequest>()
            };

            A.CallTo(() => _surveyService.GetSurveyContributorsAsync(A<int>.Ignored)).Returns(Task.FromResult(contributorsDto));

            var result = await _target.RequestContributor(contributorRequestViewModel);

            Assert.Equal(123, invitations[0].SurveyId);
            Assert.Equal("unregistereduser@contoso.com", invitations[0].EmailAddress);
        }

        [Fact]
        public async Task Index_CallsProcessPendingContributorRequests()
        {
            bool surveyContributorProcessed = false;

            var apiResultContributors = A.Fake<ApiResult<ContributorsDTO>>();
            A.CallTo(() => apiResultContributors.Succeeded).Returns(true);
            A.CallTo(() => _surveyService.ProcessPendingContributorRequestsAsync()).Invokes(() => surveyContributorProcessed = true)
                .Returns(Task<ApiResult>.FromResult(new ApiResult()));

            _target.ControllerContext = CreateActionContextWithUserPrincipal("54321", "unregistereduser@contoso.com");
            var result = await _target.Index();

            Assert.True(surveyContributorProcessed);
        }

        #region Helpers

        private ControllerContext CreateActionContextWithUserPrincipal(string userId, string emailAddress)
        {
            var httpContext = A.Fake<HttpContext>();
            var routeData = A.Fake<RouteData>();
            var controllerActionDescriptor = A.Fake<ControllerActionDescriptor>();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(SurveyClaimTypes.SurveyUserIdClaimType, userId),
                new Claim(ClaimTypes.Email, emailAddress),
                new Claim(AzureADClaimTypes.ObjectId, "objectId"),
                new Claim(AzureADClaimTypes.TenantId, "TenantId"),
                new Claim(OpenIdConnectClaimTypes.IssuerValue, "issuer")

            }));
            A.CallTo(() => httpContext.User).Returns(principal);

            return new ControllerContext(
                new ActionContext(
                    httpContext,
                    routeData,
                    controllerActionDescriptor
                    ));
        }

        #endregion
    }
}
