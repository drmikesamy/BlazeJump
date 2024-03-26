using BlazeJump.Common.Models;
using BlazeJump.Common.Models.Crypto;

namespace BlazeJump.Common.Services.UserProfile
{
    public interface IUserProfileService
    {
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