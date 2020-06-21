using BlazorBrowserStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelicMatcher.Client.ViewModels;

namespace RelicMatcher.Client
{
    public class UserStateService
    {
        private readonly ILocalStorage _localStorage;
        private readonly string _settingsKey = "userSettings";
        private readonly string _sessionKey = "userSession";

        public UserStateService(ILocalStorage localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<UserSettings> GetUserSettings()
        {
            try
            {
                return await _localStorage.GetItem<UserSettings>(_settingsKey);
            }
            catch (ArgumentNullException)
            {
                return null;
            }

        }

        public async Task SaveUserSettings(UserSettings userSettings)
        {
            await _localStorage.SetItem<UserSettings>(_settingsKey, userSettings);
        }

        public async Task<Guid> GetOrCreateUserSession()
        {
            try
            {
                return await _localStorage.GetItem<Guid>(_sessionKey);
            }
            catch (ArgumentNullException)
            {
                var newGuid = Guid.NewGuid();
                await _localStorage.SetItem<Guid>(_sessionKey, newGuid);
                return newGuid;
            }
        }
    }
}
