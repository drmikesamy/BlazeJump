using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using Microsoft.AspNetCore.Components;

namespace BlazeJump.Common.Pages.Components
{
	public partial class MessageCard
	{
		[Parameter]
		public NMessage Message { get; set; }
		public NEvent NEvent => Message.Event;
		public User User => MessageService.MessageStore.TryGetValue(NEvent.UserId, out var user) ? user.Event.User : new User();
		public List<NEvent> Children => MessageService.RelationRegister.TryGetRelations(new List<string> { NEvent.Id }, FetchTypeEnum.Replies, out var replies) ? replies.Select(id => MessageService.MessageStore[id].Event).ToList() : new List<NEvent>();
		public NMessage TaggedReply => MessageService.RelationRegister.TryGetRelations(new List<string> { NEvent.Id }, FetchTypeEnum.TaggedParentIds, out var replies) ? replies.Select(id => MessageService.MessageStore.ContainsKey(id) ? MessageService.MessageStore[id] : new NMessage()).FirstOrDefault() : null;
		public NMessage TaggedRoot => MessageService.RelationRegister.TryGetRelations(new List<string> { NEvent.Id }, FetchTypeEnum.TaggedRootId, out var replies) ? replies.Select(id => MessageService.MessageStore.ContainsKey(id) ? MessageService.MessageStore[id] : new NMessage()).FirstOrDefault() : null;
		public void ViewUser()
		{
			NavManager.NavigateTo($"user/{NEvent.UserId}", true);
		}
		public void ViewMessage()
		{
			NavManager.NavigateTo($"event/{NEvent.Id}", true);
		}
		public void VerifyMessage(NEvent message)
		{
			message.Verified = MessageService.Verify(NEvent);
			StateHasChanged();
		}
	}
}