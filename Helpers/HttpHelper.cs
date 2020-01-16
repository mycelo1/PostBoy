using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PostBoy.Helpers
{
    public class HttpHelper
    {
        public int? timeout { get; set; }
        public string? proxy_address { get; set; }

        public (int status, string header, string content_type, string body) Request(string method, string url, string header, string content_type, string charset, string? body)
        {
            byte[]? v_request_bytes = null;
            HttpWebRequest v_web_request;
            HttpWebResponse? v_web_response = null;
            int v_resp_status_code;
            string v_resp_body;
            string v_resp_header;
            string v_resp_content_type;

            Uri v_uri = new Uri(url);
            v_web_request = (HttpWebRequest)WebRequest.Create(v_uri);
            v_web_request.Method = method;

            if (timeout != null)
            {
                v_web_request.Timeout = (int)timeout;
            }

            if (!String.IsNullOrEmpty(proxy_address))
            {
                if (proxy_address == "+")
                {
                    v_web_request.Proxy = null;
                }
                else
                {
                    v_web_request.Proxy = new WebProxy(proxy_address);
                }
            }

            foreach (var header_line in header.Split(new string[] { "\n\r", "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                v_web_request.Headers.Add(header_line);
            }

            if (!String.IsNullOrWhiteSpace(body))
            {
                v_request_bytes = Encoding.GetEncoding(charset).GetBytes(body);
                v_web_request.ContentLength = v_request_bytes.Length;
                v_web_request.ContentType = $"{content_type}; charset={charset}";
            }

            try
            {
                if (v_request_bytes != null)
                {
                    using (Stream v_request_stream = v_web_request.GetRequestStream())
                    {
                        v_request_stream.Write(v_request_bytes, 0, v_request_bytes.Length);
                    }
                }

                v_web_response = (HttpWebResponse)(v_web_request.GetResponse());
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    v_web_response = (HttpWebResponse)(e.Response);
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                using (Stream v_response_stream = v_web_response.GetResponseStream())
                using (StreamReader v_reader = new StreamReader(v_response_stream))
                {
                    v_resp_status_code = (int)(((HttpWebResponse)v_web_response).StatusCode);
                    var v_nvc = ((HttpWebResponse)v_web_response).Headers;
                    v_resp_header = String.Join("\n", v_nvc.AllKeys.Select((x, y) => $"{x}: {v_nvc[x]}"));
                    v_resp_body = v_reader.ReadToEnd();
                    v_resp_content_type = ((HttpWebResponse)v_web_response).ContentType;
                }

                return (v_resp_status_code, v_resp_header, v_resp_content_type, v_resp_body);
            }
            finally
            {
                v_web_response.Dispose();
            }
        }
    }
}
