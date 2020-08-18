using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.ExternalServices.Base
{
    public interface IExternalService
    {
        Task<HtmlNode> ExecuteHtmlRequest(string requestUrl, bool takeBreathTime = true);
        Task<string> ExecuteJsonRequest(string requestUrl, bool takeBreathTime = true);
        Task<Stream> ExecuteImageRequest(string requestUrl);
        HtmlNode ExecuteWebDriverRequest(string requestUrl);
        Task<HttpResponseMessage> ExecuteHttpRequest(Uri requestUri, string referer);
    }
}
