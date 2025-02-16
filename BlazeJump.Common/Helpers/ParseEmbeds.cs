
using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using System.Web;

namespace BlazeJump.Helpers
{
	public static class ParseEmbeds
	{
		public static Regex youtubeUrlPattern = new Regex(@"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public static Regex vimeoUrlPattern = new Regex(@"(http|https)?:\/\/(www\.|player\.)?vimeo\.com\/(?:channels\/(?:\w+\/)?|groups\/([^\/]*)\/videos\/|video\/|)(\d+)(?:|\/\?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		public static MarkupString ParseInlineEmbeds(this string content)
		{
			var linkFinder = new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			foreach (Match m in linkFinder.Matches(content))
			{
				content = content.Replace(m.Value, $"<a href=\"{m.Value}\" target=\"_blank\">{m.Value}</a>");
			}
			foreach (var bech32ByType in ParseEmbeddedStrings(content))
			{
				foreach(var bech32 in bech32ByType.Value)
				{
					content = content.Replace($"nostr:{bech32}", "");
				}
			}
			return (MarkupString)content;
		}
		public static List<MarkupString> ParsePreviewContent(string content)
		{
			var linkFinder = new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var htmlItems = new List<MarkupString>();
			foreach (Match m in linkFinder.Matches(content))
			{
				if (youtubeUrlPattern.IsMatch(m.Value))
				{
					htmlItems.Add(ParseVideoUrl(m.Value));
				}
				if (vimeoUrlPattern.IsMatch(m.Value))
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
		public static Dictionary<string, List<string>> ParseEmbeddedStrings(string input)
		{
			var result = new Dictionary<string, List<string>>();
			var regex = new Regex(@"nostr:(nevent[\w]+|nprofile[\w]+)");

			foreach (Match match in regex.Matches(input))
			{
				string fullString = match.Value.Split(':')[1];
				string key = fullString.Substring(0, 7);

				if (!result.ContainsKey(key))
				{
					result[key] = new List<string>();
				}

				result[key].Add(fullString);
			}

			return result;
		}

		public static MarkupString StringAsMarkup(string text)
		{
			return (MarkupString)text;
		}
	}
}
