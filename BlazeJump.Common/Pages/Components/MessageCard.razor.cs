using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using Microsoft.AspNetCore.Components;

namespace BlazeJump.Common.Pages.Components
{
	public partial class MessageCard
	{
		[Parameter]
		public NMessage Message { get; set; }

		[Parameter] public bool Featured { get; set; } = false;
		public NEvent NEvent => Message.Event;
		public User User => MessageService.MessageStore.TryGetValue(NEvent.UserId, out var user) ? user.Event.User : new User();
		public void ViewUser()
		{
			NavManager.NavigateTo($"{NEvent.UserId}", true);
		}
		public void ViewMessage()
		{
			NavManager.NavigateTo($"{NEvent.Id}", true);
		}
		public void VerifyMessage(NEvent message)
		{
			message.Verified = MessageService.Verify(NEvent);
			StateHasChanged();
		}
	}
}