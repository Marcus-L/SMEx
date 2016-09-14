using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace SMEx
{
	public class SmugMugHelper
	{
		public static string Username { get; private set; }

		internal static void VerifyOAuth(string oauth_token, string oauth_verifier)
		{
			_oauth_verifier = oauth_verifier;
			_mre.Set();
		}

		private static ManualResetEvent _mre = new ManualResetEvent(false);
		private static string _oauth_verifier = "";
		private static SmugmugOAuth.Manager _manager = null;

		public static bool Authenticate(string smugmug_key, string smugmug_secret)
		{
			// start OAuth response server
			using (var host = new WebHostBuilder()
				.UseKestrel()
				.UseUrls("http://127.0.0.1:0")
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.Build())
			{
				host.Start();

				// create redirect url for the OAuth response
				var serverUrl = new Uri(host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First());
				var redirectUrl = serverUrl.ToString() + "api/smugmug";

				// generate SmugMug OAuth request (OAuth 1.0a)
				_manager = new SmugmugOAuth.Manager(smugmug_key, smugmug_secret);
				_manager["callback"] = redirectUrl;
				var requestToken = _manager.AcquireRequestToken("https://api.smugmug.com/services/oauth/1.0a/getRequestToken", "GET");

				// start the browser for the user to approve us
				Process.Start("cmd.exe", $"/c start \"\" \"https://api.smugmug.com/services/oauth/authorize.mg?oauth_token={requestToken["oauth_token"]}&Access=Full&Permissions=Modify\"");

				// wait for the response
				_mre.WaitOne();
			}

			if (_oauth_verifier == null) return false; // if the user denied us or CTRL-C'd us

			_manager.AcquireAccessToken("https://api.smugmug.com/services/oauth/1.0a/getAccessToken", "GET", _oauth_verifier);
			if (_manager["token"] == "") return false; // verification code was bad

			// set username (used in other URLs)
			Username = ((dynamic)GetJson("/api/v2!authuser")).Response.User.NickName;

			return true;
		}

		private static HttpClient AuthenticatedClient(string uri, string method)
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("OAuth", _manager.GenerateAuthzHeader(uri, method));
			return client;
		}

		private static JObject GetJson(string api)
		{
			string url = $"https://api.smugmug.com{api}";
			using (var client = AuthenticatedClient(url, "GET"))
			{
				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));

				string json = client.GetStringAsync(url).Result;

				return JObject.Parse(json);
			}
		}

		public static byte[] Download(string url)
		{
			using (var client = AuthenticatedClient(url, "GET"))
			{
				return client.GetByteArrayAsync(url).Result;
			}
		}

		public static IEnumerable<SmugModel.Node> Folders
		{
			get
			{
				var folders = GetJson("/api/v2/folder/user/" + Username + "!folderlist");
				return folders["Response"]["FolderList"].Children()
					.Select(j => j.ToObject<SmugModel.Node>());
			}
		}

		public static IEnumerable<SmugModel.Node> Albums(SmugModel.Node folder)
		{
			var albums = GetJson(folder.Uri + "!albums");
			return albums["Response"]["Album"].Children()
				.Select(j => j.ToObject<SmugModel.Node>());
		}

		public static IEnumerable<SmugModel.Image> Images(SmugModel.Node album)
		{
			var images = GetJson(album.Uri + "!images");
			return images["Response"]["AlbumImage"].Children()
				.Select(j => j.ToObject<SmugModel.Image>());
		}
	}

	namespace SmugModel
	{
		public class Node
		{
			public string Uri { get; set; }
			public string UrlPath { get; set; }
			public string Name { get; set; }
		}

		public class Image : Node
		{
			public string FileName { get; set; }

			public string ArchivedUri { get; set; }

			public string ImageKey { get; set; }

			public string Format { get; set; }
		}
	}

}
