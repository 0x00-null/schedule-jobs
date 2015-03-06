using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using EventReminder.PlatformService;

namespace EventReminder.Utils
{
    class PlatformUtils
    {
        public static T Call<T>(String token, Func<PlatformServiceClient, T> ministryPlatformFunc)
        {
            T result;
            var platformServiceClient = new PlatformServiceClient();
            using (new OperationContextScope((IClientChannel)platformServiceClient.InnerChannel))
            {
                if (WebOperationContext.Current != null)
                    WebOperationContext.Current.OutgoingRequest.Headers.Add("Authorization", "Bearer " + token);
                result = ministryPlatformFunc(platformServiceClient);
            }
            return result;
        }

        public static void VoidCall(String token, Action<PlatformServiceClient> ministryPlatformFunc)
        {
            var platformServiceClient = new PlatformServiceClient();
            using (new OperationContextScope((IClientChannel)platformServiceClient.InnerChannel))
            {
                if (WebOperationContext.Current != null)
                    WebOperationContext.Current.OutgoingRequest.Headers.Add("Authorization", "Bearer " + token);
                ministryPlatformFunc(platformServiceClient);
            }
        }
    }
}
