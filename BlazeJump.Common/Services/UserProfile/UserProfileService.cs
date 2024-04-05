using BlazeJump.Common.Models;
using BlazeJump.Common.Models.Crypto;
using BlazeJump.Common.Services.Crypto;

namespace BlazeJump.Common.Services.UserProfile
{
    public class UserProfileService : IUserProfileService
    {
        private ICryptoService _cryptoService;
        public UserProfileService(ICryptoService cryptoService) {
            _cryptoService = cryptoService;
        }
        public User User { get; set; } = new User();
		public bool IsLoggedIn { get; set; }
		public Dictionary<string, User> UserList { get; set; } = new Dictionary<string, User>();
        public string NPubKey { get; set; } = string.Empty;

		public async Task Init()
        {
        }

        public void ChangeProfilePicture(string imageUrl)
        {
            throw new NotImplementedException();
        }

        public Task Login()
        {
            throw new NotImplementedException();
        }

        public Task Logout()
        {
            throw new NotImplementedException();
        }

        public Task Register()
        {
            throw new NotImplementedException();
        }
    }
}
