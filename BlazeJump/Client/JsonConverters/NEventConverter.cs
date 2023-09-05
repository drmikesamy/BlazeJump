using BlazeJump.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazeJump.Client.Enums
{
	class NEventConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(NMessage));
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JArray ja = new JArray();
			NEvent nEvent = (NEvent)value;

			ja.Add(nEvent.Id);
			ja.Add(nEvent.Pubkey);
			ja.Add(nEvent.Created_At);
			ja.Add(nEvent.Kind);
			ja.Add(nEvent.Tags);
			ja.Add(nEvent.Content);
			ja.Add(nEvent.Sig);

			ja.WriteTo(writer);
		}
	}

}