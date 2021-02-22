using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using System.Reflection;

namespace DruIDSdk
{
    public class OAuthClient
    {

        public OAuthClient(string client_id, string client_secret, int client_auth = AUTH_TYPE_URI)
        {
            this.setClientId(client_id);
            this.setClientSecret(client_secret);
            this.setClientAuthType(client_auth);
            
        }
        /** Different AUTH method */
	    public const int AUTH_TYPE_URI                 = 0;
        public const int AUTH_TYPE_AUTHORIZATION_BASIC = 1;
        public const int AUTH_TYPE_FORM = 2;

	    /** Different Access token type */
	    const int ACCESS_TOKEN_URI      = 0;
	    const int ACCESS_TOKEN_BEARER   = 1;
	    const int ACCESS_TOKEN_OAUTH    = 2;
	    const int ACCESS_TOKEN_MAC      = 3;

	    /** Different Grant types */
        public const String GRANT_TYPE_AUTH_CODE = "authorization_code";
	    public const String GRANT_TYPE_PASSWORD           = "password";
	    public const String GRANT_TYPE_CLIENT_CREDENTIALS = "client_credentials";
//	    const GRANT_TYPE_CLIENT_CREDENTIALS = 'none';
        public const String GRANT_TYPE_REFRESH_TOKEN = "refresh_token";

	    /** HTTP Methods */
        public const String HTTP_METHOD_GET = "GET";
        public const String HTTP_METHOD_POST = "POST";
        public const String HTTP_METHOD_PUT = "PUT";
        public const String HTTP_METHOD_DELETE = "DELETE";
        public const String HTTP_METHOD_HEAD = "HEAD";

	    const String LOG_TAG_ERROR_APIS = "class.OAuthClient:";

	    /** @var string $client_id Client ID. */
	    protected static string client_id = null;
	    /** @var string $client_secret Client Secret. */
	    protected static string client_secret = null;
	    /** @var integer $client_auth Client Authentication method */
	    protected static int client_auth = AUTH_TYPE_URI;
	    /** @var string $access_token Access Token */
	    protected string access_token = null;
	    /** @var integer $access_token_type Access Token Type */
	    protected int access_token_type = ACCESS_TOKEN_URI;
	    /** @var string $access_token_secret Access Token Secret */
	    protected string access_token_secret = null;
	    /** @var string $access_token_algorithm Access Token crypt algorithm */
	    protected string access_token_algorithm = null;
	    /** @var string $access_token_param_name Access Token Parameter name */
	    protected string access_token_param_name = "access_token";

	    /**
	     * Asigna el ID del cliente de la aplicación.
	     *
	     * @param string $client_id El ID del cliente (aplicación)
	     * @return void
	     */
	    public void setClientId(string p_client_id)
	    {
            client_id = p_client_id.Trim();
	    }
	    /**
	     * Devuelve el ID del cliente de la aplicación.
	     *
	     * @return string El ID del cliente de la aplicación.
	     */
	    public string  getClientId()
	    {
	    	return client_id;
	    }

	    /**
	     * Asigna el "client_secret" de la aplicación.
	     *
	     * @param string $client_secret El código secreto.
	     * @return void
	     */
	    public void setClientSecret(string p_client_secret)
	    {
	    	client_secret = p_client_secret.Trim();
	    }
	    /**
	     * Devuelve el "client_secret" de la aplicación.
	     *
	     * @return string El client_secret de la aplicación.
	     */ 
	    public string getClientSecret()
	    {
	    	return client_secret;
	    }
        
        /**
	     * Asigna el tipo de autenticación del cliente.
	     *
	     * @param string $client_auth Cualquiera de los siguientes: {@link OAuthClient::AUTH_TYPE_URI}, {@link OAuthClient::AUTH_TYPE_FORM},
	     * 		{@link OAuthClient::AUTH_TYPE_AUTHORIZATION_BASIC}
	     * @return void
	     */
	    public void setClientAuthType(int p_client_auth)
	    {
	    	switch (p_client_auth) {
	    		case AUTH_TYPE_URI:
	    		case AUTH_TYPE_FORM :
	    		case AUTH_TYPE_AUTHORIZATION_BASIC:
	    			client_auth = p_client_auth;
	    			break;
	    		default:
	    			client_auth = AUTH_TYPE_URI;
                    break;
	    	}
	    }

	    /**
	     * Devuelve el tipo de autenticación del cliente.
	     *
	     * @param string $client_auth (AUTH_TYPE_URI, AUTH_TYPE_AUTHORIZATION_BASIC, AUTH_TYPE_FORM)
	     * @return integer Cualquiera de los siguientes: {@link OAuthClient::AUTH_TYPE_URI}, {@link OAuthClient::AUTH_TYPE_FORM}, {@link OAuthClient::AUTH_TYPE_AUTHORIZATION_BASIC}
	     */
	    static public int getClientAuthType()
	    {
	        return client_auth;
	    }

