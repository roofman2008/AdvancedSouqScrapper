using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SouqScrapper.Helpers;
using SouqScrapper.LinkGenerators;

namespace SouqScrapper.ApiModels
{
    public static class SouqApi
    {
        private static DateTime ExpireSession = DateTime.Now;
        private static CookieCollection Cookies = null;
        private static string cookieString = null;

        #region Helpers
        private static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
        {
            ArrayList al = new ArrayList();
            CookieCollection cc = new CookieCollection();
            if (strHeader != string.Empty)
            {
                al = ConvertCookieHeaderToArrayList(strHeader);
                cc = ConvertCookieArraysToCookieCollection(al, strHost);
            }
            return cc;
        }
        private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            string[] strCookTemp = strCookHeader.Split(',');
            ArrayList al = new ArrayList();
            int i = 0;
            int n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                {
                    al.Add(strCookTemp[i]);
                }
                i = i + 1;
            }
            return al;
        }
        private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            CookieCollection cc = new CookieCollection();

            int alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (int i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");
                            string firstName = strCNameAndCValue.Substring(0, firstEqual);
                            string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName;
                            cookTemp.Value = allValue;
                        }
                        continue;
                    }
                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');
                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Path = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Path = "/";
                            }
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');

                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Domain = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Domain = strHost;
                            }
                        }
                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                {
                    cookTemp.Path = "/";
                }
                if (cookTemp.Domain == string.Empty)
                {
                    cookTemp.Domain = strHost;
                }
                cc.Add(cookTemp);
            }
            return cc;
        }
        #endregion

        private static void SyncSouqCookies()
        {
            if (ExpireSession > DateTime.Now)
            {
                return;
            }

            var url = "https://egypt.souq.com/eg-en/shop-all-categories/c/?ref=nav";

            retry:
            try
            {
                var obj = (WebScrapper.GetHeaders(url))["Set-Cookie"];
                var cc = GetAllCookiesFromHeader(obj, url);
                ExpireSession = DateTime.Now.AddYears(1).AddDays(-2);
                Cookies = cc;
                cookieString = obj;
            }
            catch (Exception ex)
            {
                goto retry;
            }
        }

        public static IEnumerable<SouqProductReview> GetProductReview(string productId, int page = 0, int pageLimit = -1)
        {
            List<SouqProductReview> tmpList = new List<SouqProductReview>();

            int tmpPage = page;
            bool stillAvailable;

            do
            {
                retry:
                SyncSouqCookies();
                var url = $"https://egypt.souq.com/eg-en/reviews.php?page={tmpPage + 1}&action=get_reviews&id_item={productId}";

                try
                {
                    var obj = WebScrapper.GetDownloadJson<SouqProductReview>(url, headers: new Dictionary<string, string>()
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });

                    tmpList.Add(obj);

                    stillAvailable = obj.show_more;

                    if (pageLimit < 0 || page < page + pageLimit)
                        tmpPage++;
                    else
                        stillAvailable = false;
                }
                catch (HtmlScapperException ex)
                {
                    /*Console Write*/
                    goto retry;
                }
            } while (stillAvailable);

            return tmpList;
        }

        public static IEnumerable<SouqSellerReview> GetSellerReview(string sellerId, int page = 0, int pageLimit = -1)
        {
            List<SouqSellerReview> tmpList = new List<SouqSellerReview>();

            int tmpPage = 0;
            bool stillAvailable;

            do
            {
                retry:
                SyncSouqCookies();
                var url =
                    $"https://egypt.souq.com/eg-en/{sellerId.ToPercentageEncoding(SouqApiConstants.PercentageEncodingLevel)}/p/profile.html";

                try
                {
                    var obj = WebScrapper.PostDownloadJson<SouqSellerReview>(url, headers: new Dictionary<string, string>()
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    }, parameters: new Dictionary<string, string>()
                    {
                        {"action", "show_more"},
                        {"source", "seller_profile"},
                        {"row", $"{tmpPage}"},
                        {"seller", $"{sellerId}"},
                        {"filter", ""},
                        {"r_url", $"{url}"}
                    });

                    tmpList.Add(obj);

                    stillAvailable = obj.bShowMore;

                    if (pageLimit < 0 || page < page + pageLimit)
                        tmpPage++;
                    else
                        stillAvailable = false;
                }
                catch (HtmlScapperException ex)
                {
                    /*Console Write*/
                    goto retry;
                }
            } while (stillAvailable);

            return tmpList;
        }

        public static IDictionary<string, SouqToken> GetProductAccessTokens(string productFullId)
        {
            retry:

            SyncSouqCookies();
            var url = $"https://egypt.souq.com/eg-en/{productFullId}/i/";

            try
            {
                Dictionary<string, SouqToken> tmpResult = new Dictionary<string, SouqToken>();

                var document = WebScrapper.GetDownloadHtml(url, cookies: Cookies);

                var formIds = new[]
                {
                    "searchForm",
                    "addItemToCart",
                    "reviews-list-id"
                };

                int index = 0;
                var formResults = new SouqToken[3];

                foreach (var formId in formIds)
                {
                    var filter_hitNodeExist = document.DocumentNode.Descendants()
                        .AnyByNameNAttribute("form", "id", formId);

                    if (filter_hitNodeExist)
                    {
                        var filter_hitsNode = document.DocumentNode.Descendants()
                            .SingleByNameNAttribute("form", "id", formId);
                        var filter_hitsCfs = filter_hitsNode.Descendants()
                            .SingleByNameNAttribute("input", "name", "hitsCfs");
                        var filter_hitsCfsMeta = filter_hitsNode.Descendants()
                            .SingleByNameNAttribute("input", "name", "hitsCfsMeta");

                        formResults[index] = new SouqToken()
                        {
                            hitsCfs = filter_hitsCfs.Attributes["value"].Value,
                            hitsCfsMeta = filter_hitsCfsMeta.Attributes["value"].Value
                        };
                    }
                    else
                    {
                        formResults[index] = null;
                    }

                    index++;
                }

                for (int i = 0; i < formIds.Length; i++)
                {
                    tmpResult.Add(formIds[i], formResults[i]);
                }

                return tmpResult;
            }
            catch (HtmlScapperException ex)
            {
                goto retry;
            }
        }

        public static SouqProduct GetProduct(string productFullId)
        {
            retry:
            SyncSouqCookies();

            var url = $"https://egypt.souq.com/eg-en/{productFullId}/i/";

            try
            {
                var obj = WebScrapper.GetDownloadJson(url, headers: new Dictionary<string, string>()
                {
                    {"X-Requested-With", "XMLHttpRequest"}
                });

                SouqProduct product = null;

                if (obj != null)
                {
                    product = new SouqProduct
                    {
                        body = obj.Property("body").Value.ToString(),
                        bundles = null
                    };

                    if (obj.Property("aBundles").Value.ToString() != "[]")
                    {
                        var bundle = obj.Property("aBundles").Value.ToObject<JObject>();
                        var bundlesList = bundle.Properties().Values().Select(l => l.ToObject<JObject>()).ToList();

                        product.bundles = new List<SouqBundle>();

                        foreach (var tmpBundle in bundlesList)
                        {
                            var bundleObj = tmpBundle.ToObject<SouqBundle>();

                            if (tmpBundle.Property("units") != null)
                            {
                                var units = tmpBundle.Property("units").Value.ToObject<JObject>();
                                var unitsList = units.Properties().Values().Select(l => l.ToObject<JObject>()).ToList();
                                bundleObj.units = unitsList.Select(l => l.ToObject<SouqBundleUnits>()).ToList();
                            }

                            product.bundles.Add(bundleObj);
                        }
                    }
                }
                else
                {
                    var objHtml = WebScrapper.GetDownloadHtml(url);

                    product = new SouqProduct
                    {
                        body = objHtml.DocumentNode.OuterHtml,
                        bundles = null
                    };
                }

                return product;
            }
            catch (HtmlScapperException ex)
            {
                goto retry;
            }
        }

        public static IEnumerable<SouqProductsGrid> GetProductsFromCategory(string categoryId)
        {
            List<SouqProductsGrid> tmpList = new List<SouqProductsGrid>();

            int section = 1;
            int page = 1;
            bool stillAvailable;

            peek_retry:
            SyncSouqCookies();

            try
            {
                var url_peek = $"https://egypt.souq.com/eg-en/{categoryId}/l/?ref=nav&page=1&section=1";
                var new_url = WebScrapper.GetResponseUrl(url_peek);

                if (!new_url.Equals(url_peek))
                    return null;
            }
            catch
            {
                goto peek_retry;
            }

            do
            {
                retry:
                SyncSouqCookies();
                var url = $"https://egypt.souq.com/eg-en/{categoryId}/l/?ref=nav&page={page}&section={section}&action=grid";

                try
                {
                    var obj = WebScrapper.GetDownloadJson<SouqProductsGrid>(url, headers: new Dictionary<string, string>()
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });

                    tmpList.Add(obj);

                    stillAvailable = obj.show_more == 1;

                    if (section == 2)
                    {
                        page++;
                        section = 1;
                    }
                    else
                    {
                        section++;
                    }
                }
                catch (HtmlScapperException ex)
                {
                    /*Console Write*/
                    goto retry;
                }
            } while (stillAvailable);

            return tmpList;
        }

        public static IEnumerable<SouqProductsGrid2> SearchProductsParallel(string categoryId)
        {
            ConcurrentBag<SouqProductsGrid2> tmpList = new ConcurrentBag<SouqProductsGrid2>();
            int requestCount = 0;
            retry_peek:
            try
            {
                SyncSouqCookies();
                var peek_url = $"https://egypt.souq.com/eg-en/{categoryId}/s/?page=1&section=1&action=grid";
                var peek_obj = WebScrapper.GetDownloadJson<SouqProductsGrid2>(peek_url,
                    headers: new Dictionary<string, string>()
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });

                if (!string.IsNullOrEmpty(peek_obj.redirect_url))
                {
                    categoryId = peek_obj.redirect_url.ExtractCategoryId();
                    goto retry_peek;
                }

                requestCount = (int)Math.Ceiling((float)peek_obj.jsonData.meta_data.total / SouqApiConstants.ProductSectionListLimit);
            }
            catch (Exception ex)
            {
                /*Console Write*/
                Console.WriteLine(DateTime.Now + "-" + ex.InnerException?.Message ?? ex.Message);
                goto retry_peek;
            }


            var pagingGenerator = new PagingLinkGenerator(requestCount);

            Parallel.ForEach(pagingGenerator, (pageInfo) =>
            {
                retry:
                SyncSouqCookies();
                var url = $"https://egypt.souq.com/eg-en/{categoryId}/s/?page={pageInfo.Page}&section={pageInfo.Section}&action=grid";
                //Console.WriteLine(url);
                try
                {
                    var obj = WebScrapper.GetDownloadJson<SouqProductsGrid2>(url,
                        headers: new Dictionary<string, string>()
                        {
                            {"X-Requested-With", "XMLHttpRequest"}
                        });

                    /*Under Test Fields*/
                    var is_ags_test = obj.jsonData.units.Where(l => l.is_ags);
                    if (is_ags_test.Any())
                        Debugger.Break();

                    var is_price_cbt = obj.jsonData.units.Where(l => l.price_cbt != "");
                    if (is_price_cbt.Any())
                        Debugger.Break();

                    var is_coupon = obj.jsonData.units.Where(l => l.coupon != null && l.coupon.ToString() != "[]");
                    if (is_coupon.Any())
                        Debugger.Break();

                    var is_InternationalSeller = obj.jsonData.units.Where(l => l.isInternationalSeller);
                    if (is_InternationalSeller.Any())
                        Debugger.Break();

                    var is_shipping = obj.jsonData.units.Where(l => l.shipping != false);
                    if (is_shipping.Any())
                        Debugger.Break();

                    var is_selling_points = obj.jsonData.units.Where(l => l.selling_points.ToString() != "False");
                    if (is_selling_points.Any())
                        Debugger.Break();

                    tmpList.Add(obj);
                }
                catch (Exception ex)
                {
                    /*Console Write*/
                    Console.WriteLine(DateTime.Now + "-" + ex.InnerException?.Message ?? ex.Message);
                    goto retry;
                }
            });

            return tmpList.ToList();
        }

        public static IEnumerable<SouqProductsGrid2> SearchProducts(string categoryId)
        {
            List<SouqProductsGrid2> tmpList = new List<SouqProductsGrid2>();
            int section = 1;
            int page = 1;
            bool stillAvailable;

            do
            {
                retry:
                SyncSouqCookies();
                var url = $"https://egypt.souq.com/eg-en/{categoryId}/s/?page={page}&section={section}&action=grid";
                //Console.WriteLine(url);
                try
                {
                    var obj = WebScrapper.GetDownloadJson<SouqProductsGrid2>(url, headers: new Dictionary<string, string>()
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });

                    obj.page = page;
                    obj.section = section;

                    if (string.IsNullOrEmpty(obj.redirect_url))
                    {
                        /*Under Test Fields*/
                        var is_ags_test = obj.jsonData.units.Where(l => l.is_ags);
                        if (is_ags_test.Any())
                            Debugger.Break();

                        var is_price_cbt = obj.jsonData.units.Where(l => l.price_cbt != "");
                        if (is_price_cbt.Any())
                            Debugger.Break();

                        var is_coupon = obj.jsonData.units.Where(l => l.coupon != null && l.coupon.ToString() != "[]");
                        if (is_coupon.Any())
                            Debugger.Break();

                        var is_InternationalSeller = obj.jsonData.units.Where(l => l.isInternationalSeller);
                        if (is_InternationalSeller.Any())
                            Debugger.Break();

                        var is_shipping = obj.jsonData.units.Where(l => l.shipping != false);
                        if (is_shipping.Any())
                            Debugger.Break();

                        var is_selling_points = obj.jsonData.units.Where(l => l.selling_points.ToString() != "False");
                        if (is_selling_points.Any())
                            Debugger.Break();
                    }
                    else
                    {
                        //Debugger.Break();
                        categoryId = obj.redirect_url.ExtractCategoryId();
                        goto retry;
                    }

                    tmpList.Add(obj);

                    stillAvailable = obj.show_more == 1;

                    if (section == 2)
                    {
                        page++;
                        section = 1;
                    }
                    else
                    {
                        section++;
                    }
                }
                catch (HtmlScapperException ex)
                {
                    /*Console Write*/
                    Console.WriteLine(DateTime.Now + "-" + ex.InnerException?.Message ?? ex.Message);
                    goto retry;
                }
            } while(stillAvailable);

            return tmpList.ToList();
        }

        public static HtmlDocument GetCategories()
        {
            retry:
            var url =
                $"https://egypt.souq.com/eg-en/shop-all-categories/c/?ref=nav";
            SyncSouqCookies();

            try
            {
                var document = WebScrapper.GetDownloadHtml(url);
                return document;
            }
            catch (HtmlScapperException ex)
            {
                goto retry;
            }
        }

        public static HtmlDocument GetSellerProfile(string sellerId)
        {
            retry:
            var url =
                $"https://egypt.souq.com/eg-en/{sellerId.ToPercentageEncoding(SouqApiConstants.PercentageEncodingLevel)}/p/profile.html";
            SyncSouqCookies();

            try
            {
                var document = WebScrapper.GetDownloadHtml(url);
                return document;
            }
            catch (HtmlScapperException ex)
            {
                goto retry;
            }
        }

        public static SouqCities GetCities()
        {
            retry:
            SyncSouqCookies();
            var url = $"https://egypt.souq.com/eg-en/item_one.php";

            try
            {
                var obj = WebScrapper.PostDownloadJson<SouqCities>(url, headers: new Dictionary<string, string>()
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    },
                    parameters: new Dictionary<string, string>()
                    {
                        {"action", "get_cities"},
                        {"country_code", "eg"},
                        {"selected_city", "CAI"}
                    });

                return obj;
            }
            catch (HtmlScapperException ex)
            {
                goto retry;
            }
        }

        public static SouqProductBucket GetProductBucket(string productFullId)
        {
            retry:
            SyncSouqCookies();

            var url = $"https://egypt.souq.com/eg-en/{productFullId}/i/";

            try
            {
                var document = WebScrapper.GetDownloadHtml(url);
                var globalBucketNode = document.DocumentNode.Descendants().First(l => l.Name == "script" && l.InnerHtml.Contains("globalBucket"));
                var globalBucketData =
                    globalBucketNode.InnerText.Replace("var globalBucket =", "").TrimStart().TrimEnd();
                return JsonConvert.DeserializeObject<SouqProductBucket>(globalBucketData);
            }
            catch (HtmlScapperException ex)
            {
                if (ex.InnerException.Message == "Too many automatic redirections were attempted.")
                    return null;

                goto retry;
            }
        }

        public static SouqDeliveryInfo GetDeliveryInfo(string cityCode, string cityName, string unitId, string hitsCfs, string hitsCfsMeta)
        {
            retry:
            SyncSouqCookies();
            var url =
                $"https://egypt.souq.com/eg-en/item_one.php?action=estimated_delivery&currentCityISO={cityCode}&currentCity={cityName}&" +
                $"idUnit={unitId}&hitsCfs={hitsCfs}&hitsCfsMeta={hitsCfsMeta.ToPercentageEncoding(1)}";

            try
            {
                var obj = WebScrapper.GetDownloadJson<SouqDeliveryInfo>(url, headers: new Dictionary<string, string>()
                {
                    {"X-Requested-With", "XMLHttpRequest"}
                }, cookies: Cookies);

                return obj;
            }
            catch (HtmlScapperException ex)
            {
                if (ex.InnerException.Message.Contains("Too many automatic redirections were attempted"))
                    throw;

                goto retry;
            }
        }

        public static (SouqProductBucket bucket, IDictionary<string, SouqToken> tokens) GetProductResultsHtml(string productFullId)
        {
            retry:
            SyncSouqCookies();

            var url = $"https://egypt.souq.com/eg-en/{productFullId}/i/";

            HtmlDocument document;

            try
            {
                document = WebScrapper.GetDownloadHtml(url);
            }
            catch (HtmlScapperException ex)
            {
                goto retry;
            }

            #region Bucket

            var globalBucketNode = document.DocumentNode.Descendants().First(l => l.Name == "script" && l.InnerHtml.Contains("globalBucket"));
            var globalBucketData =
                globalBucketNode.InnerText.Replace("var globalBucket =", "").TrimStart().TrimEnd();

            var bucketObj = JsonConvert.DeserializeObject<SouqProductBucket>(globalBucketData);

            #endregion

            #region Token

            Dictionary<string, SouqToken> tmpResult = new Dictionary<string, SouqToken>();

            var formIds = new[]
            {
                "searchForm",
                "addItemToCart",
                "reviews-list-id"
            };

            int index = 0;
            var formResults = new SouqToken[3];

            foreach (var formId in formIds)
            {
                var filter_hitNodeExist = document.DocumentNode.Descendants()
                    .AnyByNameNAttribute("form", "id", formId);

                if (filter_hitNodeExist)
                {
                    var filter_hitsNode = document.DocumentNode.Descendants()
                        .SingleByNameNAttribute("form", "id", formId);
                    var filter_hitsCfs = filter_hitsNode.Descendants()
                        .SingleByNameNAttribute("input", "name", "hitsCfs");
                    var filter_hitsCfsMeta = filter_hitsNode.Descendants()
                        .SingleByNameNAttribute("input", "name", "hitsCfsMeta");

                    formResults[index] = new SouqToken()
                    {
                        hitsCfs = filter_hitsCfs.Attributes["value"].Value,
                        hitsCfsMeta = filter_hitsCfsMeta.Attributes["value"].Value
                    };
                }
                else
                {
                    formResults[index] = null;
                }

                index++;
            }

            for (int i = 0; i < formIds.Length; i++)
            {
                tmpResult.Add(formIds[i], formResults[i]);
            }

            #endregion

            return (bucketObj, tmpResult);
        }
    }
}