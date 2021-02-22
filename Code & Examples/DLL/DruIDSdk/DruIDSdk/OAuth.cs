using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using System.Reflection;
namespace DruIDSdk
{
    public class OAuth:OAuthClient
    {
   

        public OAuth(string client_id, string client_secret, int client_auth = OAuthClient.AUTH_TYPE_URI)
            : base(client_id, client_secret, client_auth = OAuthClient.AUTH_TYPE_URI)
        {
            
        }  
  
        public enum Method { GET, POST };
        public const string AUTHORIZE = "https://savvistest.cocacola.es/oauth2/authorize";
        public const string ACCESS_TOKEN = "https://savvistest.cocacola.es/oauth2/token";
        public const string CALLBACK_URL = "http://docs.cocacola.es/postlogin";

        private string _consumerKey = "";
        private string _consumerSecret = "";
        private string _token = "";
        		/** Tipos de permisos. */
//		const GRANT_TYPE_CLIENT_TOKEN = 'none'; # @@ISMA@@ (2011-10-25) - Ya no se usa 'none'. En su lugar hay que usar 'client_credentials'
		const string GRANT_TYPE_VALIDATE_BEARER = "urn:es.cocacola:oauth2:grant_type:validate_bearer";
		const string GRANT_TYPE_EXCHANGE_SESSION = "urn:es.cocacola:oauth2:grant_type:exchange_session";

		/** Lugares donde se pueden almacenar los tokens. Se utiliza para determinar dónde buscar el token almacenado. */
		const int SESSION = 1;
		const int COOKIE = 2;
		const int BOTH = 3;

		/** Número de segundos que usamos por defecto cuando no nos pasan la caducidad de las cosas. */
		const int DEFAULT_EXPIRES_IN = 900;
		/** Indica el porcentaje que hay que restar del número de segundos de "expires_in" para que no estar tan cerca de la fecha de caducidad
		 * 		del token. */
		const double SAFETY_RANGE_EXPIRES_IN = 0.10;

		/** Nombre de la cookie que gestiona el SSO (Single Sign-Out). */
		const string SSO_COOKIE_NAME = "datr";

        private string user_id = "";

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
        static public string WebRequest(Method method, string url, string postData, string useragent)
        {

            HttpWebRequest webRequest = null;
            StreamWriter requestWriter = null;
            string responseData = "";

            webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.UserAgent = useragent;
            webRequest.Timeout = 20000;

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
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch
            {
                throw;
            }
            finally
            {
                webRequest.GetResponse().GetResponseStream().Close();
                responseReader.Close();
                responseReader = null;
            }

            return responseData;
        }
        
        
        /**
        * JFT Construye la url a partir de un array
        */
         public string http_build_query(Dictionary<string, string> dicParams, string separator)
        {
            
            string strParams = "";

            foreach (string key in dicParams.Keys)
            {
                strParams += separator + key + "=" + dicParams[key];
            }
            if (strParams.Length > 0)
            {
                strParams = strParams.Substring(1);
            }

            return strParams;
        }


