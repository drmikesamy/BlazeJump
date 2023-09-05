using BlazeJump.Client.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazeJump.Client.Models
{
	//[JsonConverter(typeof(NEventConverter))]
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
    }

}