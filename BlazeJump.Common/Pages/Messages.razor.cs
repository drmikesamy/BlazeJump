using BlazeJump.Common.Builders;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using Microsoft.AspNetCore.Components;
using System.Reflection.Metadata.Ecma335;

namespace BlazeJump.Common.Pages
{
	public partial class Messages
	{
		[Parameter]
		public string? PageType { get; set; }
		public PageTypeEnum? PageTypeParsed { get; set; }
		[Parameter]
		public string? Hex { get; set; }
		private string _searchString { get; set; } = string.Empty;
		protected override async Task OnParametersSetAsync()
		{
			NotificationService.Loading = true;
			PageTypeEnum pageType;
			var pageTypeLoaded = Enum.TryParse(PageType, true, out pageType);
			if (pageTypeLoaded)
			{
				PageTypeParsed = pageType;
				await MessageService.FetchPage(Hex, pageType, true);
			}
			NotificationService.Loading = false;
			NotificationService.UpdateState += UpdateState;
		}

		private void UpdateState(object sender, EventArgs e)
		{
			StateHasChanged();
		}

		private void UpdateSearch()
		{
			_ = MessageService.LookupUser(_searchString);
		}
	}
}