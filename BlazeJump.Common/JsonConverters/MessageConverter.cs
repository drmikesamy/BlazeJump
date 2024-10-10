using BlazeJump.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazeJump.Common.Enums
{
	class MessageConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(NMessage));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			NMessage message = new NMessage();

			JArray ja = JArray.Load(reader);

			Enum.TryParse(ja[0]?.ToString(), true, out MessageTypeEnum messageType);

			message.MessageType = messageType;

			switch (messageType)
			{
				case MessageTypeEnum.Req:
					var reqSubscriptionId = ja[1].ToString();
					var filterJa = ja[2].ToString();
					var filter = JsonConvert.DeserializeObject<Filter>(filterJa);

					message.SubscriptionId = reqSubscriptionId;
					message.Filter = filter;

					break;
				case MessageTypeEnum.Event:
					var eventSubscriptionId = ja[1].ToString();
					var eventJa = ja[2].ToString();
					var evt = JsonConvert.DeserializeObject<NEvent>(eventJa);
					if(evt.Kind == KindEnum.Metadata)
					{
						evt.User = JsonConvert.DeserializeObject<User>(evt.Content);
					}
					message.SubscriptionId = eventSubscriptionId;
					message.Event = evt;

					break;
				case MessageTypeEnum.Count:
					var countSubscriptionId = ja[1].ToString();
					message.SubscriptionId = countSubscriptionId;

					break;
				case MessageTypeEnum.Notice:
					var noticeMessage = ja[1].ToString();

					message.NoticeMessage = noticeMessage;

					break;
				case MessageTypeEnum.Eose:
					message.SubscriptionId = ja[1].ToString();
					break;
				case MessageTypeEnum.Ok:
					var okEventId = ja[1].ToString();
					var okSuccess = ja[2].ToObject<bool>();
					var okMessage = ja[3].ToString();

					message.NEventId = okEventId;
					message.Success = okSuccess;
					message.NoticeMessage = okMessage;

					break;
				case MessageTypeEnum.Close:
					var closeSubscriptionId = ja[1].ToString();

					message.SubscriptionId = closeSubscriptionId;

					break;
			}

			return message;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JArray ja = new JArray();
			NMessage message = (NMessage)value;

			ja.Add(message.MessageType.ToString().ToUpperInvariant());

			switch (message.MessageType)
			{
				case MessageTypeEnum.Req:
					var filter = JsonConvert.SerializeObject(message.Filter);

					ja.Add(message.SubscriptionId);
					ja.Add(filter);

					break;
				case MessageTypeEnum.Event:
					var evt = JsonConvert.SerializeObject(message.Event);

					ja.Add(message.SubscriptionId);
					ja.Add(evt);

					break;
				case MessageTypeEnum.Notice:
					ja.Add(message.NoticeMessage);

					break;
				case MessageTypeEnum.Close:

					break;
			}

			ja.WriteTo(writer);
		}
	}

}