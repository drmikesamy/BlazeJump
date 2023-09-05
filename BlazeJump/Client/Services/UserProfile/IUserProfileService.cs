using BlazeJump.Client.Models;
using BlazeJump.Client.Models.SubtleCrypto;

namespace BlazeJump.Client.Services.UserProfile
{
    public interface IUserProfileService
    {
        RSAKeyPair RSAKeys { get; set; }
        User User { get; set; }
        Dictionary<string, User> UserList { get; set; }
		string NPubKey { get; set; }
		bool IsLoggedIn { get; set; }
        Task Init();
		void ChangeProfilePicture(string imageUrl);
        void SaveChanges(User user);
        Task Login();
        Task Logout();
        Task Register();
    }
}