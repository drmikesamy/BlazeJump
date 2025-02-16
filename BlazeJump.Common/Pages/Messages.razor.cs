using Microsoft.AspNetCore.Components;
using BlazeJump.Common.Helpers;
using BlazeJump.Common.Models;
using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Pages
{
	public partial class Messages
	{
		[Parameter] public string? RootId { get; set; }
		private string? HexRootId => GeneralHelpers.Bech32ToHex(RootId, Bech32PrefixEnum.npub);
		private string _searchString { get; set; } = string.Empty;
		private NEvent _nEventToSend { get; set; }
		protected override async Task OnParametersSetAsync()
		{
			NotificationService.Loading = true;
			if (HexRootId != null)
			{
				await MessageService.FetchPage(HexRootId);
				NewNEventToSend();
			}
			NotificationService.Loading = false;
			NotificationService.UpdateState += UpdateState;
		}

		private void UpdateState(object sender, EventArgs e)
		{
			StateHasChanged();
		}

		private void NewNEventToSend()
		{
			if (HexRootId != null
				&& _nEventToSend?.Content == null
				&& (MessageService.RelationRegister.TryGetRelation(HexRootId, RelationTypeEnum.EventRoot, out var eventRoot)
				|| MessageService.RelationRegister.TryGetRelation(HexRootId, RelationTypeEnum.EventParent, out var eventParent)))
			{
				_nEventToSend = MessageService.CreateNEvent(KindEnum.Text, String.Empty, HexRootId, eventRoot.SingleOrDefault());
			}
			StateHasChanged();
		}

		private void UpdateSearch()
		{
			_ = MessageService.LookupUser(_searchString);
		}
	}
}