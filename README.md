# SMEx - A SmugMug Exporter
SMex is a full-account export tool to download all media files from SmugMug into a folder structure that roughly matches the URL folder/album structure. It was quick-n-dirtily written since my PictureLife images were imported into SmugMug when [PictureLife went out of business](http://www.theverge.com/2016/8/22/12587656/picturelife-shutdown-streamnation-jonathan-benassaya), and I only had a few days of free service to download all of my imported files.

Prerequisites
-------------
* Install .NET Core - https://www.microsoft.com/net/core
* Get a SmugMug API Key - https://api.smugmug.com/api/developer/apply (agree to the API 2.0 beta terms)
* Windows - although you could probably replace the [Process.Start code](https://github.com/Marcus-L/SMEx/blob/master/src/SMEx/SmugMugHelper.cs#L51) (I think that's the only Windows dependency) pretty easily to [run on Linux or MacOS](http://stackoverflow.com/a/2283716/490657).

Installation
-----
* Git Clone the repo - `git clone https://github.com/Marcus-L/SMEx.git`
* Update the [`appsettings.json`](/src/SMEx/appsettings.json) file with your API key/secret
* CD to the project dir - `cd smex\src\smex`
* dotnet commands: `dotnet restore` and `dotnet run`

*Note - there are no options, the tool downloads everything! If you stop it (or it crashes) part-way through, if you run it again it will skip any files already downloaded.*

![Screenshot](/src/SMEx/screenshot.png)

About
----
* .NET Core Console application that hosts ASP.Net via Kestrel to handle the OAuth response redirect automatically (although you still have to close the window since javascript can no longer do that)
* Uses OAuth 1.0a helper library from suntsu (Inital version by Dino Chiesa for [Cropper](http://cropperplugins.codeplex.com/)) - https://suntsu.ch/index.php?/archives/234-Authenticate-to-smugmug-with-oAuth-and-c.html since there's no built-in Oauth 1.0a helper in .NET Core (or any compatible nuget packages as of 9/2016) for OAuth 1.0a. Why doesn't SmugMug use OAuth 2.0 like everybody else? Geez.

License
-------
SMEx - A SmugMug Exporter

Copyright (c) 2016, Marcus Lum

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see <"http://www.gnu.org/licenses/":http://www.gnu.org/licenses/>.
