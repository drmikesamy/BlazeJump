using BlazeJump.Common.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace BlazeJump.Common.Enums
{
	class TagConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(EventTag));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			EventTag tag = new EventTag();

			JArray ja = JArray.Load(reader);
			var jaLength = ja.Count();

			Enum.TryParse(ja[0]?.ToString(), true, out TagEnum tagType);

			tag.Key = tagType;
			if (jaLength >= 2)
				tag.Value = ja[1]?.ToString();
			if (jaLength >= 3)
				tag.Value2 = ja[2]?.ToString();
			if (jaLength >= 4)
				tag.Value3 = ja[3]?.ToString();
			if (jaLength >= 5)
				tag.Value4 = ja[4]?.ToString();

			return tag;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JArray ja = new JArray();
			EventTag tag = (EventTag)value;
			ja.Add(tag.Key.ToString());
			ja.Add(tag.Value ?? "");
			if(tag.Value2 != null)
			{
				ja.Add(tag.Value2);
			}
			if(tag.Value3 != null)
			{
				ja.Add(tag.Value3);
			}
			if(tag.Value4 != null)
			{
				ja.Add(tag.Value4);
			}
			ja.WriteTo(writer);
		}
	}

}