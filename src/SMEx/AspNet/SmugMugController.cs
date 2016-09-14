using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMEx.Controllers
{
	[Route("api/[controller]")]
    public class SmugMugController : Controller
    {
		[HttpGet]
		public ContentResult Go([FromQuery]string oauth_token, [FromQuery]string oauth_verifier)
		{
			Task.Delay(1000).ContinueWith(task =>
			{
				// send the access token back to the app after a second to allow the req to finish
				SmugMugHelper.VerifyOAuth(oauth_token, oauth_verifier);
			});

			// tell the user to close the window 
			// (can't do this in javascript:window.close() due to browser security)
			var retval = new ContentResult { ContentType = "text/html" };
			retval.Content = "<html>SmugMug authorization complete, this window may be closed.</html>";

			return retval;
		}
	}
}
