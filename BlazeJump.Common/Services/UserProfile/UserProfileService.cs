using BlazeJump.Common.Models;
using BlazeJump.Common.Models.Crypto;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Database;

namespace BlazeJump.Common.Services.UserProfile
{
    public class UserProfileService : IUserProfileService
    {
        private ICryptoService _cryptoService;
        private IBlazeDbService _dbService;
        public UserProfileService(ICryptoService cryptoService, IBlazeDbService dbService) {
            _cryptoService = cryptoService;
            _dbService = dbService;
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

        public void SaveChanges(User user)
        {
            var existingRecord = _dbService.Context.Set<User>().Select(u => u.Id);
            if (existingRecord != null)
            {
				_dbService.Context.Set<User>().Add(user);
            }
            else
            {
				_dbService.Context.Set<User>().Update(user);
			}
        }
    }
}
