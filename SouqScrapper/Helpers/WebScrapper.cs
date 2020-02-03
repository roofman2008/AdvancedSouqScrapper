using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.Mentalis.Network.ProxySocket;
using SouqScrapper.Core;

namespace SouqScrapper.Helpers
{
    public static class WebScrapper
    {
        private const int HttpRequestTimeout = 90 * 1000;

        public static HttpStatusCode GetStatus(string url, Dictionary<string, string> headers = null,
            CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            return response.StatusCode;
        }

        public static string GetResponseUrl(string url, Dictionary<string, string> headers = null,
            CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            return response.ResponseUri.AbsoluteUri;
        }

        public static HttpStatusCode PostStatus(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> parameters = null, CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response;

            string postData = String.Empty;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    postData += $"{parameter.Key}={parameter.Value}&";
                }

                postData = postData.Substring(0, postData.Length - 1);
                byte[] data = Encoding.ASCII.GetBytes(postData);
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            return response.StatusCode;
        }

        public static T GetDownloadJson<T>(string url, Dictionary<string, string> headers = null,
            CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    string json = null;

                    using (var stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException(),
                            Encoding.UTF8);
                        json = reader.ReadToEnd();
                    }

                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception e)
                {
                    throw new HtmlScapperException("Cannot Load Json Stream.", e);
                }
            }

            throw new HtmlScapperException("Cannot Find Proper Response.");
        }

        public static JObject GetDownloadJson(string url, Dictionary<string, string> headers = null, CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    string json = null;

                    using (var stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException(),
                            Encoding.UTF8);
                        json = reader.ReadToEnd();
                    }

                    return (JObject) JsonConvert.DeserializeObject(json);
                }
                catch (Exception e)
                {
                    throw new HtmlScapperException("Cannot Load Html Stream.", e);
                }
            }

            throw new HtmlScapperException("Cannot Find Proper Response.");
        }

        public static T PostDownloadJson<T>(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> parameters = null, CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response;

            string postData = String.Empty;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    postData += $"{parameter.Key}={parameter.Value}&";
                }

                postData = postData.Substring(0, postData.Length - 1);
                byte[] data = Encoding.ASCII.GetBytes(postData);
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    string json = null;

                    using (var stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException(),
                            Encoding.UTF8);
                        json = reader.ReadToEnd();
                    }

                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception e)
                {
                    throw new HtmlScapperException("Cannot Load Json Stream.", e);
                }
            }

            throw new HtmlScapperException("Cannot Find Proper Response.");
        }

        public static HtmlDocument GetDownloadHtml(string url, Dictionary<string, string> headers = null, CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                HtmlDocument doc = new HtmlDocument();

                try
                {
                    doc.Load(response.GetResponseStream());

                    return doc;
                }
                catch (Exception e)
                {
                    throw new HtmlScapperException("Cannot Load Html Stream.", e);
                }
            }

            throw new HtmlScapperException("Cannot Find Proper Response.");
        }

        public static HtmlDocument PostDownloadHtml(string url, Dictionary<string, string> headers = null,
            Dictionary<string, string> parameters = null, CookieCollection cookies = null)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response;

            string postData = String.Empty;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    postData += $"{parameter.Key}={parameter.Value}&";
                }

                postData = postData.Substring(0, postData.Length - 1);
                byte[] data = Encoding.ASCII.GetBytes(postData);
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
           

            if (headers != null)
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                HtmlDocument doc = new HtmlDocument();

                try
                {
                    doc.Load(response.GetResponseStream());

                    return doc;
                }
                catch (Exception e)
                {
                    throw new HtmlScapperException("Cannot Load Html Stream.", e);
                }
            }

            throw new HtmlScapperException("Cannot Find Proper Response.");
        }

        public static WebHeaderCollection GetHeaders(string url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse response;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = HttpRequestTimeout;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                throw new HtmlScapperException("Cannot Get Response.", e);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    return response.Headers;
                }
                catch (Exception e)
                {
                    throw new HtmlScapperException("Cannot Load Cookies.", e);
                }
            }

            throw new HtmlScapperException("Cannot Find Proper Response.");
        }
    }

    public class HtmlScapperException : Exception
    {
        public HtmlScapperException()
        {
        }

        public HtmlScapperException(string message) : base(message)
        {
        }

        public HtmlScapperException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HtmlScapperException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}