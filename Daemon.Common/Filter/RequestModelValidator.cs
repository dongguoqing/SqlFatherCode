using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Daemon.Common.Filter
{
    /// <summary>
    /// request model validator
    /// </summary>
    public class RequestModelValidator : ActionFilterAttribute
    {
        /// <summary>
        /// Occured before the action method is invoked
        /// </summary>
        /// <param name="actionContext">HttpActionContext value</param>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext != null)
            {
                ValidRequestModel(actionContext);
            }
        }

        private void ValidRequestModel(ActionExecutingContext actionContext)
        {
            // Validate ModelState
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Result =  new ResultModel(HttpStatusCode.BadRequest, "Invalid request");
            }
        }
    }
}
