using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMEx
{
    public class Program
    {
        public static void Main(string[] args)
        {
			// load config
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			Console.Write("Press Enter to Authenticate with SmugMug via OAuth: ");
			Console.ReadLine();

			if (!SmugMugHelper.Authenticate(config["smugmug_key"], config["smugmug_secret"]))
			{
				Console.WriteLine("Error authenticating with SmugMug.");
				return;
			}

			// see: http://stackoverflow.com/a/146162/490657 for reference
			string regexSearch = new string(Path.GetInvalidFileNameChars());
			Regex invalidChars = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

			foreach (var folder in SmugMugHelper.Folders)
			{
				Console.WriteLine("folder: " + folder.Name);
				Directory.CreateDirectory(config["folder"] + folder.UrlPath);
				foreach  (var album in SmugMugHelper.Albums(folder))
				{
					Console.WriteLine("> album: " + album.Name);
					Directory.CreateDirectory(config["folder"] + album.UrlPath);

					Parallel.ForEach(SmugMugHelper.Images(album), new ParallelOptions { MaxDegreeOfParallelism = 4 }, image =>
					{
						// sometimes the file name is missing, so we'll use the key & format to name the file.
						string file = (image.FileName == "" ? $"{image.ImageKey}.{image.Format}" : image.FileName);

						// sometimes the file name is invalid, so we need to fix it
						file = invalidChars.Replace(file, "_");

						Console.WriteLine("  > image: " + file);
						string filename = config["folder"] + album.UrlPath + "/" + file;
						if (!File.Exists(filename))
						{
							try
							{
								File.WriteAllBytes(filename, SmugMugHelper.Download(image.ArchivedUri));
							}
							catch (Exception ex)
							{
								Console.Error.WriteLine($"Error while downloading: {file}: {ex.Message}");
							}
						}
					});
				}
			}
			Console.ReadLine();
		}
    }
}
