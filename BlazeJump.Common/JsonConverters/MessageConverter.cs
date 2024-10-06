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
					var contextFound = Enum.TryParse<MessageContextEnum>(eventSubscriptionId.Split('_')[0], true, out var messageContext);

					message.SubscriptionId = eventSubscriptionId.Split('_')[1];
					message.Event = evt;
					if (contextFound)
					{
						message.Context = messageContext;
						switch (messageContext)
						{
							case MessageContextEnum.User:
								message.Priority = 1;
								break;
							case MessageContextEnum.Event:
								message.Priority = 2;
								break;
							case MessageContextEnum.Reply:
								message.Priority = 3;
								break;
						}
					}

					break;
				case MessageTypeEnum.Count:
					var countSubscriptionId = ja[1].ToString();
					message.SubscriptionId = countSubscriptionId;
					message.Stats = JsonConvert.DeserializeObject<Stats>(ja[2].ToString());

					break;
				case MessageTypeEnum.Notice:
					var noticeMessage = ja[1].ToString();

					message.NoticeMessage = noticeMessage;

					break;
				case MessageTypeEnum.Eose:
					var eoseSubscriptionId = ja[1].ToString();
					var eoseContextFound = Enum.TryParse<MessageContextEnum>(eoseSubscriptionId.Split('_')[0], true, out var eoseMessageContext);

					message.SubscriptionId = eoseSubscriptionId.Split('_')[1];
					if (eoseContextFound)
					{
						message.Context = eoseMessageContext;
					}

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