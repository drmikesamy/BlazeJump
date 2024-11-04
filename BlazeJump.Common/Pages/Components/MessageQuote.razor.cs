using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using Microsoft.AspNetCore.Components;

namespace BlazeJump.Common.Pages.Components
{
    public partial class MessageQuote
    {
        [Parameter]
        public NMessage Message { get; set; }
        public NEvent NEvent => Message.Event;
        public User User => MessageService.MessageStore.TryGetValue(NEvent.UserId, out var user) ? user.Event.User : new User();
        public NMessage TaggedReply => MessageService.RelationRegister.TryGetRelations(new List<string> { NEvent.Id }, FetchTypeEnum.TaggedParentIds, out var replies) ? replies.Select(id => MessageService.MessageStore.ContainsKey(id) ? MessageService.MessageStore[id] : new NMessage()).FirstOrDefault() : null;
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