	    /**
	     * Ejecuta una petición con CURL.
	     *
	     * @param string $url La URL donde lanzamos la petición.
	     * @param mixed $parameters Vector con los parámetros que queremos lanzar. Es asociativo, usando la clave como nombre del parámetro y el valor
	     * 		como valor ('oviousli')
	     * @param string $http_method El método HTTP usado para lanzar la petición. Admite cualquiera de éstos valores:
	     * 		- {@link OAuthClient::HTTP_METHOD_GET}
	     * 		- {@link OAuthClient::HTTP_METHOD_POST}
	     * 		- {@link OAuthClient::HTTP_METHOD_HEAD}
	     * 		- {@link OAuthClient::HTTP_METHOD_PUT}
	     * 		- {@link OAuthClient::HTTP_METHOD_DELETE}
	     * @param mixed $http_headers Un vector con las cabeceras que queremos incluir en la petición o FALSE si no queremos enviar cabeceras HTTP.
	     * @param mixed $cookies Un vector con todas las cookies que hay que enviar o FALSE si no se va a enviar ninguna cookie. Una línea por cookie, con
	     * 		el formato "nombre=valor". Sin punto y coma (;) al final.
	     *
	     * @return array
	     */

        public JObject executeRequest(string url, Dictionary<string, string> parameters, string http_method = HTTP_METHOD_GET, Boolean http_headers = false, string cookies = "")
	    {
            Boolean is_ssl=false;
            Dictionary<string,  object> dictResult = new Dictionary<string, object>();
            Dictionary<string, string> dictJson = new Dictionary<string, string>();
	    

            if(url.IndexOf("https")>=0){
                is_ssl=true;
	    	}

            string strResult = "";


            strResult = WebRequest(Method.POST, url, String.Join("&", parameters.Select(x => String.Format("{0}={1}", x.Key, x.Value)).ToArray()), "", cookies);
            JObject json = new JObject();
            if(!string.IsNullOrEmpty( strResult)){
                json= JObject.Parse(strResult);
            }
            
            return json;
        }

        public enum Method { GET, POST };
        public const string AUTHORIZE = "https://savvistest.cocacola.es/oauth2/authorize";
        public const string ACCESS_TOKEN = "https://savvistest.cocacola.es/oauth2/token";
        public const string CALLBACK_URL = "http://docs.cocacola.es/postlogin";

        private string _consumerKey = "";
        private string _consumerSecret = "";
        private string _token = "";

        #region Properties

 

        public string Token { get { return _token; } set { _token = value; } }

        #endregion


  

        /// <summary>
        /// Web Request Wrapper
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="url">Full url to the web resource</param>
        /// <param name="postData">Data to post in querystring format</param>
        /// <returns>The web server response.</returns>
        static public string WebRequest(Method method, string url, string postData, string useragent, string strCookie="")
        {

            HttpWebRequest webRequest = null;
            StreamWriter requestWriter = null;
            string responseData = "";

            webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.UserAgent = useragent;
            webRequest.Timeout = 20000;
            
            if(strCookie.IndexOf("=")>0){
                webRequest.Headers["Cookie"] = strCookie;
                /*try
                {
                    webRequest.CookieContainer = new CookieContainer();
                    char[] splitchar = { '=' };
                    Cookie cookie = new Cookie(strCookie.Split(splitchar)[0], strCookie.Split(splitchar)[1]);
                    webRequest.CookieContainer.Add(new Uri("/"), cookie);
                }
                catch
                {
                    throw;
                }*/
            }
           

            //webRequest.CookieContainer.Add(
            //Cookie cookie =new Cookie(
            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                //POST the data.
                requestWriter = new StreamWriter(webRequest.GetRequestStream());

                try
                {
                    requestWriter.Write(postData);
                }
                catch
                {
                    throw;
                }

                finally
                {
                    requestWriter.Close();
                    requestWriter = null;
                }
            }

            responseData = WebResponseGet(webRequest);
            webRequest = null;
            return responseData;
        }

        /// <summary>
        /// Process the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <returns>The response data.</returns>
        static public string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            string responseData = "";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback =

              delegate(object s
                  , X509Certificate certificate
                  , X509Chain chain
                  , SslPolicyErrors sslPolicyErrors)

              { return true; };
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    webRequest = null;
                }
            }
            finally
            {
                if (webRequest != null)
                {
                    webRequest.GetResponse().GetResponseStream().Close();
                    responseReader.Close();
                }
                
                responseReader = null;
            }

            return responseData;
        }
        public long time()
        {
            return time(DateTime.UtcNow);
        }

        public long time(DateTime time)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = time.Subtract(unixEpoch);

            return (long)span.TotalSeconds;
        }

        public Dictionary<string, string> deserializeSimpleDict(string strToken)
        {
            
            
 
            Dictionary<string, string> returnDeserialized = new Dictionary<string,string>();
            returnDeserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(strToken);


            return returnDeserialized;
        }

    }
}
