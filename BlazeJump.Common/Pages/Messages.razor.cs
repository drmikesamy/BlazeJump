using BlazeJump.Common.Builders;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using Microsoft.AspNetCore.Components;
using System.Reflection.Metadata.Ecma335;
using BlazeJump.Common.Helpers;

namespace BlazeJump.Common.Pages
{
    public partial class Messages
    {
        [Parameter] public string? RootId { get; set; }
        private string? HexRootId => GeneralHelpers.NpubToHex(RootId);
        private string _searchString { get; set; } = string.Empty;
        private string _messageToSend { get; set; } = string.Empty;
        protected override async Task OnParametersSetAsync()
        {
            NotificationService.Loading = true;
            if (HexRootId != null)
            {
                await MessageService.FetchPage(HexRootId);
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