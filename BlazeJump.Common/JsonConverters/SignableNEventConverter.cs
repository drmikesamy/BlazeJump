using BlazeJump.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace BlazeJump.Common.JsonConverters
{
	class SignableNEventConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(SignableNEvent));
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
			SignableNEvent nEvent = (SignableNEvent)value;

			ja.Add(0);
			if (!String.IsNullOrEmpty(nEvent.Pubkey))
			{
				ja.Add(nEvent.Pubkey);
			}
			if (nEvent.Created_At != 0)
			{
				ja.Add(nEvent.Created_At);
			}
			if (nEvent.Kind != null)
			{
				ja.Add(nEvent.Kind);
			}
			JArray tagListJa = new JArray();
			foreach (var tag in nEvent.Tags)
			{
				JArray tagJa = new JArray();
				tagJa.Add(tag.Key.ToString());
				tagJa.Add(tag.Value ?? "");
				if (tag.Value2 != null)
				{
					tagJa.Add(tag.Value2);
				}
				if (tag.Value3 != null)
				{
					tagJa.Add(tag.Value3);
				}
				if (tag.Value4 != null)
				{
					tagJa.Add(tag.Value4);
				}
				tagListJa.Add(tagJa);
			}
			ja.Add(tagListJa);
			if (!String.IsNullOrEmpty(nEvent.Content))
			{
				ja.Add(nEvent.Content);
			}

			ja.WriteTo(writer);
		}
	}

}