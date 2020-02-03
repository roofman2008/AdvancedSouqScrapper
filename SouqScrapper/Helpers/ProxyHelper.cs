using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace SouqScrapper.Helpers
{
    public static class ProxyHelper
    {
        private class ProxyPostModel
        {
            public string group { get; set; }
            public int rows { get; set; }
            public int skipped { get; set; }
            public int stringfails { get; set; }
            public JObject items { get; set; }
        }

        private class ProxyGetModel
        {
            public int finished { get; set; }
            public JObject items { get; set; }
            public int notchanged { get; set; }
            public int queued { get; set; }
            public int rows { get; set; }
            public int working { get; set; }
        }

        private class ProxyStatus
        {
            public long id { get; set; }
            public ProxyCapability result_http { get; set; }
            public ProxyCapability result_socks4 { get; set; }
            public ProxyCapability result_socks5 { get; set; }
            public ProxyCapability result_ssl { get; set; }
            public string time_http { get; set; }
            public string time_socks4 { get; set; }
            public string time_socks5 { get; set; }
            public string time_ssl { get; set; }
        }

        public class ProxyCapability
        {
            public string errstr { get; set; }
            public int status { get; set; }
        }

        private class ProxyConfiguration
        {
            public long id { get; set; }
            public string host { get; set; }
            public int port { get; set; }
        }

        private const string ResultUrl =
            "https://hidemy.name/api/checker.php?out=js&action=get&filters=progress!:queued;changed:1&fields=resolved_ip,progress,progress_http,progress_ssl,progress_socks4,progress_socks5,time_http,time_ssl,time_socks4,time_socks5,result_http,result_ssl,result_socks4,result_socks5&groups=[groupId]";

        private const string ActionUrl =
            "https://hidemy.name/api/checker.php?out=js&action=list_new&tasks=http,ssl,socks4,socks5&parser=lines";

        private static Dictionary<ProxyConfiguration, ProxyStatus> _lastAliveProxies = null;

        public static void VerifyProxies()
        {
            string proxiesString = @"202.21.115.94:44574
                                    123131241413123";

            var postModel = WebScrapper.PostDownloadJson<ProxyPostModel>(ActionUrl, null, new Dictionary<string, string>()
            {
                {"data", proxiesString}
            });

            var proxies = postModel.items.Values().Select(l =>
            {
                var obj = l.ToObject<ProxyConfiguration>();
                obj.id = Convert.ToInt64(l.Path);
                return obj;
            }).ToList();

            ProxyGetModel lastGetModel = null;
            ProxyStatus[] lastStatus = null;

            while (lastGetModel == null || lastGetModel.finished < postModel.rows)
            {
                JObject tmpResult = WebScrapper.GetDownloadJson(ResultUrl.Replace("[groupId]", postModel.group));

                tmpResult.TryGetValue("items", out JToken tmpOut);

                if (tmpOut.Type == JTokenType.Array)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                lastGetModel = tmpResult.ToObject<ProxyGetModel>();
                lastStatus = lastGetModel.items.Values().Select(l =>
                {
                    var obj = l.ToObject<ProxyStatus>();
                    obj.id = Convert.ToInt64(l.Path);
                    return obj;
                }).ToArray();
            }

            _lastAliveProxies = proxies.ToDictionary(l => l, l => lastStatus.First(s => s.id == l.id));
        }
    }
}
