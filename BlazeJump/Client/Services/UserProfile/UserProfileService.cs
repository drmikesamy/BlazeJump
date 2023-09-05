using BlazeJump.Client.Models;
using BlazeJump.Client.Models.SubtleCrypto;
using BlazeJump.Client.Services.Crypto;
using BlazeJump.Client.Services.Database;

namespace BlazeJump.Client.Services.UserProfile
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
        public RSAKeyPair RSAKeys { get; set; } = new();
		public Dictionary<string, User> UserList { get; set; } = new Dictionary<string, User>();
        public string NPubKey { get; set; }

		public async Task Init()
        {
            RSAKeys = await _cryptoService.GenerateRSAKeyPair();
            User.Id = RSAKeys.PublicKey.N;
            NPubKey = await _cryptoService.GetPublicKey();
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
