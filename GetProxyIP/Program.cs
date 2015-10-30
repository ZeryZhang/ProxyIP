using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;


namespace GetProxyIP
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var usableProxys = new List<ProxyEntity>();
            var proxys = GetProxy();
            int count = 1;
            Parallel.ForEach(proxys, item =>
            {
                if (ValidateProxy(item.IP))
                {
                    Console.WriteLine("{0} 测试成功",item.IP);
                    usableProxys.Add(item);
                    
                }
            });

            Console.WriteLine("测试完成共{0}个代理可用",usableProxys.Count);
            Console.WriteLine("是否开始请求? Y/N");
            string keyvalue = Console.ReadLine();
            if (keyvalue.Equals("Y"))
            {
                Console.WriteLine("开始请求");
                foreach (var item in usableProxys)
                {
                    PostProxyHttp(item);
                    Console.WriteLine("{0} 请求完成",item.IP);
                }
            }

            Console.Read();
        }


        public static List<ProxyEntity> GetProxy()
        {
            int depth = 3;

            string html = string.Empty;
            List<ProxyEntity> proxyList = new List<ProxyEntity>();

            //========================
            // www.xicidaili.com
            //======================== 
            //国内高匿
            for (int i = 0; i < depth; i++)
            {
                string url = string.Format("http://www.xicidaili.com/nn/{0}", i + 1);
                html = HttpPost(url, Encoding.UTF8);
                var list = GetXiCiProxy(html);
                proxyList.AddRange(list);
            }

            //国外高匿
            for (int i = 0; i < depth; i++)
            {
                string url = string.Format("http://www.xicidaili.com/wn/{0}", i + 1);
                html = HttpPost(url, Encoding.UTF8);
                var list = GetXiCiProxy(html);
                proxyList.AddRange(list);
            }

            //========================
            // www.nianshao.me
            //======================== 
            //HttpPost("http://www.nianshao.me/?page=1", Encoding.Default);
            //HttpPost("http://www.nianshao.me/?page=1&yunsuo_session_verify=14aa79ba2a44866074322858310aae15", Encoding.Default);

            //for (int i = 0; i < depth; i++)
            //{
            //    string url = string.Format("http://www.nianshao.me/?page={0}", i + 1);
            //    html = HttpPost(url, Encoding.Default);
            //    var list = GetNianShaoProxy(html);
            //    proxyList.AddRange(list);
            //}


            //========================
            // www.89ip.cn
            //======================== 
            string bjipUrl = "http://www.89ip.cn/api.php?&tqsl=500&sxa=&sxb=&tta=&ports=&ktip=&cf=1";
            html = HttpPost(bjipUrl, Encoding.Default);
            var proxys = Get89ipProxy(html);
            proxyList.AddRange(proxys);

            //========================
            //  www.71https.com
            //======================== 
            HttpPost("http://www.71https.com/index.asp", Encoding.Default);
            HttpPost("http://www.71https.com/index.asp?page=1&yunsuo_session_verify=14aa79ba2a44866074322858310aae15", Encoding.Default);
            //国内高匿
            for (int i = 0; i < depth; i++)
            {
                string url = string.Format("http://www.71https.com/index.asp?page={0}", i + 1);
                html = HttpPost(url, Encoding.Default);
                var list = Get71HttpsProxy(html);
                proxyList.AddRange(list);
            }

            //国外高匿
            for (int i = 0; i < depth; i++)
            {
                string url = string.Format("http://www.71https.com/index.asp?stype=3&page={0}", i + 1);
                html = HttpPost(url, Encoding.Default);
                var list = Get71HttpsProxy(html);
                proxyList.AddRange(list);
            }

            return proxyList;
        }

        public static string PostProxyHttp(ProxyEntity proxy)
        {
            string url = "http://hktest.hk515.com:1237/Default.aspx";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebProxy webproxy = new WebProxy();
            Uri uri = new Uri(string.Format("http://{0}:{1}", proxy.IP, proxy.Port));
            webproxy.Address = uri;
            request.Proxy = webproxy;

            request.Accept = "text/plain, */*; q=0.01";
            request.Method = "GET";
            request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.ContentLength = 0;
            request.ContentType = "keep-alive";
            //request.Host = "www.cnblogs.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:25.0) Gecko/20100101 Firefox/25.0";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            string html = reader.ReadToEnd();

            return html;
        }

        /// <summary>
        /// Http请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpPost(string url ,Encoding encode)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebProxy webproxy = new WebProxy();
            //Uri uri = new Uri(string.Format("http://{0}:{1}", "Adrress", "Port"));
            //webproxy.Address = uri;
            //request.Proxy = webproxy;

            request.Accept = "text/plain, */*; q=0.01";
            request.Method = "GET";
            request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.ContentLength = 0;
            request.ContentType = "keep-alive";
            //request.Host = "www.cnblogs.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:25.0) Gecko/20100101 Firefox/25.0";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, encode);
            string html = reader.ReadToEnd();

            return html;
        }

        /// <summary>
        /// 西刺
        /// </summary>
        /// <returns></returns>
        public static List<ProxyEntity> GetXiCiProxy(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection htmlCollection = document.DocumentNode.SelectNodes("//table/tr[position()>2]");

            var proxyList = new List<ProxyEntity>();
            //遍历列
            foreach (var item in htmlCollection)
            {
                if (item.SelectNodes("td") == null)
                    continue;
                //取列中行 值
                var proxy = new ProxyEntity();

                var tds = item.SelectNodes("td");
                for (int i = 0; i < tds.Count(); i++)
                {
                    if (i == 2)
                        proxy.IP = tds[i].InnerText;
                    if (i == 3)
                        proxy.Port = tds[i].InnerText;
                    if (i == 4)
                        proxy.CityName = tds[i].InnerText;
                    if (i == 5)
                        proxy.Anonymity = tds[i].InnerText;
                    if (i == 6)
                        proxy.RequestType = tds[i].InnerText;
                }

                proxyList.Add(proxy);
            }

            return proxyList;
        }

        public static List<ProxyEntity> GetNianShaoProxy(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection htmlCollection = document.DocumentNode.SelectNodes("//table/tbody/tr");
            var proxyList = new List<ProxyEntity>();
            if (htmlCollection == null)
                return proxyList;

            //遍历列
            foreach (var item in htmlCollection)
            {
                if (item.SelectNodes("td") == null)
                    continue;
                //取列中行 值
                var proxy = new ProxyEntity();

                var tds = item.SelectNodes("td");
                for (int i = 0; i < tds.Count(); i++)
                {
                    if (i == 0)
                        proxy.IP = tds[i].InnerText;
                    if (i == 1)
                        proxy.Port = tds[i].InnerText;
                    if (i == 2)
                        proxy.CityName = tds[i].InnerText;
                    if (i == 3)
                        proxy.Anonymity = tds[i].InnerText;
                    if (i == 4)
                        proxy.RequestType = tds[i].InnerText;
                }

                proxyList.Add(proxy);
            }

            return proxyList; ;
        }


        public static List<ProxyEntity> Get89ipProxy(string html)
        {
            var array = html.Split(new string[] { "<BR>" }, StringSplitOptions.RemoveEmptyEntries);
            array[0] = null;
            array[array.Length - 1] = null;
            List<ProxyEntity> proxyList = new List<ProxyEntity>();

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                    continue;

                var proxyEntity = new ProxyEntity();
                var value = array[i];
                var proxyArray = value.Split(':');

                proxyEntity.IP = proxyArray[0];
                proxyEntity.Port = proxyArray[1];
                proxyEntity.CityName = "未知";
                proxyEntity.Anonymity = "未知";
                proxyEntity.RequestType = "未知";

                proxyList.Add(proxyEntity);
            }

            return proxyList;
        }

        public static List<ProxyEntity> Get71HttpsProxy(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection htmlCollection = document.DocumentNode.SelectNodes("//table[@class]/tbody/tr");

            var proxyList = new List<ProxyEntity>();

            //遍历列
            foreach (var item in htmlCollection)
            {
                if (item.SelectNodes("td") == null)
                    continue;
                //取列中行 值
                var proxy = new ProxyEntity();

                var tds = item.SelectNodes("td");
                for (int i = 0; i < tds.Count(); i++)
                {
                    if (i == 0)
                        proxy.IP = tds[i].InnerText;
                    if (i == 1)
                        proxy.Port = tds[i].InnerText;
                    if (i == 2)
                        proxy.Anonymity = tds[i].InnerText;
                    if (i == 3)
                        proxy.RequestType = tds[i].InnerText;
                    if (i == 4)
                        proxy.CityName = tds[i].InnerText;
                }

                proxyList.Add(proxy);
            }

            return proxyList;
        }

        /// <summary>
        /// Filter Unusable ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool ValidateProxy(string ip)
        {
            bool result = false;
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            IPAddress ipAddress = IPAddress.Parse(ip);
            var replay = pingSender.Send(ipAddress, 1500);//set timeout time

            if (replay.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                //可用的Proxy
                result = true;
            }

            return result;
        }

        public static void Savetoxml(List<ProxyEntity> proxys)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxyIP.xml");

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(node);
            XmlNode root = xmlDoc.CreateElement("Proxys");
            xmlDoc.AppendChild(root);

            foreach (var proxy in proxys)
            {
                XmlNode childNode = xmlDoc.CreateElement("Proxy");
                root.AppendChild(childNode);
                AppendChildNode(xmlDoc, childNode, "IP", proxy.IP);
                AppendChildNode(xmlDoc, childNode, "Port", proxy.Port);
                AppendChildNode(xmlDoc, childNode, "CityName", proxy.CityName);
                AppendChildNode(xmlDoc, childNode, "Anonymity", proxy.Anonymity);
                AppendChildNode(xmlDoc, childNode, "RequestType", proxy.RequestType);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            xmlDoc.Save(path);

        }


        public static List<ProxyEntity> ReadProxyToXml(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList nodeList = xmlDoc.SelectNodes("//Proxys/Proxy");

            var proxyList = new List<ProxyEntity>();

            foreach (XmlNode node in nodeList)
            {
                var proxy = new ProxyEntity();
                proxy.IP = node.SelectSingleNode("IP").InnerText;
                proxy.Port = node.SelectSingleNode("Port").InnerText;
                proxy.CityName = node.SelectSingleNode("CityName").InnerText;
                proxy.Anonymity = node.SelectSingleNode("Anonymity").InnerText;
                proxy.RequestType = node.SelectSingleNode("RequestType").InnerText;

                proxyList.Add(proxy);
            }

            return proxyList;

        }

        public static void AppendChildNode(XmlDocument xmlDoc, XmlNode parentNode, string nodeName, string nodeText)
        {
            XmlNode xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, nodeName, null);
            xmlNode.InnerText = nodeText;
            parentNode.AppendChild(xmlNode);
        }


    }


    public class ProxyEntity
    {
        public string IP { get; set; }

        public string Port { get; set; }

        public string CityName { get; set; }

        public string Anonymity { get; set; }

        public string RequestType { get; set; }
    }

}
