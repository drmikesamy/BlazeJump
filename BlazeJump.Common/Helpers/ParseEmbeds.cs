
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace BlazeJump.Helpers
{
	public static class ParseEmbeds
	{
		public static Regex youtubeUrlPattern = new Regex(@"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static List<MarkupString> ParseEmbedsFromContent(string content)
		{
			var linkFinder = new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var htmlItems = new List<MarkupString>();
			foreach (Match m in linkFinder.Matches(content))
			{
				if (youtubeUrlPattern.IsMatch(m.Value))
				{
					htmlItems.Add(ParseVideoUrl(m.Value));
				}
			}
			return htmlItems;
		}
		public static MarkupString ParseVideoUrl(string input)
		{
			var uri = new Uri(input);

			if (uri.Host == "youtube.com" || uri.Host == "www.youtube.com" || uri.Host == "youtu.be")
			{
				var query = HttpUtility.ParseQueryString(uri.Query);
				var videoId = string.Empty;
				if (query.AllKeys.Contains("v"))
				{
					videoId = query["v"];
				}
				else
				{
					videoId = uri.Segments.Last();
				}

				return (MarkupString)$"""<iframe class="video-player" src="https://www.youtube.com/embed/{videoId}" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>""";
			}
			else if (uri.Host == "vimeo.com" || uri.Host == "www.vimeo.com")
			{
				var videoId = uri.Segments.Last();
				return (MarkupString)$"""<iframe class="video-player" src="https://player.vimeo.com/video/{videoId}" frameborder="0" allow="autoplay; fullscreen; picture-in-picture" allowfullscreen></iframe>""";
			}

			return (MarkupString)"""<div class="error"><p>Invalid Video URL</p></div>""";
		}

		public static MarkupString StringAsMarkup(string text)
		{
			return (MarkupString)text;
		}
	}
}
