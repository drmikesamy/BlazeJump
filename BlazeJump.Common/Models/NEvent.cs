using BlazeJump.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace BlazeJump.Common.Models
{
	[JsonConverter(typeof(NEventConverter))]
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class NEvent
    {
        public string Id { get; set; }
        public string? Pubkey { get; set; }
		public long Created_At { get; set; }
		public KindEnum? Kind { get; set; }
        public List<EventTag>? Tags { get; set; } = new List<EventTag>();
        public string? Content { get; set; }
        public string? Sig { get; set; }

		[JsonIgnore]
		public User? User { get; set; }
		[JsonIgnore]
		public DateTime CreatedAtDateTime => Helpers.UnixTimeStampToDateTime(Created_At).ToLocalTime();
		[JsonIgnore]
        public string? RootNEventId
        {
            get
            {
                if (Tags.Count(x => x.Key == Enums.TagEnum.e) > 1)
                {
                    return Tags.FirstOrDefault(t => t.Key == Enums.TagEnum.e)?.Value;
                }
                return null;
            }
        }
        [JsonIgnore]
        public string? ParentNEventId
        {
            get
            {
                return Tags.LastOrDefault(t => t.Key == Enums.TagEnum.e)?.Value;
			}
            set
            {
                if(value != null)
                    Tags.Add(new EventTag { Key = Enums.TagEnum.e, Value = value });
            }
        }
        [JsonIgnore]
        public virtual NEvent? ParentNEvent { get; set; }
        [JsonIgnore]
        public virtual List<NEvent> Replies { get; set; } = new List<NEvent>();
        [JsonIgnore]
        public bool RepliesLoaded { get; set; } = false;
        [JsonIgnore]
        [InverseProperty(nameof(Reactions))]
		public string? ReactionId
		{
			get
			{
                if(Kind == KindEnum.Reaction)
                {
					return Tags.LastOrDefault(t => t.Key == Enums.TagEnum.e)?.Value;
				}
                return null;
			}
			set
			{
				if (value != null)
					Tags.Add(new EventTag { Key = Enums.TagEnum.e, Value = value });
			}
		}
		[JsonIgnore]
		public List<NEvent> Reactions { get; set; } = new List<NEvent>();
        [JsonIgnore]
        public int ReplyCount => Replies.Count;
        [JsonIgnore]
        public int ReactionCount { get; set; } = 0;
        [JsonIgnore]
        public bool Verified { get; set; } = false;
        public NEvent GetSignableNEvent()
        {
			return new NEvent
            {
                Pubkey = this.Pubkey,
                Created_At = this.Created_At,
                Kind = this.Kind,
                Tags = this.Tags,
                Content = this.Content,
            };
        }
    }

}