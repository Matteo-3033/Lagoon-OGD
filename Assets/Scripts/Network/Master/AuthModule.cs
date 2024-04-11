using System.Threading.Tasks;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;

namespace Network.Master
{
    public class AuthModule : MasterServerToolkit.MasterServer.AuthModule
    {
        
        protected override async Task<IAccountInfoData> SignInWithLoginAndPassword(IPeer peer, MstProperties userCredentials, IIncomingMessage message)
        {
            // Trying to get user extension from peer
            var userPeerExtension = peer.GetExtension<IUserPeerExtension>();

            // If user peer has IUserPeerExtension means this user is already logged in
            if (userPeerExtension != null)
            {
                logger.Debug($"User {peer.Id} trying to login, but he is already logged in");
                message.Respond(ResponseStatus.Failed);
                return null;
            }

            // Get username
            var userName = userCredentials.AsString(MstDictKeys.USER_NAME);

            // Get user password
            var userPassword = userCredentials.AsString(MstDictKeys.USER_PASSWORD);

            // If another session found
            if (IsUserLoggedInByUsername(userName))
            {
                logger.Error($"Another user with {userName} is already logged in");
                message.Respond(ResponseStatus.Failed);
                return null;
            }

            // Get account by its username
            var userAccount = await authDatabaseAccessor.GetAccountByUsernameAsync(userName);

            if (userAccount == null)
            {
                userAccount = authDatabaseAccessor.CreateAccountInstance();
                userAccount.Username = userName;
                userAccount.IsGuest = false;
                userAccount.Password = Mst.Security.CreateHash(userPassword);

                // Let's set user email as confirmed if confirmation is not required by default
                userAccount.IsEmailConfirmed = true;

                _ = Task.Run(async () =>
                {
                    // Insert new account ot DB
                    await authDatabaseAccessor.InsertAccountAsync(userAccount);
                });
            }
            else if (!Mst.Security.ValidatePassword(userPassword, userAccount.Password))
            {
                logger.Error($"Invalid credentials for client {message.Peer.Id}");
                message.Respond(ResponseStatus.Invalid);
                return null;
            }

            // Let's save user auth token
            CreateAccountToken(userAccount);

            return userAccount;
        }

        protected override Task<IAccountInfoData> SignInWithEmail(IPeer peer, MstProperties userCredentials, IIncomingMessage message)
        {
            return null;
        }

        protected override Task<IAccountInfoData> SignInAsGuest(IPeer peer, MstProperties userCredentials, IIncomingMessage message)
        {
            return null;
        }
    }
}