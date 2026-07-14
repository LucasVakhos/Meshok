using GH.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Text.RegularExpressions;
using static GH.Components.DlgHelper;
using System.Windows.Forms;
using System.Linq;
namespace NewsMaker.Common
{
    internal enum UnprocessReason
    {
        [Description("???????????? ??????.")] BadRequest = 400,
        [Description("API-???? ?? ?????? ???????? (?????????? ????).")] ApiKeyNotFound = 401,
        [Description("????????? ??????.")]  PaymentRequired = 402,
        [Description("?????????.")] ApiKeyForbidden = 403,
        [Description("?? ??????.")] ResourceNotFound = 404,
        [Description("User Domain not found")] UserDomainNotFound,
        [Description("TemplateMailUser not found by id")] TemplateMailUserNotFoundById,
        [Description("MailUuid not found")] MailUuidNotFound,
        [Description("Email receiver unsubscribed from this API key mails")] ReceiverUnsubscribed,
        [Description("Email receiver complained from this API key mails")] ReceiverComplained,
        [Description("Email receiver doesn't exist")] ReceiverNotExist,
        [Description("Email receiver unavailable")] ReceiverUnavailable,
        [Description("Template not found by templateId")] TemplateNotFound,
        [Description("Aanother UnSent Reason")] AanotherUnSentReason,
    }
    public static class MyObjectExtensions
    {
        public static T ToEnumByDescription<T>(this string value) where T : struct
        {
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute && attribute.Description.ToUpper() == value.ToUpper())
                    return (T)field.GetValue(null);
            }
            throw new ArgumentException($"?? ??????? ???????????? ??? ???????? ??? {typeof(T)}. ", nameof(value));
        }
        public static string ToIdempotencyKey(this string value)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public static string ToUTF8(this string value)
        {
            string unicodeString = Regex.Unescape(value);
            Encoding utf8 = Encoding.UTF8;
            Encoding unicode = Encoding.Unicode;
            //????????? ? ???????? ??????
            byte[] unicodeBytes = unicode.GetBytes(unicodeString);
            // ????????? ????? ??????? ? ???8
            byte[] utf8Bytes = Encoding.Convert(unicode, utf8, unicodeBytes);
            // ????????? ?? ? ???????
            char[] utf8Chars = new char[utf8.GetCharCount(utf8Bytes, 0, utf8Bytes.Length)];
            utf8.GetChars(utf8Bytes, 0, utf8Bytes.Length, utf8Chars, 0);
            return new string(utf8Chars);
        }
        public static string ToDebugInfo(this WebException value, HttpWebRequest request)
        {
            char[] sp = { '\r', '\n' };
            string[] strings = request.Headers.ToString().Split(sp);
            string ApiKeyName = "X-Api-Key";
            string ApiKey = strings.First(o => o.StartsWith(ApiKeyName)).Substring(ApiKeyName.Length + 1).Trim();
            string RequestUri = request.RequestUri.AbsoluteUri;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("=== DEBUG ===");
            builder.AppendLine("WebException: " + value.Message + $" HResult: {value.HResult}");
            builder.AppendLine($"{ApiKeyName} length: " + (ApiKey?.Length ?? 0));
            builder.AppendLine($"{ApiKeyName} begin with: {ApiKey.Substring(0, 20)}");
            builder.AppendLine($"Expect100Continue: {request.ServicePoint.Expect100Continue}");
            builder.AppendLine($"RequestUri: {RequestUri}");
            builder.AppendLine("=== request.Headers ===");
            for (int i = 0; i < strings.Length; i++)
            {
                string curr_val = strings[i].Trim();
                if (string.IsNullOrWhiteSpace(curr_val))
                    continue;
                if (curr_val.StartsWith(ApiKeyName))
                {
                    builder.AppendLine($"{ApiKeyName}: {ApiKey.Substring(0, 20)}");
                }
                else
                    builder.AppendLine(curr_val);
            }
            builder.AppendLine("=== DEBUG ===");
            return builder.ToString();
        }
        public static T InvokeIfRequired<T>(this Form form, Func<T> function)
        {
            if (form.InvokeRequired)
            {
                return (T)form.Invoke(function);
            }
            else
            {
                return function();
            }
        }
    }
    public class ResponseData : Dictionary<string, object>
    {
        public List<Exception> Errors
        {
            get
            {
                if (ContainsKey("errors"))
                    return this["errors"] as List<Exception>;
                else
                    return null;
            }
        }
        public void RegError(Exception ex)
        {
            if (!ContainsKey("errors"))
                this["errors"] = new List<Exception>();
            (this["errors"] as List<Exception>).Add(ex);
            handleError(ex);
        }
        internal string GetErrMessages()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in Errors)
            {
                builder.Append(item.Message);
            }
            return builder.ToString();
        }
        private void handleError(Exception ex)
        {
            if (ex is WebException)
            {
                string errorMessage = string.Empty;
                WebException web = ex as WebException;
                string statusDescription = web.Message;
                HttpStatusCode statusCode = (web.Response as HttpWebResponse).StatusCode;
                UnprocessReason unprocessReason = UnprocessReason.AanotherUnSentReason;
                switch ((int)statusCode)
                {
                    case 400:
                        errorMessage += statusDescription;
                        break;
                    case 401:
                        errorMessage += "API-???? ?? ?????? ???????? (?????????? ????).";
                        unprocessReason = UnprocessReason.ApiKeyNotFound;
                        break;
                    case 402:
                        errorMessage += "?? ??????? ???????????? ???????????? ???????? ??? ???????? ??????.";
                        unprocessReason = UnprocessReason.PaymentRequired;
                        break;
                    case 403:
                        errorMessage += "API-???? ??????, ?? ?? ???????????.";
                        unprocessReason = UnprocessReason.ApiKeyNotFound;
                        break;
                    case 404:
                        {
                            unprocessReason = statusDescription.ToEnumByDescription<UnprocessReason>();
                            switch (unprocessReason)
                            {
                                case UnprocessReason.ApiKeyNotFound:
                                    errorMessage += "API-???? ? ????????? ??????????????? ?? ??????????";
                                    break;
                                case UnprocessReason.UserDomainNotFound:
                                    errorMessage += "????? ??????????? ?? ?????? / ?? ???????? ? ????????????";
                                    break;
                                case UnprocessReason.TemplateMailUserNotFoundById:
                                    errorMessage += "?????? ?????? ?? ?????? (??? /send-by-template)";
                                    break;
                                case UnprocessReason.MailUuidNotFound:
                                    errorMessage += "?????????? ?????? - UUID ?????? ?? ??????)";
                                    break;
                                default:
                                    errorMessage += statusDescription;
                                    break;
                            }
                        }
                        break;
                    case 422:
                        {
                            unprocessReason = statusDescription.ToEnumByDescription<UnprocessReason>();
                            switch (unprocessReason)
                            {
                                case UnprocessReason.ReceiverUnsubscribed:
                                    errorMessage += "?????????? ????????? ?? ???????? ??????? API-?????.";
                                    break;
                                case UnprocessReason.ReceiverComplained:
                                    errorMessage += "?????????? ??????????? ?? ???????? ??????? API-?????.";
                                    break;
                                case UnprocessReason.ReceiverNotExist:
                                    errorMessage += "Email ?????????? ?? ?????????? (hard bounce)).";
                                    break;
                                case UnprocessReason.ReceiverUnavailable:
                                    errorMessage += "Email ?????????? ???????? ?????????? (soft bounce)).";
                                    break;
                                case UnprocessReason.TemplateNotFound:
                                    errorMessage += "?????? ?????? ?? ?????? (??? /send-by-template)";
                                    break;
                                default:
                                    errorMessage += statusDescription;
                                    break;
                            }
                        }
                        break;
                    case 500:
                        errorMessage += "?????????????? ?????? ?? ??????? ???????.";
                        break;
                    case 501:
                        errorMessage += "C????? ?? ???????????? ????????????? ?? .";
                        break;
                    case 502:
                        errorMessage += "????????????? ??????-?????? ??????? ???????????? ????? ?? ??????? ?????? ??? ????????? ???????.";
                        break;
                    case 503:
                        errorMessage += "?????? ????????? ????? ???????? ??????????.";
                        break;
                    case 504:
                        errorMessage += "????????????? ??????-?????? ???????????, ?????? ?????? ?? ??????? ?????? ??? ????????? ???????.";
                        break;
                    case 505:
                        errorMessage += "????????????? ?????? HTTP ?? ?????????????? ????????.";
                        break;
                    default:
                        errorMessage += statusDescription;
                        break;
                }
                Add("error_code", (int)statusCode);
                Add("status_code", statusCode);
                Add(nameof(UnprocessReason), unprocessReason);
                Add("error_message", errorMessage);
            }
            else
            {
                Add("error_code", ex.HResult);
                Add("error_message", ex.Message);
            }
        }
    }
    public class RuSender
    {
        private string ApiUrl => "https://api.rusender.ru/api/v1/external-mails/send";
        //public static int ID => 3875;
        public int ID { get; }
        //public static string ApiKey => "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZFVzZXIiOjIyMDc1LCJpZEV4dGVybmFsTWFpbEFwaUtleSI6NDAyNywiaWF0IjoxNzc1MjAzMjQxfQ.9TeNqCda1pAovQZ_Vj7zZmgbYOycI_XDigsvyD-4YUw";
        public string ApiKey { get; }
        public RuSender(int id, string apikey)
        {
            if (id == 0 || apikey == null)
                DlgError("Empty ID or ApiKey");
            ID = id;
            ApiKey = apikey;
        }
        public ResponseData sendRequest(Letter letter)
        {
            string returnStr = null;
            ResponseData response = new ResponseData();
            HttpWebRequest request = WebRequest.CreateHttp(ApiUrl);
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("X-Api-Key", ApiKey);
                request.ServicePoint.Expect100Continue = false;
                request.UserAgent = "BridgenoteApp/1.0";
                //request.Headers.Add("Accept-Language", "en-UK");
                string stringdata = MakeRequestString(letter.Content);
                byte[] buffer = Encoding.UTF8.GetBytes(stringdata);
                request.ContentLength = buffer.Length;
                Stream PostData = request.GetRequestStream();
                PostData.Write(buffer, 0, buffer.Length);
                PostData.Close();
                HttpWebResponse webResp = null;
                try
                {
                    webResp = (HttpWebResponse)request.GetResponse();
                    HttpStatusCode status = webResp.StatusCode;
                    response.Add("http_code", (int)status);
                    Stream WebResponse = webResp.GetResponseStream();
                    StreamReader _response = new StreamReader(WebResponse);
                    returnStr = _response.ReadToEnd();
                    if (returnStr.Length > 0)
                    {
                        object jo = null;
                        try
                        {
                            jo = JsonConvert.DeserializeObject<object>(returnStr.Trim());
                            if (jo.GetType() == typeof(JObject))
                                jo = (JObject)jo;
                            else if (jo.GetType() == typeof(JArray))
                                jo = (JArray)jo;
                            response.Add("data", jo);
                        }
                        catch (JsonException jex)
                        {
                            response.RegError(jex);
                        }
                    }
                }
                catch (WebException we)
                {
                    HttpStatusCode code = (we.Response as HttpWebResponse).StatusCode;
                    Logger.Error(we);
                    //Logger.Error(debug);
                    response.RegError(we);
                    response.Add("DebugInfo", we.ToDebugInfo(request));
                }
            }
            catch (Exception ex)
            {                
                Logger.Error(ex);
                response.RegError(ex);
            }
            return response;
        }
        public class Converter : KeyValuePairConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Type t = value.GetType();
                PropertyInfo keyProperty = t.GetProperty("Key");
                PropertyInfo valueProperty = t.GetProperty("Value");
                writer.WriteStartObject();
                writer.WritePropertyName(keyProperty.GetValue(value).ToString());
                serializer.Serialize(writer, valueProperty.GetValue(value));
                writer.WriteEndObject();
            }
        };
        public static string MakeRequestString(object data)
        {
            var converter = new Converter();
            var setting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            setting.Converters.Add(converter);
            return JsonConvert.SerializeObject(data, setting);
        }
    }
}