    /**
     * Construye la URL para el login
     *
     * Ésta acción require de la intervención activa del usuario, por tanto es necesario generar una petición con parámetros pasados por GET
     * para llevarla a cabo.
     *
     * @param string $endpoint_url Normalmente es el 'authorization_endpoint' del servidor oauth
     * @param string $redirect_url Donde devuelve al usuario cuando finaliza el proceso, tanto si se conecta o se deniega la conexión
     * @param string $scope Sección section_key identificador de la sección dentro del cliente web
     *
     * @return string La URL generada para la página de autorización
     *
     * @throws Exception Si alguno de los parámetros no es válido.
     */
        public string buildLoginUrl(string endpoint_url, string redirect_url, string scope = null)
        {
            try
            {
                if (endpoint_url.Trim() == "")
                {
                    throw new Exception("Endpoint URL is empty");
                }
                if (redirect_url.Trim() == "")
                {
                    throw new Exception("Redirect URL is empty");
                }

                endpoint_url = endpoint_url.Replace("?", "");
                
                
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
                dicParams["client_id"] = this.getClientId();
                dicParams["redirect_uri"] = redirect_url;
                dicParams["response_type"] = "code";
                if (!string.IsNullOrEmpty(scope))
                {
                    dicParams["scope"] = scope;
                }


                return endpoint_url + "?" + this.http_build_query(dicParams, "&");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
		/**
		 * Construye la URL para lanzar el proceso de alta del usuario.
		 *
		 * Ésta acción require de la intervención activa del usuario, por tanto es necesario generar una petición con parámetros pasados por GET
		 * para llevarla a cabo.
		 *
		 * @param string $endpoint_url A donde apuntará el enlace, normalmente será 'signup_endpoint' del servidor oauth
		 * @param string $client_id El ID de la aplicacion que lanza la petición
		 * @param string $redirect_url Donde devuelve al usuario cuando finaliza el proceso, tanto si acepta registrarse como si no.
		 *
		 * @return string La URL generada para el registro
		 *
		 * @throws Exception Si alguno de los parámetros no es válido.
		 */
		public string buildSignupUrl(string endpoint_url, string redirect_url, string scope = null){
			try {
				// La petición es la misma que para el login, más un parámetro adicional. Aprovechamos lo que hay hecho.
				string url="";
                url =  buildLoginUrl(endpoint_url, redirect_url);

                if (url.Equals("")) {
					throw new Exception("Can't build sign up URL");
				}
		
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
                dicParams["x_method"] = "sign_up";
                if (!string.IsNullOrEmpty(scope))
                {
                    dicParams["scope"] = scope;
                }

				return url + '&' + this.http_build_query(dicParams, "&");
			} catch(Exception e) {
				throw e;
			}
		}

		/**
		 * Recupera la información de un token determinado.
		 *
		 * Prevalece SESSION frente a COOKIE. Si encuentra en el primero entonces no recupera el segundo.
		 *
		 * @param string $name El nombre del token que queremos borrar. El listado de nombres disponible está definido en {@link iTokenTypes}
		 * @param string $source Donde buscar el toquen que queremos recuperar. Cualquiera de estos valores:
		 * 	- {@link OAuthJQC::SESSION}
		 * 	- {@link OAuthJQC::COOKIE}
		 * 	- {@link OAuthJQC::BOTH}
		 *
		 * @return mixed Un objeto {@link StoredToken} con los datos del token o FALSE si no se ha podido recuperar.
		 */
		public StoredToken getStoredToken(string name, int source = BOTH)
		{
			if ((name = name.Trim()).Equals("")) {
				return null;
			}
            //string test=HttpContext.Current.Session["test"].ToString();
			// Primero busco en sesión.
			//if (((source == BOTH) || (source == SESSION)) && isset($_SESSION[$name]) && (($temp = unserialize($_SESSION[$name])) instanceof StoredToken)) {
            if(HttpContext.Current.Session[name]!=null){
                string temp=HttpContext.Current.Session[name].ToString();
                if ((source == BOTH) || (source == SESSION)) {
                    return StoredToken.factory(name, HttpContext.Current.Session[name].ToString(), 0, default(DateTime), "/");
			    }
            }            
            
			// Si no tengo en sesión, busco en cookie.
			//JFT revisar encriptacion de la cookie
            //$encryption = new Encryption($this->client_id);
			if ((source == BOTH || source == COOKIE) &&  HttpContext.Current.Request.Cookies[name]!=null) {
                Encryption crypt = new Encryption(this.getClientId());
                string decryptedString = "";
                if (!string.IsNullOrEmpty(HttpContext.Current.Request.Cookies[name].Value))
                {
                    decryptedString = crypt.Decrypt(HttpContext.Current.Request.Cookies[name].Value);
                }

                return StoredToken.factory(name, decryptedString, 0, default(DateTime), "/");//StoredToken::factory($name, $encryption->decode($_COOKIE[$name]['value']), 0, 0, '/');				
			}
			return null;
		}
		/**
		 * Construye la URL para entrar en la zona de edición de los datos del usuario.
		 *
		 * Ésta acción require de la intervención activa del usuario, por tanto es necesario generar una petición con parámetros pasados por GET
		 * para llevarla a cabo.
		 *
		 * @param string $endpoint_url A donde apuntará el enlace, normalmente es el 'edit_account_endpoint'
		 * @param string $next_url Dónde se redirige al usuario cuando termine el proceso de edición de datos
		 * @param string $cancel_url Dónde se redirige al usuario si cancela el proceso de edición.
		 ** @param string $scope Sección section_key identificador de la sección dentro del cliente web
		 * @return string La URL generada.
		 *
		 * @throws Exception Si alguno de los parámetros no es válido.
		 */
		public string buildEditAccountUrl(string endpoint_url, string next_url, string cancel_url, string scope = null) {
			try {
				if ((endpoint_url = endpoint_url.Trim()).Equals("")) {
					throw new Exception ( "Endpoint URL is empty" );
				}
				if ((next_url = next_url.Trim()).Equals("")) {
					throw new Exception ( "Next URL is empty" );
				}
				if ((cancel_url = cancel_url.Trim()).Equals("")) {
					throw new Exception ( "Cancel URL is empty" );
				}
                AccessToken  access_token = (AccessToken) this.getStoredToken( StoredToken.ACCESS_TOKEN );
                System.Diagnostics.Debug.WriteLine(access_token.ToString());
                //return access_token.getValue();
				if (access_token.getValue().Equals("")) {
					throw new Exception ( "Access token is empty" );
				}
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
                dicParams["next"] = next_url;
                dicParams["cancel_url"] = cancel_url;
                dicParams["oauth_token"] = access_token.getValue();
				/*$endpoint_url = rtrim ( $endpoint_url, '?' );
				$params = array ();
				$params ['next'] = $next_url;
				$params ['cancel_url'] = $cancel_url;
				$params ['oauth_token'] = $access_token->getValue ();
				if (!is_null($scope)){
					$params ['scope'] = $scope;
				}
				unset ( $access_token );
				
				return $endpoint_url . '?' . http_build_query ( $params, null, '&' );
                 */
                 return endpoint_url + "?" + this.http_build_query(dicParams, "&");
			} catch ( Exception e ) {				
				throw e;
			}
		}

		/**
		 * Construye la URL para completar los datos de una sección.
		 *
		 * @param string $endpoint_url A donde apuntará el enlace, normalmente es el 'edit_account_endpoint'
		 * @param string $next_url Dónde se redirige al usuario cuando termine el proceso de edición de datos
		 * @param string $cancel_url Dónde se redirige al usuario si cancela el proceso de edición.
		 * @param string $scope Section sección en la que faltan por completar datos
		 *
		 * @return string La URL generada.
		 *
		 * @throws Exception Si alguno de los parámetros no es válido.
		 */
		public string  buildCompleteAccountUrl(string endpoint_url, string next_url, string cancel_url, string scope=null) {
			try {
				if ((endpoint_url = endpoint_url.Trim()).Equals("")) {
					throw new Exception ( "Endpoint URL is empty" );
				}
				if ((next_url = next_url.Trim()).Equals("")) {
					throw new Exception ( "Next URL is empty" );
				}
				if ((cancel_url = cancel_url.Trim()).Equals("")) {
					throw new Exception ( "Cancel URL is empty" );
				}
                AccessToken  access_token = (AccessToken) this.getStoredToken( StoredToken.ACCESS_TOKEN );
                System.Diagnostics.Debug.WriteLine(access_token.ToString());
                //return access_token.getValue();
				if (access_token.getValue().Equals("")) {
					throw new Exception ( "Access token is empty" );
				}
		
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
                dicParams["next"] = next_url;
                dicParams["cancel_url"] = cancel_url;
                dicParams["oauth_token"] = access_token.getValue();
                if (!string.IsNullOrEmpty(scope))
                {
                    dicParams["scope"] = scope;
                }

                return endpoint_url + "?" + this.http_build_query(dicParams, "&");
			} catch ( Exception e ) {
				throw e;
			}
		}

	    /**
	     * Comienza el proceso de revocación de los tokens.
	     *
	     * Debido a que los tokens de acceso se pueden generar en otra aplicación que no sea JQC, es necesario
	     * revocar los tokens de acceso a nivel global, ya que si se desconecta en una aplicación, tendra que hacerlo
	     * en todas las restantes.
	     *
	     * @param string $endpoint_url Donde hay que lanzar el proceso de revocación.
	     * @return void
	     *
	     * @throws Exception Si hay algún problema durante el proceso.
	     */
	    public void doLogout(string endpoint_url) {
		    try {
			    // Verifico los parámetros necesarios.
			    if ((endpoint_url = endpoint_url.Trim()).Equals("")) {
				    throw new Exception ( "Endpoint URL is empty" );
			    }
			    if (this.getClientId().Equals("")) {
				    throw new Exception ( "Client ID is empty" );
			    }
			    if (this.getClientSecret().Equals("")) {
				    throw new Exception ( "Client secret is empty" );
			    }
			    if (this.getStoredToken(StoredToken.REFRESH_TOKEN)==null){
				    throw new Exception ( "Refresh token is empty" );
			    }
				
                Dictionary<string, string> dictParams = new Dictionary<string, string>( );
                dictParams.Add("token", this.getStoredToken(StoredToken.REFRESH_TOKEN).getValue());
                dictParams.Add("token_type", "refresh_token");
                dictParams.Add("client_id", this.getClientId());
                dictParams.Add("client_secret", this.getClientSecret());

                JObject response;
                response=this.executeRequest(endpoint_url,dictParams, HTTP_METHOD_POST);		
			    /*$params = array ();
			    $params ['token'] = $refresh_token->getValue ();
			    $params ['token_type'] = 'refresh_token';
			    $params ['client_id'] = $this->client_id;
			    $params ['client_secret'] = $this->client_secret;
			    //unset ( $access_token );
			    $response = $this->executeRequest ( $endpoint_url, $params, self::HTTP_METHOD_POST );
			    */
			    this.checkErrors (response);
			
			    // Eliminamos cookies (access,refresh)
			    this.purgeTokens ();
			    //return true;
		    } catch ( Exception e ) {			
			    throw e;
		    }
	    }

		/**
		 * Elimina todos los tokens almacenados en sesión.
		 *
		 * @param mixed $redirect Una cadena de texto con la URL donde debemos redirigir al usuario al purgar
		 * 		los tokens o FALSE si no queremos redirigir. El pasar una URL no asegura que redirigirá al
		 * 		usuario, depende de si ya se ha enviado información al navegador o no.
		 *
		 * @return void
		 */
		public void purgeTokens(string redirect = "")
		{
			this.deleteStoredToken(StoredToken.ACCESS_TOKEN);
			this.deleteStoredToken(StoredToken.REFRESH_TOKEN);
			
			//No es necesario eliminar el client_token

			/*if ((this.hasToken(StoredToken.ACCESS_TOKEN, COOKIE) || this.hasToken(StoredToken.CLIENT_TOKEN, COOKIE) || this.hasToken(StoredToken.REFRESH_TOKEN, COOKIE)) && !headers_sent() && (redirect.Equals("")) && ((redirect = trim((string)$redirect)) != '')) {
				header('Location: '.$redirect);
				die;
			}*/
		}

		/**
		 * Busca y borra un token determinado de los almacenes persistentes (SESSION y COOKIE)
		 *
		 * @param string $name El nombre del token que queremos borrar. El listado de nombres disponible está definido en {@link iTokenTypes}
		 * @return void
		 */
		public void deleteStoredToken(string name){
			
			// Todo se basó en los datos almacenados en la sesión.
			
            if (!string.IsNullOrEmpty(name) &&  HttpContext.Current.Session[name] !=null ) {
				//Cojo los datos para borrar la cookie.
				
                if (HttpContext.Current.Request.Cookies[name]!=null) {
					HttpContext.Current.Request.Cookies.Remove(name);
                    //$result = setcookie($name, '', time()-42000, $stored_token->getPath());
					//unset($_COOKIE[$name]);
				}

				// Borramos el token en sesión.
				//Console::log ('['.__CLASS__.']['.__FUNCTION__.']['.__LINE__.'] procedemos a borrar sesión'.$name);
				HttpContext.Current.Session.Remove(name);
                //unset($_SESSION[$name]);
				
			} else {
				// Si por algún motivo no tenemos el token en sesión, comprobamos si lo tenemos en COOKIE. El problema que tenemos
				// con ésto es que no sabemos el "path" de la cookie (ya que no se puede sacar de la cookie y no lo tenemos en
				// sesión), por lo que si está fuera del raíz no podríamos borrarla. Lo intentamos de todas maneras.
				if (HttpContext.Current.Request.Cookies[name]!=null) {
					HttpContext.Current.Request.Cookies.Remove(name);
					
				}
			}
		}
		 public LoginStatus doLoginStatus(string endpoint_url) {
			try {
				if ((endpoint_url = endpoint_url.Trim()).Equals("")) {
					throw new Exception ( "Endpoint URL is empty" );
				}
				if ( this.getClientId().Equals("")) {
					throw new Exception ( "Client ID is empty" );
				}
				if (this.getClientSecret().Equals("")) {
					throw new Exception ( "Client secret is empty" );
				}
                AccessToken  access_token = (AccessToken) this.getStoredToken ( StoredToken.ACCESS_TOKEN );
				
                if (access_token==null|| (access_token.getValue().Equals(""))) {
					throw new Exception ( "Access token is empty" );
				}
				
				// Lanzamos la petición.
                Dictionary<string, string> dictParams = new Dictionary<string, string>( );
                dictParams.Add("grant_type", GRANT_TYPE_VALIDATE_BEARER);
                dictParams.Add("oauth_token", access_token.getValue());
                dictParams.Add("client_id", this.getClientId());
                dictParams.Add("client_secret", this.getClientSecret());
                access_token = null;

                JObject response;
                response=this.executeRequest(endpoint_url,dictParams, HTTP_METHOD_POST);
				/*$params = array ();
				$params ['grant_type'] = self::GRANT_TYPE_VALIDATE_BEARER;
				$params ['oauth_token'] = $access_token->getValue ();
				$params ['client_id'] = $this->client_id;
				$params ['client_secret'] = $this->client_secret;
				unset ( $access_token );
				$response = $this->executeRequest ( $endpoint_url, $params, self::HTTP_METHOD_POST );
				*/
				this.checkErrors ( response );
												
				LoginStatus loginStatus = new LoginStatus();
				//JFT no me devuelve ningún code
                //if(response.ContainsKey("code") && response["code"].ToString().Equals("200")){

                    if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["login_status"]["uid"]).ToString()))
                    {
                        loginStatus.setUid(response["login_status"]["uid"].ToString());
                        loginStatus.setConnect_state(response["login_status"]["connect_state"].ToString());
                    }
                //}
											
                return loginStatus;
				
			} catch ( Exception e ) {

                return null;
			}
		}				

