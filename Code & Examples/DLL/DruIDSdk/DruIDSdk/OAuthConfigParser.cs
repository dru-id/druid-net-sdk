using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using log4net;
using System.Reflection;

namespace DruIDSdk
{
    public class OAuthConfigParser
    {

        //private static Array environment;
        public OAuthConfigFile appEnvironment = null;
        public Dictionary<string, string> sections = new Dictionary<string,string>();
        public String pathLog = "";
        public String pathCache = "";

        public OAuthConfigFile newloadFromFile(String environment, String pathConfigFolder)
        {
            if (pathConfigFolder.Trim().Equals("") ||
                !File.Exists(pathConfigFolder + Path.DirectorySeparatorChar + environment + $"{Path.DirectorySeparatorChar}oauthconf.xml"))
            {
                throw new Exception("The config file is not found or can´t be readable");
            }

            this.appEnvironment = newloadXml(environment, pathConfigFolder + Path.DirectorySeparatorChar);
            return this.appEnvironment;

        }

        public OAuthConfigFile newloadXml(String environment, String fileConfigPath)
        {
            //xml = xml.Trim();
            environment = environment.Trim();
            if (environment.Equals(""))
            {
                throw new Exception("Environment id must be valid");
            }

            OAuthConfigFile oauthConfigFile = new OAuthConfigFile();
            XmlDocument xDoc = new XmlDocument();

            xDoc.Load(fileConfigPath + Path.DirectorySeparatorChar + environment + $"{Path.DirectorySeparatorChar}oauthconf.xml");
            XmlNode nodeRoot = xDoc.SelectSingleNode("/oauth-config");

            //credentials
            ConfigCredentials configCredential = new ConfigCredentials();
            configCredential.clientid = nodeRoot.SelectSingleNode("credentials/clientid").InnerText;
            configCredential.clientsecret = nodeRoot.SelectSingleNode("credentials/clientsecret").InnerText;
            oauthConfigFile.credentials = configCredential;

            //redirections
            oauthConfigFile.redirections = new List<ConfigRedirections>();
            XmlNodeList nodeListRedirections = nodeRoot.SelectNodes("redirections/url");
            foreach (XmlNode currentRedirection in nodeListRedirections)
            {
                ConfigRedirections configRedirection = new ConfigRedirections();
                configRedirection.url = currentRedirection.InnerText;
                configRedirection.type = currentRedirection.Attributes["type"].Value;
                configRedirection.isdefault = false;
                if (currentRedirection.Attributes["default"] != null)
                {
                    if (currentRedirection.Attributes["default"].Value.ToLower() == "true")
                    {
                        configRedirection.isdefault = true;
                    }
                }
                oauthConfigFile.redirections.Add(configRedirection);
            }

            //sections
            oauthConfigFile.sections = new List<ConfigSections>();
            XmlNodeList nodeListSections = nodeRoot.SelectNodes("sections/section");
            foreach (XmlNode currentSection in nodeListSections)
            {
                ConfigSections configSection = new ConfigSections();
                configSection.id = currentSection.Attributes["id"].Value;
                configSection.section = currentSection.InnerText;
                configSection.isdefault = false;
                if (currentSection.Attributes["default"] != null)
                {
                    if (currentSection.Attributes["default"].Value.ToLower() == "true")
                    {
                        configSection.isdefault = true;
                    }
                }
                oauthConfigFile.sections.Add(configSection);
            }

            //hosts
            oauthConfigFile.hosts = new List<ConfigHosts>();
            XmlNodeList nodeListHosts = nodeRoot.SelectNodes("hosts/host");
            foreach (XmlNode currentHost in nodeListHosts)
            {
                ConfigHosts configHost = new ConfigHosts();
                configHost.id = currentHost.Attributes["id"].Value;
                configHost.host = currentHost.InnerText;
                oauthConfigFile.hosts.Add(configHost);
            }

            //endpoints
            oauthConfigFile.endPoints = new List<ConfigEndPoints>();
            XmlNodeList nodeListEndPoints = nodeRoot.SelectNodes("endpoints/url");
            foreach (XmlNode currentEndPoint in nodeListEndPoints)
            {
                ConfigEndPoints configEndPoint = new ConfigEndPoints();
                configEndPoint.id = currentEndPoint.Attributes["id"].Value;
                configEndPoint.url = currentEndPoint.InnerText;
                oauthConfigFile.endPoints.Add(configEndPoint);
            }

            //apis
            oauthConfigFile.apis = new List<ConfigApis>();
            XmlNodeList nodeListApis = nodeRoot.SelectNodes("apis/api");
            foreach (XmlNode currentApi in nodeListApis)
            {
                String api_urlbase = "";
                if (currentApi.Attributes["base-url"] != null)
                {
                    api_urlbase = currentApi.Attributes["base-url"].Value;
                }
                foreach(XmlNode currentUrl in currentApi.ChildNodes)
                {
                    ConfigApis configApi = new ConfigApis();
                    configApi.id = currentUrl.Attributes["id"].Value;
                    configApi.url = api_urlbase + currentUrl.InnerText;
                    oauthConfigFile.apis.Add(configApi);
                }
            }

            return oauthConfigFile;
        }




        public void getPathFolders(String pathConfig)
        { 
            try{
                if(!File.Exists(pathConfig + $"{Path.DirectorySeparatorChar}druidconfig.xml"))
                {
                    throw new Exception("druidconfig.xml not found");
                }
                
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(pathConfig + $"{Path.DirectorySeparatorChar}druidconfig.xml");
                this.pathCache = pathConfig + Path.DirectorySeparatorChar + xDoc.SelectSingleNode("DruID/CachePath").LastChild.Value;
                this.pathLog = pathConfig + Path.DirectorySeparatorChar + xDoc.SelectSingleNode("DruID/LogPath4NetConfig").LastChild.Value;
                
                xDoc = null;
            }
            catch(Exception e)
            {
                throw new Exception("getPathFolders errors: " + e.Message);
            }
        }

    }

    public class ConfigCredentials
    {
        public string clientid { get; set; }
        public string clientsecret { get; set; }
    }
    public class ConfigRedirections
    {
        public string type { get; set; }
        public string url { get; set; }
        public bool isdefault { get; set; }
    }
    public class ConfigSections
    {
        public string id { get; set; }
        public string section { get; set; }
        public bool isdefault { get; set; }
    }
    public class ConfigEndPoints
    {
        public string id { get; set; }
        public string url { get; set; }
    }
    public class ConfigApis
    {
        public string id { get; set; }
        public string url { get; set; }
    }
    public class ConfigHosts
    {
        public string id { get; set; }
        public string host { get; set; }
    }
    public class OAuthConfigFile
    {
        public ConfigCredentials credentials { get; set; }
        public List<ConfigRedirections> redirections { get; set; }
        public List<ConfigSections> sections { get; set; }
        public List<ConfigHosts> hosts { get; set; }
        public List<ConfigEndPoints> endPoints { get; set; }
        public List<ConfigApis> apis { get; set; }
    }
}
