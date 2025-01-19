using NUnit.Framework;
using BlazeJump.Helpers;
using Microsoft.AspNetCore.Components;

namespace BlazeJump.Common.Tests.HelperTests
{
	[TestFixture]
	public class ParseEmbedsTests
	{
		[Test]
		public void ParseInlineEmbeds_ValidUrls_ReturnsAnchorTags()
		{
			// Arrange
			string input = "Check this link: https://example.com and https://another-link.com";

			// Act
			var result = input.ParseInlineEmbeds();

			// Assert
			var expected = "Check this link: <a href=\"https://example.com\" target=\"_blank\">https://example.com</a> and <a href=\"https://another-link.com\" target=\"_blank\">https://another-link.com</a>";
			Assert.That(result.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void ParsePreviewContent_ValidYoutubeLink_ReturnsEmbeddedIframe()
		{
			// Arrange
			string input = "Check out this video: https://www.youtube.com/watch?v=dQw4w9WgXcQ";

			// Act
			var result = ParseEmbeds.ParsePreviewContent(input);

			// Assert
			var expected = "<iframe class=\"video-player\" src=\"https://www.youtube.com/embed/dQw4w9WgXcQ\" title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" allowfullscreen></iframe>";
			Assert.That(result[0].ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void ParsePreviewContent_ValidVimeoLink_ReturnsEmbeddedIframe()
		{
			// Arrange
			string input = "Check out this Vimeo video: https://vimeo.com/123456789";

			// Act
			var result = ParseEmbeds.ParsePreviewContent(input);

			// Assert
			var expected = "<iframe class=\"video-player\" src=\"https://player.vimeo.com/video/123456789\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen></iframe>";
			Assert.That(result[0].ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void ParsePreviewContent_InvalidVideoUrl_ReturnsErrorMessage()
		{
			// Arrange
			string input = "Check this out: https://invalid-url.com";

			// Act
			var result = ParseEmbeds.ParsePreviewContent(input);

			// Assert
			Assert.That(result.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ParseVideoUrl_ValidYoutubeUrl_ReturnsIframe()
		{
			// Arrange
			string input = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

			// Act
			var result = ParseEmbeds.ParseVideoUrl(input);

			// Assert
			var expected = "<iframe class=\"video-player\" src=\"https://www.youtube.com/embed/dQw4w9WgXcQ\" title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" allowfullscreen></iframe>";
			Assert.That(result.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void ParseVideoUrl_ValidVimeoUrl_ReturnsIframe()
		{
			// Arrange
			string input = "https://vimeo.com/123456789";

			// Act
			var result = ParseEmbeds.ParseVideoUrl(input);

			// Assert
			var expected = "<iframe class=\"video-player\" src=\"https://player.vimeo.com/video/123456789\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen></iframe>";
			Assert.That(result.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void ParseVideoUrl_InvalidVideoUrl_ReturnsErrorMessage()
		{
			// Arrange
			string input = "https://invalid-video-url.com";

			// Act
			var result = ParseEmbeds.ParseVideoUrl(input);

			// Assert
			var expected = "<div class=\"error\"><p>Invalid Video URL</p></div>";
			Assert.That(result.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void StringAsMarkup_ReturnsStringAsMarkupString()
		{
			// Arrange
			string input = "Hello, world!";

			// Act
			var result = ParseEmbeds.StringAsMarkup(input);

			// Assert
			Assert.That(result.ToString(), Is.EqualTo(input));
		}
	}
}
