using System;
using System.Collections.Generic;
using EventReminder.PlatformService;
using EventReminder.Utils;

namespace EventReminder.Services
{
    public class BaseService
    {
        protected static void UpdateRecord(int pageId, Dictionary<string, object> dictionary, String token)
        {
            PlatformUtils.VoidCall(token, platfromClient => platfromClient.UpdatePageRecord(pageId, dictionary, false));
        }

        protected static int CreatePageRecord(int pageId, Dictionary<string, object> dictionary, String token)
        {
            var recordId = PlatformUtils.Call(token, platformClient => platformClient.CreatePageRecord(pageId, dictionary, false));
            return recordId;
        }

        protected static int CreateSubPageRecord(int pageId, int parentRecordId, Dictionary<string, object> dictionary, String token)
        {
            var recordId = PlatformUtils.Call(token, platformClient => platformClient.CreateSubpageRecord(pageId, parentRecordId, dictionary, false));
            return recordId;
        }

        protected static List<Dictionary<string, object>> GetPageRecords(int pageId, string token, string searchString)
        {
            var platformResult = PlatformUtils.Call<SelectQueryResult>(token,
                platformClient => platformClient.GetPageRecords(pageId, searchString, string.Empty, 0));
            return MpFormatToList(platformResult);
        }

        protected static Dictionary<string, object> GetPageRecord(int pageId, string token, int recordId)
        {
            var platformResult = PlatformUtils.Call<SelectQueryResult>(token,
                platformClient => platformClient.GetPageRecord(pageId,recordId, false));
            return MpFormatToDictionary(platformResult);
        }

        protected static List<Dictionary<string, object>> GetSubPageRecords(int subPageId, int recordId, String token)
        {
            var platformResult = PlatformUtils.Call<SelectQueryResult>(token,
                platformClient => platformClient.GetSubpageRecords(subPageId, recordId, string.Empty, string.Empty, 0));

            return MpFormatToList(platformResult);
        }

        private static Dictionary<string, object> MpFormatToDictionary(SelectQueryResult mpObject)
        {
            var ret = new Dictionary<string, object>();
            foreach (var dataitem in mpObject.Data)
            {
                foreach (var mpField in mpObject.Fields)
                {
                    ret.Add(mpField.Name, dataitem[mpField.Index]);
                }
            }
            return ret;
        }


        private static List<Dictionary<string, object>> MpFormatToList(SelectQueryResult mpObject)
        {
            var list = new List<Dictionary<string, object>>();


            foreach (var dataitem in mpObject.Data)
            {
                var ret = new Dictionary<string, object>();
                foreach (var mpField in mpObject.Fields)
                {
                    ret.Add(mpField.Name, dataitem[mpField.Index]);
                }
                list.Add(ret);
            }
            return list;
        }
    }
}
