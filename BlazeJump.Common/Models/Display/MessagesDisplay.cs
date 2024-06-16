using BlazeJump.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeJump.Common.Models.Display
{
	public class MessagesDisplay
	{
		public NMessage PrimaryMessage { get; set; }
		public Dictionary<int, List<NMessage>> MessageBuckets { get; set; }
		public Dictionary<string, List<NMessage>> Replies { get; set; }
		public List<User> Users { get; set; }
	}
}