		/**
		 * Comprueba si el usuario está logeado o no.
		 *
		 * @param string $endpoint_url Donde se lanzará la petición.
		 * @param string $cookie_value El contenido de la cookie donde se registra el SSO.
		 *
		 * @return boolean TRUE si está conectado (y tenemos access_token) o FALSE en caso contrario.
		 *
		 * @throws Exception Si hay algún problema con la peticion.
		 */
		public AccessToken doCheckLoginConnect(string endpoint_url, string cookie_value) {
			try {


				AccessToken access_token = null;
                RefreshToken refresh_token = null;
				
				// Verificación de parámetros.
				if ((endpoint_url = endpoint_url.Trim()).Equals("")) {
					throw new Exception ( "Endpoint URL is empty" );
				}
				if (this.getClientId().Trim().Equals("")) {
					throw new Exception ( "Client ID is empty" );
				}
				if (this.getClientSecret().Trim().Equals("")) {
					throw new Exception ( "Client secret is empty" );
				}
				if ((cookie_value = cookie_value.Trim()).Equals("")) {
					throw new Exception ( "SSO cookie is empty" );
				}
				
				// Lanzamos la petición.
                Dictionary<string, string> dictParams = new Dictionary<string, string>( );
                dictParams.Add("grant_type", GRANT_TYPE_EXCHANGE_SESSION);
                dictParams.Add("client_id", this.getClientId());
                dictParams.Add("client_secret", this.getClientSecret());

                JObject response;
                response = this.executeRequest(endpoint_url, dictParams, HTTP_METHOD_POST, false, "datr=" + cookie_value);
				/*$params = array ();
				$params ['grant_type'] = self::GRANT_TYPE_EXCHANGE_SESSION;
				$params ['client_id'] = $this->client_id;
				$params ['client_secret'] = $this->client_secret;
				$response = $this->executeRequest ( $endpoint_url, $params, self::HTTP_METHOD_POST, false, array ('datr=' . $cookie_value ) );
				*/
				this.checkErrors ( response );
				
				// Identifico si estoy conectado. Si lo estoy, entonces deberia tener tanto un "access_token" como un "refresh_token" válido,
				// por tanto compruebo este dato.
                bool is_connected=false;
                if (response.Count > 0)
                {
                    if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["login_status"]["connect_state"]).ToString()))
                    {
                        if (response["login_status"]["connect_state"].ToString().Equals("connected"))
                        {
                            is_connected = true;
                        }
                    }
                }
                

				if (is_connected) {
					// Verifico si dispongo de los tokens necesarios.
                    if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["access_token"]).ToString()))
                    {
                        throw new Exception ( "The client_token retrieved is empty" );
                    }

                    if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["refresh_token"]).ToString()))
                    {
                        throw new Exception ( "The client_token retrieved is empty" );
                    }
					
					// Me fio más del tiempo de segundos que faltan para que caduque el token que de la fecha que devuelve DruID. Ésta no está
					// representada en GMT, sino que depende de la hora del servidor. Si nosotros no tenemos establecido la misma hora estamos jodidos.
					int expires_in = DEFAULT_EXPIRES_IN; // 900 segundos = 15 minutos
                    if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expiresIn"]).ToString())){
					    expires_in = int.Parse(response ["expiresIn"].ToString());
				    } else if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expiresIn"]).ToString())){
                        expires_in = int.Parse(response["expires_in"].ToString());
				    }

				    // Resto un porcentaje del total para asegurarnos de que nuestra fecha de caducidad calculada es menor que la devuelta por el servidor.
				    // Ésto permite evitar errores de "invalid tokens".
				    expires_in = (int)((double)expires_in - ((double)expires_in * SAFETY_RANGE_EXPIRES_IN));
				
				    DateTime expires_at = DateTime.Now.AddSeconds(expires_in);
					
					// Guardo los tokens en soportes persistentes.
					access_token = new AccessToken ( response["access_token"].ToString().Trim(), expires_in, expires_at, "/" );
					refresh_token = new RefreshToken ( response["refresh_token"].ToString().Trim(), 0, default(DateTime), "/" );
					this.storeToken ( access_token );
					this.storeToken ( refresh_token );
					
					// Si tengo el ID del usuario, aprovecho y me lo quedo.
                    if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["login_status"]["uid"]).ToString()))
                    {
                        this.user_id = response["login_status"]["uid"].ToString();
                    }
				}
				return access_token;
			} catch ( Exception e ) {				
				throw e;
			}
		}
		/**
		 * Comprueba si tenemos el token solicitado.
		 *
		 * @param string $name El nombre del token. Todos los tipos están definidos en {@link iTokenTypes}
		 * @param string $source Donde buscar el toquen que queremos recuperar. Cualquiera de éstos valores:
		 * 	- {@link OAuthJQC::SESSION}
		 * 	- {@link OAuthJQC::COOKIE}
		 * 	- {@link OAuthJQC::BOTH}
		 *
		 * @return boolean TRUE si existe o FALSE en caso contrario.
		 */
		public StoredToken hasToken(string name, int source = BOTH)
		{
            
			return this.getStoredToken(name, source);
		}

		/**
		 * Comprueba si el token ha caducado en base a la fecha actual del sistema.
		 *
		 * @return boolean TRUE si ha caducado, FALSE en caso contrario.
		 */
		public Boolean tokenExpired(string name)
		{
            StoredToken temp=this.getStoredToken(name);
			if (temp==null) {
				return false;
			}

			return (  DateTime.Now >  temp.getExpiresAt());
		}

		/**
		 * Recupera el "client_token" del cliente.
		 * 
		 * No es necesario estar logado para tener el client_token
		 *
		 * @param string $endpoint_url Donde vamos a solicitar la información.
		 * @return mixed Un objeto {@link ClientToken} con los datos recuperados. En caso de fallo se lanza una excepción.
		 *
		 * @throws Exception Si hay algún problema durante el proceso.
		 */
		public ClientToken doGetClientToken(string endpoint_url) {
			try {
                if ((endpoint_url = endpoint_url.Trim()).Equals(""))
                {
					throw new Exception ( "Endpoint URL is empty" );
				}
				if (this.getClientId().Equals("")) {
					throw new Exception ( "Client ID is empty" );
				}
				if (this.getClientSecret().Equals("")) {
					throw new Exception ( "Client secret is empty" );
				}
				
                Dictionary<string, string> dictParams = new Dictionary<string, string>( );
                dictParams.Add("grant_type", GRANT_TYPE_CLIENT_CREDENTIALS);
                dictParams.Add("client_id", this.getClientId());
                dictParams.Add("client_secret", this.getClientSecret());

                JObject response;
                response=this.executeRequest(endpoint_url,dictParams, HTTP_METHOD_POST);

				// Lanzo la petición.
				/*$params = array ();
				$params ['grant_type'] = self::GRANT_TYPE_CLIENT_CREDENTIALS;
				$params ['client_id'] = $this->client_id;
				$params ['client_secret'] = $this->client_secret;
				$response = $this->executeRequest ( $endpoint_url, $params, self::HTTP_METHOD_POST );
				*/				
				this.checkErrors ( response );
				
				// Almaceno los datos recuperados.
				// No está mal, según la documentación del DruID, el client_token es un access_token especial, que sirve para identificar el cliente.
                if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["access_token"]).ToString())){
                    throw new Exception ( "The client_token retrieved is empty" );
                }

				
				int expires_in = DEFAULT_EXPIRES_IN; // 900 segundos = 15 minutos
                if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expiresIn"]).ToString())){
					expires_in = int.Parse(response ["expiresIn"].ToString());
				} else if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expires_in"]).ToString())){
                    expires_in = int.Parse(response["expires_in"].ToString());
				}

				// Resto un porcentaje del total para asegurarnos de que nuestra fecha de caducidad calculada es menor que la devuelta por el servidor.
				// Ésto permite evitar errores de "invalid tokens".
				expires_in = (int)((double)expires_in - ((double)expires_in * SAFETY_RANGE_EXPIRES_IN));
				
				
                DateTime expires_at = DateTime.Now.AddSeconds(expires_in);
                ClientToken client_token = new ClientToken(response["access_token"].ToString().Trim(), expires_in, expires_at, "/");
                this.storeToken(client_token);
				//$this->storeToken ( $client_token );
								
				
				return client_token;
			} catch ( Exception e ) {
				
				throw e;
			}
		}

		/**
		 * Recupera el "access_token" del cliente a partir de un código determinado, aportado por DruID.
		 *
		 * @param string $endpoint_url Donde vamos a solicitar la información.
		 * @return mixed Un objeto {@link AccessToken} con los datos o FALSE si no ha podido recuperar ningún token.
		 *
		 * @throws Exception Si hay algún problema durante el proceso.
		 */
		public AccessToken doGetAccessToken(string endpoint_url, string code, string redirect_url) {
			try {
				// Verificación de parámetros.
				if ((endpoint_url = endpoint_url.Trim()).Equals("")) {
					throw new Exception ( "Endpoint URL is empty" );
				}
				if ((code = code.Trim()).Equals("")) {
					throw new Exception ( "Code is empty" );
				}
				if ((redirect_url = redirect_url.Trim()).Equals("")) {
					throw new Exception ( "Redirect URL is empty" );
				}
				if (this.getClientId().Equals("")) {
					throw new Exception ( "Client ID is empty" );
				}
				if (this.getClientSecret().Equals("")) {
					throw new Exception ( "Client secret is empty" );
				}
				
				// Lanzo la petición.
                Dictionary<string, string> dictParams = new Dictionary<string, string>( );
                dictParams.Add("grant_type", GRANT_TYPE_AUTH_CODE);
                dictParams.Add("client_id", this.getClientId());
                dictParams.Add("client_secret", this.getClientSecret());
                dictParams.Add("code", code);
                dictParams.Add("redirect_uri", redirect_url);

                JObject response;
                response=this.executeRequest(endpoint_url,dictParams, HTTP_METHOD_POST);
				/*$params = array ();
				$params ['grant_type'] = self::GRANT_TYPE_AUTH_CODE;
				$params ['client_id'] = $this->client_id;
				$params ['client_secret'] = $this->client_secret;
				$params ['code'] = $code;
				$params ['redirect_uri'] = $redirect_url;
				$response = $this->executeRequest ( $endpoint_url, $params, self::HTTP_METHOD_POST );
				*/
				this.checkErrors ( response );
				
				// Almaceno los datos recibidos.
                if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["access_token"]).ToString())){
                    throw new Exception ( "The access_token retrieved is empty" );
                }

                if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["refresh_token"]).ToString())){
                    throw new Exception ( "The refresh_token retrieved is empty" );
                }
			
				int expires_in = DEFAULT_EXPIRES_IN; // 900 segundos = 15 minutos
                if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expiresIn"]).ToString()))
                {
                    expires_in = int.Parse(response["expiresIn"].ToString());
                }
                else if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expires_in"]).ToString()))
                {
                    expires_in = int.Parse(response["expires_in"].ToString());
                }
				// Resto un porcentaje del total para asegurarnos de que nuestra fecha de caducidad calculada es menor que la devuelta por el servidor.
				// Ésto permite evitar errores de "invalid tokens".
				expires_in = (int)((double)expires_in - ((double)expires_in * SAFETY_RANGE_EXPIRES_IN));
				DateTime expires_at = DateTime.Now.AddSeconds(expires_in);
				
				// Guardo los tokens en soportes persistentes.
                AccessToken access_token = new AccessToken(response["access_token"].ToString().Trim(), expires_in, expires_at, "/");
                RefreshToken refresh_token = new RefreshToken(response["refresh_token"].ToString().Trim(), 0, default(DateTime), "/");
				this.storeToken ( access_token );
				this.storeToken ( refresh_token );
				
				// Si tengo el ID del usuario, aprovecho y me lo quedo.
                if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["login_status"]["uid"]).ToString()))
                {
                    this.user_id = response["login_status"]["uid"].ToString();
                }
                
				//if (isset ( $response ['result'] ['login_status'] ['uid'] ) && (trim ( $response ['result'] ['login_status'] ['uid'] ) != '')) {
				//	$this->user_id = (is_integer ( $response ['result'] ['login_status'] ['uid'] ) ? $response ['result'] ['login_status'] ['uid'] : intval ( $response ['result'] ['login_status'] ['uid'] ));
				//}
				return access_token;
			} catch ( Exception e ) {
				
				throw e;
			}
		}

		/**
		 * Actualiza los tokens almacenados.
		 *
		 * @param string $endpoint_url Donde vamos a solicitar la información.
		 * @return boolean TRUE si ha podido almacenar los tokens almaceandos o FALSE en caso contrario.
		 */
		public bool doRefreshToken(string endpoint_url) {
			try {
				if ((endpoint_url = endpoint_url.Trim()).Equals("")) {
					throw new Exception ( "Endpoint URL is empty" );
				}
				if (this.getClientId().Equals("")) {
					throw new Exception ( "Client ID is empty" );
				}
				if (this.getClientSecret().Equals("")) {
					throw new Exception ( "Client secret is empty" );
				}
				if (this.getStoredToken(StoredToken.REFRESH_TOKEN)==null){//  ((refresh_token = $this->getStoredToken ( iTokenTypes::REFRESH_TOKEN )) instanceof RefreshToken)) {
					throw new Exception ( "Refresh token is empty" );
				}
				
                Dictionary<string, string> dictParams = new Dictionary<string, string>( );
                dictParams.Add("grant_type", GRANT_TYPE_REFRESH_TOKEN);
                dictParams.Add("client_id", this.getClientId());
                dictParams.Add("client_secret", this.getClientSecret());
                dictParams.Add("refresh_token", this.getStoredToken(StoredToken.REFRESH_TOKEN).getValue());

                JObject response;
                response=this.executeRequest(endpoint_url,dictParams, HTTP_METHOD_POST);
				/*# Lanzo la petición de actualización.
				*/
                if (response.Children().Count() == 0)
                {
                    return false;
                }
                this.checkErrors ( response );

				// Actualizo los datos.
                if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["access_token"]).ToString())){
                    throw new Exception ( "The access_token retrieved is empty" );
                }

                if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["refresh_token"]).ToString())){
                    throw new Exception ( "The refresh_token retrieved is empty" );
                }

				// Me fio más del tiempo de segundos que faltan para que caduque el token que de la fecha que devuelve DruID. Ésta no está
				// representada en GMT
				int expires_in = DEFAULT_EXPIRES_IN; // 900 segundos = 15 minutos
					if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expiresIn"]).ToString())){
					    expires_in = int.Parse(response ["expiresIn"].ToString());
				    } else if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["expiresIn"]).ToString())){
                        expires_in = int.Parse(response["expires_in"].ToString());
				    }
				// Resto un porcentaje del total para asegurarnos de que nuestra fecha de caducidad calculada es menor que la devuelta por el servidor.
				// Ésto permite evitar errores de "invalid tokens".
				expires_in = (int)((double)expires_in - ((double)expires_in * SAFETY_RANGE_EXPIRES_IN));
				DateTime expires_at = DateTime.Now.AddSeconds(expires_in);
				
				bool status = true;
				status = (this.storeToken ( new AccessToken ( response ["access_token"].ToString(), expires_in, expires_at, "/" ) ));
				status = (this.storeToken ( new RefreshToken ( response["refresh_token"].ToString(), 0, default(DateTime), "/" ) ) );
				return status;
			} catch ( Exception e ) {
				throw e;
			}
		}

		/**
		 * Almacena un token para uso persistente.
		 *
		 * @param StoredToken $token Objeto con los datos que hay que almacenar.
		 * @return boolean TRUE si ha podido guardar el valor, FALSE en caso contrario.
		 */
		public bool storeToken(StoredToken token)
		{
			//Guardo en sesión.
            if (!token.getValue().Trim().Equals(""))
            {
                HttpContext.Current.Session[token.getName()] = token.getValue(); //JsonConvert.SerializeObject(token);
            }
            Encryption crypt = new Encryption(this.getClientId());
            string encryptedString = "";
            if (!string.IsNullOrEmpty(token.getValue()))
            {
                encryptedString = crypt.Encrypt(token.getValue());
            }

            HttpCookie tmpCookie = new HttpCookie(token.getName(), encryptedString); // token.getValue());
            tmpCookie.Expires=token.getExpiresAt();
            tmpCookie.Path=token.getPath();
            tmpCookie.HttpOnly=true;
             
			HttpContext.Current.Request.Cookies.Set(tmpCookie);
            return true;
		}



		/**
		 * Verifica los errores en la respuesta recibida.
		 *
		 * @param array $response Vector donde buscar el mensaje de error.
		 * @return void
		 * @throws Exception El error recuperado.
		 */
		public void checkErrors( JObject response)
		{
			string error ="";// (isset(response["error"]) ? trim($response['result']['error']) : '');
            string type="";
            if (response["error"]!=null)
                error= response["error"].ToString();
            if (response["type"]!=null)
                type= response["type"].ToString();

			if (!error.Equals("")) {
				throw new Exception(error+" (" + type + ")");
			}
		}

			public bool doCheckUserCompleted(string endpoint_url, string scope="") {			
				try {
					if (string.IsNullOrEmpty(endpoint_url)) {
						throw new Exception ( "Endpoint URL is empty" );
					}
			
					if (string.IsNullOrEmpty(scope)) {
						throw new Exception ( "Scope is empty" );
					}
                    
                    AccessToken  access_token = (AccessToken) this.getStoredToken( StoredToken.ACCESS_TOKEN );
					if(access_token.getValue().Equals("")){
						throw new Exception ( "Access token is empty" );
					}
                    
                    Dictionary<string, string> dictParams = new Dictionary<string, string>( );
                    dictParams.Add("oauth_token", access_token.getValue());
                    dictParams.Add("s", "needsToCompleteData");
                    dictParams.Add("f", "UserMeta");
                    dictParams.Add("w.section", scope);

                    JObject response;
                    response=this.executeRequest(endpoint_url,dictParams, HTTP_METHOD_POST);
			
					// Lanzamos la petición.
					/*$params = array();
					$params['oauth_token'] = $access_token->getValue();
					$params['s'] = "needsToCompleteData";
					$params['f'] = "UserMeta";
					$params['w.section'] = $scope;
			        
					$response = $this->executeRequest($endpoint_url, $params, self::HTTP_METHOD_POST);
			        */
					this.checkErrors(response);
					
					string checkCompleted = "";
                    
                    //if (!string.IsNullOrEmpty(AuxStrings.ToSafeString(response["code"]).ToString()) && response["code"].ToString().Equals("200")){
						checkCompleted = response["data"][0]["meta"]["value"].ToString();
					//}
					
					//El método devuelve si necesita completar datos
					if (checkCompleted.Equals("false")){
						return true;
					} else {
						return false;
					}
		
				} catch (Exception e) {
					throw e;
				}
			}
    }


}
