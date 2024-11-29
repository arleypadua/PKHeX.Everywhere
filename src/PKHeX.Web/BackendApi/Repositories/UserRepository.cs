using System.Runtime.Caching;
using PKHeX.Web.BackendApi.Representation;
using PKHeX.Web.Services.Auth;

namespace PKHeX.Web.BackendApi.Repositories;

public class UserRepository(
    AuthService auth)
{
    private readonly MemoryCache _cache = MemoryCache.Default;
    
    public async Task<UserRepresentation> GetSignedInUser()
    {
        if (_cache.Get(SignedInUserKey) is UserRepresentation user)
            return user;
        
        var firebaseUser = await auth.GetSignedInUser();
        user = new UserRepresentation
        {
            Id = firebaseUser.Id,
            
            // TODO: make it a backend feature
            SyncQuota = DefaultSyncQuota,
            IsAdmin = false
        };
        
        _cache.Set(SignedInUserKey, user, DateTimeOffset.Now.AddMinutes(5));
        
        return user;
    }
    
    private const int DefaultSyncQuota = 6;
    private const string SignedInUserKey = "signed-in-user";
}