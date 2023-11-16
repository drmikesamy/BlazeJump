using BlazeJump.Common.Models.Crypto;
using System.ComponentModel.DataAnnotations;

namespace BlazeJump.Common.Models
{
	public class User
	{
		[Key]
		public string Id { get; set; }
		public string? Username { get; set; }
        public string? Bio { get; set; }
		public string? Email { get; set; }
		public string? Password { get; set; }
        public string? RepeatPassword { get; set; }
		public string? ProfilePic { get; set; }
		public string? Banner { get; set; }
		public ICollection<NEvent> Events { get; set; } = new List<NEvent>();
	}

}