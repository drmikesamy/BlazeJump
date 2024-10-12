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
			List<Filter> filters = new();
			if (initialLoad)
			{
				SetFeatureFilter(ref filters);
			}
			SetBodyFilter(ref filters);
			await FetchPosts(filters);
		}
		private void SetFeatureFilter(ref List<Filter> filters)
		{
			if (PageTypeParsed == PageTypeEnum.Event)
			{
				filters.Add(new Filter
				{
					Kinds = new int[] { (int)KindEnum.Text },
					Since = DateTime.Now.AddYears(-20),
					Until = DateTime.Now.AddDays(1),
					Limit = 1,
					EventIds = new List<string> { Hex },
				});
			}
		}
		private void SetBodyFilter(ref List<Filter> filters)
		{
			filters.Add(new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = UntilMarker,
				Limit = 5,
				TaggedEventIds = PageTypeParsed == PageTypeEnum.Event ? new List<string> { Hex } : null,
				Authors = PageTypeParsed == PageTypeEnum.Event ? null : new List<string> { Hex }
			});
		}
		private void SetUserFilter(ref List<Filter> filters, List<string> pubkeys)
		{
			if (pubkeys.Count() == 0)
				return;
			filters.Add(new Filter
			{
				Kinds = new int[] { (int)KindEnum.Metadata },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now.AddDays(1),
				Authors = pubkeys.Distinct().ToList()
			});
		}
		private void SetReplyFilter(ref List<Filter> filters, List<string> parentEventIds)
		{
			if (parentEventIds.Count() == 0)
				return;
			filters.Add(new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now.AddDays(1),
				TaggedEventIds = parentEventIds.Distinct().ToList()
			});
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

				List<Filter> filters = new();
				SetUserFilter(ref filters, MessageService.RelationRegister.GetRelations(eventIds, KindEnum.Metadata).ToList());
				SetReplyFilter(ref filters, MessageService.RelationRegister.GetRelations(eventIds, KindEnum.Text).ToList());
				if (filters.Count() > 0)
				{
					_ = FetchPosts(filters, true);
				}
			}
			StateHasChanged();
		}
	}
}