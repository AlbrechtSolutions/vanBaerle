using Dna.Ecommerce.LiveIntegration.Configuration;
using Dna.Ecommerce.LiveIntegration.Extensions;
using Dna.Ecommerce.LiveIntegration.Logging;
using Dynamicweb.Extensibility.Notifications;
using Dynamicweb.Security.UserManagement;
using Dynamicweb.Security.UserManagement.Notifications;

namespace Dna.Ecommerce.LiveIntegration.NotificationSubscribers
{
    [Subscribe(Dynamicweb.Security.UserManagement.Notifications.Notifications.UserSaved)]
    public class UserSavedObserver : NotificationSubscriber
    {
        private static bool _isSaving;
        private static readonly object Lock = new object();

        public override void OnNotify(string notification, NotificationArgs args)
        {
            if (args == null
                || !(args is UserNotificationArgs)
                || _isSaving
                || !Global.IsIntegrationActive
                || !Connector.IsWebServiceConnectionAvailable()
                || !Global.IsUserSynchEnabled)
            {
                return;
            }

            var userArgs = (UserNotificationArgs)args;
            var user = userArgs.Subject;

            try
            {
                lock (Lock)
                {
                    _isSaving = true;
                    Logger.Instance.Log(ErrorLevel.DebugInfo, "UserSavedObserver: Call ERP! ExternalId = " + user.ExternalID);
                    user.SynchronizeUsingLiveIntegration(false);
                    user.Save();
                    Logger.Instance.Log(ErrorLevel.DebugInfo, "UserSavedObserver: After save! ExternalId = " + user.ExternalID);
                    _isSaving = false;
                }
            }
            catch (System.Exception anyError)
            {
                Logger.Instance.Log(ErrorLevel.DebugInfo, string.Format("Error sync the user: {0}", anyError));
            }
        }
    }
}

