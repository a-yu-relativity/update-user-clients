using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace UpdateClientOnUsers
{
    public static class Users
    {
        /// <summary>
        /// Update single user's client
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="userId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static bool UpdateClientForUser(IRSAPIClient proxy, int userId, int clientId)
        {
            proxy.APIOptions.WorkspaceID = -1;

            var userToUpdate = new kCura.Relativity.Client.DTOs.User(userId)
            {
                Client = new Client(clientId)
            };

            WriteResultSet<kCura.Relativity.Client.DTOs.User> resultSet = null;
            try
            {
                proxy.Repositories.User.UpdateSingle(userToUpdate);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
