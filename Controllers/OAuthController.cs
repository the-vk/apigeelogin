using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ApigeeLogin.Code;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using OAuthAuthorizationServer.Code;
using OAuthAuthorizationServer.Models;

namespace ApigeeLogin.Controllers
{
	public class OAuthController : Controller
	{
		private static readonly AuthorizationServer AuthorizationServer = new AuthorizationServer(new OAuth2AuthorizationServer());

		/// <summary>
		/// The OAuth 2.0 token endpoint.
		/// </summary>
		/// <returns>The response to the Client.</returns>
		public ActionResult Token()
		{
            var result = AuthorizationServer.HandleTokenRequest(this.Request);
            return result.AsActionResultMvc5();
		}

		/// <summary>
		/// Prompts the user to authorize a client to access the user's private data.
		/// </summary>
		/// <returns>The browser HTML response that prompts the user to authorize the client.</returns>
		public ActionResult Authorize()
		{
			var pendingRequest = AuthorizationServer.ReadAuthorizationRequest();
			if (pendingRequest == null)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

            // Consider auto-approving if safe to do so.
            if (((OAuth2AuthorizationServer)AuthorizationServer.AuthorizationServerServices).CanBeAutoApproved(pendingRequest))
            {
                var approval = AuthorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, "email@example.com");
                var response = AuthorizationServer.Channel.PrepareResponse(approval);
                var redirectParams = System.Web.HttpUtility.ParseQueryString(new Uri(response.Headers["Location"]).Query);
                var code = redirectParams["Code"];
                var client_id = pendingRequest.ClientIdentifier;
                var request = WebRequest.Create($"http://apigeepoc.westus2.cloudapp.azure.com:9001/oauth/registerauth?code={code}&client_id={client_id}");
                var apigeeResponse = request.GetResponse();
                apigeeResponse.Close();
                return response.AsActionResultMvc5();
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Processes the user's response as to whether to authorize a Client to access his/her private data.
		/// </summary>
		/// <param name="isApproved">if set to <c>true</c>, the user has authorized the Client; <c>false</c> otherwise.</param>
		/// <returns>HTML response that redirects the browser to the Client.</returns>
		public ActionResult AuthorizeResponse(bool isApproved)
		{
			var pendingRequest = AuthorizationServer.ReadAuthorizationRequest();
			if (pendingRequest == null)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			IDirectedProtocolMessage response;
			if (isApproved)
			{
				// In this simple sample, the user either agrees to the entire scope requested by the client or none of it.  
				// But in a real app, you could grant a reduced scope of access to the client by passing a scope parameter to this method.
				response = AuthorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
			}
			else
			{
				response = AuthorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
				//var errorResponse = response as EndUserAuthorizationFailedResponse;
				//if (errorResponse != null) {
				//    errorResponse.Error = "accesss_denied";  // see http://tools.ietf.org/id/draft-ietf-oauth-v2-31.html#rfc.section.4.1.2.1 for valid values
				//    errorResponse.ErrorDescription = "The resource owner or authorization server denied the request";
				//}
			}

			return AuthorizationServer.Channel.PrepareResponse(response).AsActionResult();
		}
	}
}
