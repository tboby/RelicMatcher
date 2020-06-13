using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RelicMatcher.Server.Service
{
    public class ConnectionSessionService
    {
        private static readonly Dictionary<string, Guid> ConnectionSessionDictionary = new Dictionary<string, Guid>();

        public void SetUserForConnection(string connectionID, Guid userSessionGuid)
        {
            ConnectionSessionDictionary[connectionID] = userSessionGuid;
        }
        public Guid GetUserForConnection(string connectionID)
        {
            return ConnectionSessionDictionary[connectionID];
            //if (ConnectionSessionDictionary.TryGetValue(connectionID, out var value))
            //{
            //    return value;
            //}
            //else
            //{
            //    return null;
            //}
        }

        public List<string> GetAllConnectionsForUser(Guid userGuid)
        {
            return ConnectionSessionDictionary.Where(x => x.Value == userGuid)
                .Select(x => x.Key)
                .ToList();
        }
    }
}
