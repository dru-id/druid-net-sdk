using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using System.Reflection;
using System.IO;

namespace DruIDSdk
{
    public class Identity
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public DruIDThings druIDThings;
		public OAuth oauth_client;
		public OAuthConfigParser oauth_config;
        public String strPathDll = "";
        public FileCache cache; 

        
        /**
         * When you instanciate the library, the constructor loads the configuration that is defined in oauthconf.xml 
         * file of the environment
         * Params:
         *  xmlConfigPath (string): full path to the config file (for example: C:\\myfilepath\\DLL\\oauthconf.xml)
         *  environment (string): environment to load settings
         */

        public Identity(string configPath)
        {

            try
            {
                string environment = getCurrentEnvironment(configPath);

                this.oauth_config = new OAuthConfigParser();
                this.oauth_config.getPathFolders(configPath);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(this.oauth_config.pathLog));
                this.cache = new FileCache(this.oauth_config.pathCache, "development");

                this.oauth_config.newloadFromFile(environment, configPath);

                oauth_client = new OAuth(this.oauth_config.appEnvironment.credentials.clientid, this.oauth_config.appEnvironment.credentials.clientsecret);
                this.druIDThings = new DruIDThings();

            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                throw e;
            }
        }

        /**
         * This method returns current environment checking the current url against the info stored  at the libcokeid.ini file
         */
        private string getCurrentEnvironment(string configPath)
        {
            string environment = "";
            try
            {
                string initPath = configPath + $"{Path.DirectorySeparatorChar}druid.ini";
                IniParser parser = new IniParser(initPath);

                string currentHost = HttpContext.Current.Request.Url.Host;
                string devServer = parser.GetSetting("ENVIRONEMTS SERVERS", "DEV_SERVER").Replace("\"", "").Replace(";", "");
                string testServer = parser.GetSetting("ENVIRONEMTS SERVERS", "TEST_SERVER").Replace("\"", "").Replace(";", "");
                string prodServer = parser.GetSetting("ENVIRONEMTS SERVERS", "PROD_SERVER").Replace("\"", "").Replace(";", "");
                if (currentHost.ToUpper().Equals(devServer) || currentHost.ToLower().Equals("localhost") || currentHost.ToUpper().Equals("127.0.0.1"))
                {
                    environment = "dev";
                }
                else if (currentHost.ToUpper().Equals(testServer))
                {
                    environment = "test";
                }
                else if (currentHost.ToUpper().Equals(prodServer))
                {
                    environment = "prod";
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return environment;
        }

        /**
         * This method returns the PII of the logged user
         *
         * @result User instance of logged user PII
         */
		public User getUserLogged(){
			User userLogged = new User();
            string  uid="";
			try {

                LoginStatus loginStatus = this.oauth_client.doLoginStatus(this.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "token_endpoint").url);
                Log.Debug("Get user Logged info");
                
				if (loginStatus.getConnect_state().Equals("connected")){
					uid = loginStatus.getUid();

                    userLogged = this.getDataUsers(uid, this.getAccessToken());
					
				}
							
			} catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                throw e;
            }
			return userLogged;
		}

		/**
		 * Recupera el "client_token" Almacenado.
		 * 
		 * Se comprueba si existe o si está caducado, almacenando un client_token válido en storedToken
		 * 
		 * @return ClientToken Devuelve un client_token válido
		 * 		 
		 */
		private ClientToken checkAndUpdateClientToken(){			
			try {
                Log.Debug("");
                ClientToken client_token = null;
				
				// Si no tengo client_token o está espirado, obtengo un cliet token y lo almaceno
				if ((this.oauth_client.hasToken(StoredToken.CLIENT_TOKEN)==null)||this.oauth_client.tokenExpired(StoredToken.CLIENT_TOKEN)) {
                    client_token = this.oauth_client.doGetClientToken(this.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "token_endpoint").url);
                    Log.Info("No exist Token, Request a new token");
				} else {
					client_token = (ClientToken)this.oauth_client.getStoredToken(StoredToken.CLIENT_TOKEN);
                    Log.Info("Exist Token, use it");
				}					
				this.druIDThings.setClientToken(client_token);
				
				return client_token;
				
			} catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);			
				throw e;
			}	
			
		}

        /**
         * This method verifies the authorization tokens (client_token, access_token and refresh_token)
         * Also updates the web client status, storing the client_token, access_token and refresh tokend and 
         * login_status in the library.
         * You should invoke it on each request in order to check and update the status of the user 
         * (not logged, logged or connected), and verify that every token that you are gonna use before is going to be 
         * valid.
         */
        public void synchronizeSessionWithServer()
        {
            try
            {
                Log.Info("Init SynchronizeSession");
                
                this.checkAndUpdateClientToken();
                this.loadUserTokenFromEventPersistence();
                
                
                // Comprobar estado de usuario con SSO
                this.checkLoginConnect();


                if (this.druIDThings.getUserToken()!=null){
										
					this.checkAndRefreshAccessToken();
					
					if (this.druIDThings.getUserToken()!=null){
						this.checkLoginStatus();
						
						if (!this.isConnected()){
							this.clearLocalSessionData();	
						}
					}
				}
            }
            catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }
        }

        /**
         * Obtiene el usuario logado a partir del access_token almacenado
         * 
         * Actualiza en la instancia de druIDThings, el access_token
         * 
         */
		private void loadUserTokenFromEventPersistence(){
			try {								
				if (this.druIDThings.getUserToken()==null){
                    Log.Info("");
					this.druIDThings.setUserToken(this.getAccessToken());				
				}

			} catch (Exception e) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                throw e;
            }	
		}

		/**
		 * Se obtiene el AccessToken del usuario
		 * 
		 * Obtiene el access_token de la cookie local
		 * 
		 * @return AccessToken Devuelve el access_token obtenido
		 */
		private AccessToken getAccessToken(){
            AccessToken access_token = null;
            try {								
				if (this.oauth_client.hasToken(StoredToken.ACCESS_TOKEN)!=null) {															
					access_token = (AccessToken)this.oauth_client.getStoredToken(StoredToken.ACCESS_TOKEN);	
				} 
			} catch (Exception e) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                throw e;
            }

            return access_token;
		}

        /**
         * If a user succesfully logs-in DruID, the application, will redirect to post-login url of the 
         * web client.
         * 
         * In that case, the post-login URL will revieve an authorization code as a GET parameter.
         * Once the authorization code is provided to the web client, the SDK will send again to DruID the 
         * token_endpoint to obtain the access_token of the user and create the cookie
         * 
         * The metod is needed to authorize de user when the web client takes back the control of the browser.
         */
		public void authorizeUser(string code){
			try {
                Log.Info("Authorize to users");

				if (string.IsNullOrEmpty(code)) { throw new Exception("Authorize Code is empty"); }
                this.oauth_client.doGetAccessToken(this.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "token_endpoint").url, code, this.oauth_config.appEnvironment.redirections.FirstOrDefault(o => o.type == "postLogin").url);
				
				this.checkAndRefreshAccessToken();

			} catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                throw e;
            }
			
		}

        /**
         * This method is needed to logout a user. 
         * It makes 
         * - the logout call to DruID 
         * - clear cookies
         * - tokens and local data for the logged user
         */
		public void logoutUser(){
			try {
				if (!(this.druIDThings.getUserToken()==null)&&(!(this.druIDThings.getRefreshToken()==null))){
                    Log.Info("User Single Sign Logout");
                    this.oauth_client.doLogout(this.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "logout_endpoint").url);
					
					this.clearLocalSessionData();				
				}
				
			} catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
				throw e;
			}
		}

		/**
		 * Elimina datos de session
		 * 
		 */
		private void clearLocalSessionData(){
			try {
				this.druIDThings.setUserToken(null);			
				this.druIDThings.setRefreshToken(null);
				this.druIDThings.setSsoCookie(null);
			} catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
	            throw e;
			}	
		}

		/**
		 * Comprobar el estado de un usuario, si está logado o no
		 * 		 		 
		 * @return boolean si el usuario está logado, o no 
		 * 		 
		 */
		public bool checkLoginStatus(){
			LoginStatus loginStatus;
			try {
                Log.Debug("");
                loginStatus = this.oauth_client.doLoginStatus(this.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "token_endpoint").url);
                if(loginStatus!=null){
                    this.druIDThings.setLoginStatus(loginStatus);
                    if(this.druIDThings.getLoginStatus().getConnect_state().Equals("connected")){
                        return true;   
                    }
                }
                else{
                    return false;
                }
                return false;
			} catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                return false;
			}
			
		}

        /**
         * Recupera el "access_token" Almacenado.
         * 
         * Se comprueba si existe o si está caducado, almacenando un access_token válido en storedToken		
         * 
         * @return void Actualiza en la instancia de druIDThings, el access_token actualizado
         * 		 
         */
		private bool checkAndRefreshAccessToken(){
			try {
				
				if ((!(this.druIDThings.getUserToken()==null))&&(this.isExpired(this.druIDThings.getUserToken().getExpiresAt()))){
                    Log.Info("");				
					if (!this.oauth_client.doRefreshToken(this.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "token_endpoint").url))
                    {
                        Log.Error("Access Token can not be refreshed");
                        throw new Exception("access token can not be refreshed");
					}														
				} else {
					this.druIDThings.setUserToken(this.getAccessToken());
				}
				
				if ((this.druIDThings.getRefreshToken()==null)){
					this.druIDThings.setRefreshToken((RefreshToken) this.oauth_client.getStoredToken(StoredToken.REFRESH_TOKEN));
				}
                return true;
			} catch (Exception e) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                return false;
            }					
		}

		private Boolean isExpired(DateTime expiresAt){
			if (!(expiresAt==null)){
				return (this.oauth_client.time() > this.oauth_client.time(expiresAt));
			}
			return true;
		}

        /**
         * Check if the user is connected. A user conected is a user logged on DruID.
         * 
         * @return boolean True is logged, False is not logged
         */
		public bool isConnected(){
			if ((!(this.druIDThings==null))&&(!(this.druIDThings.getUserToken()==null)&&(!(this.druIDThings.getLoginStatus()==null))&&(this.druIDThings.getLoginStatus().getConnect_state().Equals("connected")))){
				return true;	
			}
			return false;
		}

        /**
         * Check for SSO
         * 
         * @return void
		 */
        private void checkLoginConnect(){
            try {				
                if ((this.druIDThings.getClientToken() != null)&&(string.IsNullOrEmpty(this.druIDThings.getSsoCookie()))&&!this.isConnected()){
                    String datrvalue="";
                    if (HttpContext.Current.Request.Cookies["datr"]!=null && (!HttpContext.Current.Request.Cookies["datr"].Value.Equals(""))) {		
                        //Existe la cookie datr
                        datrvalue = HttpContext.Current.Request.Cookies["datr"].Value;
                    }
                    else
                    {   
                        //no existe datr, buscamos la que comience por datr_
                        foreach (string cookiekey in HttpContext.Current.Request.Cookies)
                        {
                            if (cookiekey.LastIndexOf("datr_") == 0)
                            {
                                datrvalue = HttpContext.Current.Request.Cookies[cookiekey].Value;
                                break;
                            }
                        }
                    }

                    if (datrvalue.Length > 0)
                    {
                        this.druIDThings.setSsoCookie(datrvalue);

                        this.druIDThings.setUserToken(this.oauth_client.doCheckLoginConnect(this.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "token_endpoint").url, this.druIDThings.getSsoCookie()));

                        this.checkAndRefreshAccessToken();
                    }

                           
                   
                }
				
            } catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                throw e;
            }
        }
        

        /**
         * Returns the User PII trough the DruID personal identifier.
         *
         * @param string uid DruID personal identifier
         * @param String access_token
         *
         * @return User instance
         */
        public User getDataUsers(string uid, StoredToken access_token)
        {
            Dictionary<string, string> user = new Dictionary<string, string>( );

            JObject response = this.cache.get(uid);

            if (response != null)
            {
                if (Log.IsDebugEnabled) { 
                    Log.Debug("objetId: " + uid + " is in Cache System");
                }
            }
            else
            {
                /**
                    *
                    Parameters:
                    oauth_token: client token
                    s (select): dynamic user data to be returned
                    f (from): User
                    w (where): param with OR w.param1&w.param2...
                    */
                //JFT: PENDIENTE DE CACHÉ PARA LOG
                Dictionary<string, string> dictParams = new Dictionary<string, string>();
                dictParams.Add("oauth_token", access_token.getValue());
                dictParams.Add("s", "*");
                dictParams.Add("f", "User");
                dictParams.Add("w.id", uid);

                response = this.oauth_client.executeRequest(this.oauth_config.appEnvironment.apis.FirstOrDefault(o => o.id == "user").url, dictParams, "POST");

                if (string.IsNullOrEmpty(AuxStrings.ToSafeString(response["data"]).ToString()) || AuxStrings.ToSafeString(response["count"]).ToString().Equals("0"))
                {
                    throw new Exception("The data retrieved is empty");
                }

                this.cache.set(uid, response.ToString(), 60);
            }

            return this.parseQueryUser(response);
        }

        private User parseQueryUser(JObject usersJson)
        {              

            JObject userJson = usersJson["data"].FirstOrDefault()["user"] as JObject;

            User user = userJson.ToObject<User>();

            /*
            user.setId(AuxStrings.ToSafeString(userJson["oid"]).ToString());
            //user.setNick(AuxStrings.ToSafeString(userJson["nick"]).ToString());
            if(userJson["user_id"]["screenName"]!=null)
                user.setNick(AuxStrings.ToSafeString(userJson["user_id"]["screenName"]["value"]).ToString());
            //user.setBirthday(AuxStrings.ToSafeString(userJson["birthday"]).ToString());
            user.setBirthday(AuxStrings.ToSafeString(userJson["user_data"]["birthday"]["value"]).ToString());
            //user.setEmail(AuxStrings.ToSafeString(userJson["email"]).ToString());
            user.setEmail(AuxStrings.ToSafeString(userJson["user_id"]["email"]["value"]).ToString());
            user.setArcc(AuxStrings.ToSafeString(userJson["arcc"]).ToString());
            user.setId(AuxStrings.ToSafeString(userJson["id"]).ToString());
            user.setCw(AuxStrings.ToSafeString(userJson["cw"]).ToString());
            user.setSection(AuxStrings.ToSafeString(userJson["section"]).ToString());
            if(userJson["user_id"]["id"]!=null)
                user.setUserId(AuxStrings.ToSafeString(userJson["user_id"]["id"]["value"].ToString()));
            user.setActive(AuxStrings.ToSafeString(userJson["active"]).ToString());
            user.setJsonUser(AuxStrings.ToSafeString(usersJson.ToString()));
            */
            user.raw = userJson;
            return user;
		}

        /**
         * This method checks if the user have completed all the fields needed for that section.
         *
         * the "scope" (section) is a group of fields configured in DruID for a web client.
         * A section can be also defined as a "part" (section) of the website (web client) that only can be accesed by a
         *  user who have filled a set of personal information configured in DruID (all of the fields required for 
         *  that section).
         *  
         * This method is commonly used for promotions or sweepstakes: if a user wants to participate in a promotion, 
         * the web client must ensure that the user have all the fields filled in order to let him participate.
         * 
         * @param string scope section-key identifier of the web client. The section-key is located in oauthconf.xml
         * file
         * @return boolean TRUE if the user have already completed all the fields needed for that section
         */
		public bool checkUserComplete(string scope){
			try {
                Log.Info("Check if user is complete in scope: " + scope);
			    bool userCompleted=false;

                if (this.isConnected()){
                    userCompleted = this.oauth_client.doCheckUserCompleted(this.oauth_config.appEnvironment.apis.FirstOrDefault(o => o.id == "user").url, scope);
				}

				return userCompleted;
			} catch ( Exception e ) {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
				throw e;
			}
		}
    }
}
