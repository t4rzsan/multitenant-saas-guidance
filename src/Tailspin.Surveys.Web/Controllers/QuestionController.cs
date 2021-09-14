// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Security;
using Tailspin.Surveys.Web.Services;
using Microsoft.Identity.Web;
using System.Net.Http;

namespace Tailspin.Surveys.Web.Controllers
{
    /// <summary>
    /// This MVC controller provides actions for the management of <see cref="Tailspin.Surveys.Data.DataModels.Question"/>s.
    /// The actions in this controller class require the user to be signed in.
    /// </summary>
    [AuthorizeForScopes(ScopeKeySection = "SurveyApi:Scope")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionsClient, SignInManager signInManager)
        {
            _questionService = questionsClient;
        }

        /// <summary>
        /// This action provides the Http Get experience for creating a <see cref="Tailspin.Surveys.Data.DataModels.Question"/> in the context of a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/>.
        /// </summary>
        /// <param name="id">The id of a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/></param>
        /// <returns>A view with form fields for a new <see cref="Tailspin.Surveys.Data.DataModels.Question"/></returns>
        public IActionResult Create(int id)
        {
            var question = new QuestionDTO { SurveyId = id };
            return View(question);
        }

        /// <summary>
        /// This action provides the Http Post experience for creating a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="question">The <see cref="QuestionDTO"/> instance that contains the fields necessary to create a <see cref="Tailspin.Surveys.Data.DataModels.Question"/></param>
        /// <returns>A view that either shows validation errors or a redirection to the Survey Edit experience</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionDTO question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _questionService.CreateQuestionAsync(question);
                    return RedirectToAction("Edit", "Survey", new { id = question.SurveyId });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Bad Request");
                    return View(question);
                }
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "403 Forbidden".Equals(requestEx.Message?.Trim()))
            {
                return ForbidenAccessToTheQuestion(question);
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Get experience for editing a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="id">The id of a <see cref="Tailspin.Surveys.Data.DataModels.Question"/></param>
        /// <returns>A view with form fields for the <see cref="Tailspin.Surveys.Data.DataModels.Question"/> being edited</returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var result = await _questionService.GetQuestionAsync(id);
                return View(result);
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "403 Forbidden".Equals(requestEx.Message?.Trim()))
            {
                return ForbidenAccessToTheQuestion();
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "404 NotFound".Equals(requestEx.Message?.Trim()))
            {
                return QuestionNotFound();
            }
            catch
            {
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Post experience for editing a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="question">The <see cref="QuestionDTO"/> instance that contains the <see cref="Tailspin.Surveys.Data.DataModels.Question"/>'s updated fields</param>
        /// <returns>A view that either shows validation errors or a redirection to the Survey Edit experience</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionDTO question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _questionService.UpdateQuestionAsync(question);
                    return RedirectToAction("Edit", "Survey", new { id = question.SurveyId });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Bad Request");
                    return View(question);
                }
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "403 Forbidden".Equals(requestEx.Message?.Trim()))
            {
                return ForbidenAccessToTheQuestion(question);
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "404 NotFound".Equals(requestEx.Message?.Trim()))
            {
                return QuestionNotFound();
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Get experience for deleting a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Tailspin.Surveys.Data.DataModels.Question"/></param>
        /// <returns>A view that shows a delete confirmation prompt</returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _questionService.GetQuestionAsync(id);
                return View("Delete", result);
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "403 Forbidden".Equals(requestEx.Message?.Trim()))
            {
                return ForbidenAccessToTheQuestion();
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "404 NotFound".Equals(requestEx.Message?.Trim()))
            {
                return QuestionNotFound();
            }
            catch
            {
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Post experience for deleting a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="question">The <see cref="QuestionDTO"/> instance that contains the id of the <see cref="Tailspin.Surveys.Data.DataModels.Question"/> to be deleted</param>
        /// <returns>A view that either shows validation errors or a redirection to the Survey Edit experience</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(QuestionDTO question)
        {
            try
            {
                await _questionService.DeleteQuestionAsync(question.Id);
                return RedirectToAction("Edit", "Survey", new { id = question.SurveyId });
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "403 Forbidden".Equals(requestEx.Message?.Trim()))
            {
                return ForbidenAccessToTheQuestion(question);
            }
            catch (HttpRequestException requestEx) when ("Microsoft.Identity.Web".Equals(requestEx.Source) && "404 NotFound".Equals(requestEx.Message?.Trim()))
            {
                return QuestionNotFound();
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        private IActionResult QuestionNotFound()
        {
            ModelState.AddModelError(string.Empty, $"The question can not be found");
            ViewBag.Message = $"The quetion can not be found";
            return View("~/Views/Shared/Error.cshtml");
        }

        private IActionResult ForbidenAccessToTheQuestion(QuestionDTO question)
        {
            ViewBag.Forbidden = true;
            return View(question);
        }

        private IActionResult ForbidenAccessToTheQuestion()
        {
            ModelState.AddModelError(string.Empty, $"Forbidden Access to the question");
            ViewBag.Message = $"Forbidden Access to the question";
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
