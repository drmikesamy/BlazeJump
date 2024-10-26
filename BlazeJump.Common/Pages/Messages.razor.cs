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
		public DateTime UntilMarker { get; set; } = DateTime.Now.AddDays(1);
		protected override async Task OnParametersSetAsync()
		{
			NotificationService.Loading = true;
			PageTypeEnum pageType;
			var pageTypeLoaded = Enum.TryParse(PageType, true, out pageType);
			if (pageTypeLoaded)
			{
				PageTypeParsed = pageType;
				await Load(true);
			}
			NotificationService.Loading = false;
			NotificationService.UpdateState += UpdateState;
			MessageService.EndOfFetchNotification += EndOfFetch;
		}
		public async Task Load(bool initialLoad = false)
		{
			FilterBuilder filters = new();
			if (initialLoad)
			{
				SetFeatureFilter(ref filters);
			}
			SetBodyFilter(ref filters);
			await FetchPosts(filters.Build());
		}
		private void SetFeatureFilter(ref FilterBuilder filters)
		{
			if (PageTypeParsed == PageTypeEnum.Event)
			{
				filters
					.AddFilter()
					.AddKind(KindEnum.Text)
					.AddEventId(Hex);
			}
		}
		private void SetBodyFilter(ref FilterBuilder filters)
		{
			if (PageTypeParsed == PageTypeEnum.Event)
			{
				filters
					.AddFilter()
					.AddKind(KindEnum.Text)
					.WithToDate(UntilMarker)
					.WithLimit(5)
					.AddTaggedEventId(Hex);
			}
			else
			{
				filters
					.AddFilter()
					.AddKind(KindEnum.Text)
					.WithToDate(UntilMarker)
					.WithLimit(5)
					.AddAuthor(Hex);
			}	
		}
		private void SetUserFilter(ref FilterBuilder filters, List<string> pubkeys)
		{
			if (pubkeys.Count() == 0)
				return;
			filters
				.AddFilter()
					.AddKind(KindEnum.Metadata)
					.AddAuthors(pubkeys.Distinct().ToList());
		}
		private void SetTextEventFilter(ref FilterBuilder filters, List<string> textEventIds)
		{
			if (textEventIds.Count() == 0)
				return;
			filters
				.AddFilter()
					.AddKind(KindEnum.Text)
					.AddEventIds(textEventIds.Distinct().ToList());
		}
		private void SetReplyFilter(ref FilterBuilder filters, List<string> parentEventIds)
		{
			if (parentEventIds.Count() == 0)
				return;
			filters
				.AddFilter()
					.AddKind(KindEnum.Text)
					.AddTaggedEventIds(parentEventIds.Distinct().ToList());
		}
		private async Task FetchPosts(List<Filter> filters, bool isRelatedData = false)
		{
			if (filters.All(f => f.TaggedEventIds?.Count() > 0 || f.Authors?.Count() > 0 || f.EventIds.Count() > 0))
			{
				var fetchId = Guid.NewGuid().ToString();
				if (!isRelatedData)
					MessageService.TopLevelFetchRegister.Add(fetchId, new List<string>());
				await MessageService.Fetch(filters, fetchId);
			}
		}
		private void UpdateState(object? o, EventArgs e)
		{
			StateHasChanged();
		}
		private void EndOfFetch(object? o, string subscriptionId)
		{
			if (MessageService.TopLevelFetchRegister.TryGetValue(subscriptionId, out var eventIds))
			{
				var messages = eventIds.Select(id => MessageService.MessageStore[id]).ToList();
				UntilMarker = messages.LastOrDefault()?.Event?.CreatedAtDateTime.AddMilliseconds(-1) ?? DateTime.Now.AddDays(1);

				FilterBuilder filterBuilder = new();

				SetReplyFilter(ref filterBuilder, eventIds);
				SetUserFilter(ref filterBuilder, messages.Select(m => m.Event.UserId).Distinct().ToList());
				if(MessageService.RelationRegister.TryGetRelations(eventIds, FetchTypeEnum.TaggedParentIds, out var childEventIdsReply))
				{
					SetTextEventFilter(ref filterBuilder, childEventIdsReply);
				}
				if (MessageService.RelationRegister.TryGetRelations(eventIds, FetchTypeEnum.TaggedRootId, out var childEventIdsRoot))
				{
					SetTextEventFilter(ref filterBuilder, childEventIdsRoot);
				}
				var filters = filterBuilder.Build();
				if (filters.Count() > 0)
				{
					_ = FetchPosts(filters, true);
				}
			}
			StateHasChanged();
		}
	}
}