using BlazeJump.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace BlazeJump.Common.Enums
{
	class NEventConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(NEvent));
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

			//	[
			//  0,
			//  < pubkey, as a lowercase hex string>,
			//  < created_at, as a number >,
			//  < kind, as a number >,
			//  < tags, as an array of arrays of non - null strings >,
			//  < content, as a string>
			//]

			if (!String.IsNullOrEmpty(nEvent.Id))
			{
				ja.Add(nEvent.Id);
			}
			else
			{
				ja.Add(0);
			}
			if (!String.IsNullOrEmpty(nEvent.UserId))
			{
				ja.Add(nEvent.UserId);
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
			if (!String.IsNullOrEmpty(nEvent.Sig))
			{
				ja.Add(nEvent.Sig);
			}

			ja.WriteTo(writer);
		}
	}

}