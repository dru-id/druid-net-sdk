using System;
using System.Collections.Generic;
using System.Linq;


namespace DruIDSdk
{
    public class UrlBuilder
    {

        public UrlBuilder(Identity identity)
        {
            this.identity = identity;
        }
        private Identity identity;

        /**
         * returns a link to the login URL
         * param string "scope" is the section of your web client. If section is null, the default section
         * will be used. If your web client has only one section (only one "section" node in your oauthconf.xml)
         * this parameter is not needed 
         * return string with login URL
         */        
        public string getUrlLogin(string scope = null){
			string urlLogin =null;

            urlLogin = this.identity.oauth_client.buildLoginUrl(this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "authorization_endpoint").url, this.identity.oauth_config.appEnvironment.redirections.FirstOrDefault(o => o.type == "postLogin" && o.isdefault==true).url, scope);
			return urlLogin;
		}

        /**
         * returns a link to the register URL
         * param string "scope" is the section of your web client. If section is null, the default section
         * will be used. If your web client has only one section (only one "section" node in your oauthconf.xml)
         * this parameter is not needed.
         * return string with register URL
         */
        public string getUrlRegister(string scope = null){
            string urlRegister = this.identity.oauth_client.buildSignupUrl(this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "signup_endpoint").url, this.identity.oauth_config.appEnvironment.redirections.FirstOrDefault(o => o.type == "register" && o.isdefault == true).url, scope);					
			return urlRegister;
		}

        /**
         * returns a link to the edit account URL
         * param string "scope" is the section of your web client. If section is null, the default section
         * will be used. If your web client has only one section (only one "section" node in your oauthconf.xml)
         * this parameter is not needed.
         * return string with edit account URL
         */
		public string getUrlEditAccount(){
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams["client_id"] = this.identity.oauth_client.getClientId();
            dicParams["redirect_uri"] = this.identity.oauth_config.appEnvironment.redirections.FirstOrDefault(o => o.type == "postEditAccount" && o.isdefault == true).url;

            string next_url = this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "next_url").url + "?" + this.identity.oauth_client.http_build_query(dicParams, "&");
            string cancel_url = this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "cancel_url").url + "?" + this.identity.oauth_client.http_build_query(dicParams, "&");

            string urlEdit = this.identity.oauth_client.buildEditAccountUrl(this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "edit_account_endpoint").url, System.Web.HttpUtility.UrlEncode(next_url), System.Web.HttpUtility.UrlEncode(cancel_url));			
			return urlEdit;
		}

        /**
         * 
         * @param unknown_type $scope
         * @return unknown
         */
		public string getUrlCompleteAccount(string scope = null){
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams["client_id"] = this.identity.oauth_client.getClientId();
            dicParams["redirect_uri"] = this.identity.oauth_config.appEnvironment.redirections.FirstOrDefault(o => o.type == "postEditAccount" && o.isdefault == true).url;


            string next_url = this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "next_url").url + "?" + this.identity.oauth_client.http_build_query(dicParams, "&");
            string cancel_url = this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "cancel_url").url + "?" + this.identity.oauth_client.http_build_query(dicParams, "&");

            string urlEdit = this.identity.oauth_client.buildCompleteAccountUrl(this.identity.oauth_config.appEnvironment.endPoints.FirstOrDefault(o => o.id == "complete_account_endpoint").url, System.Web.HttpUtility.UrlEncode(next_url), System.Web.HttpUtility.UrlEncode(cancel_url), scope);
			return urlEdit;
        }


        /**
         * The method checks if the user is logged.
         * 	- if it is not logged, will return the login URL
         * 	- if it is logged the method will check 
         * 		- if the user have not enough PII to access to a section, returns the URL needed 
         * 			to force a consumer to fill all the PII needed to enter into a section
         * 		- else will return 
         * 
         * 
         * the $scope (section) is a group of fields configured in DruID for a web client
         * 
         * A section can be also defined as a "part" (section) of the website (web client) that only can be 
         * accesed by a user who have filled a set of personal information configured in Coke 
         * ID (all of the fields required for that section).
         * When a web client has different sections, the oauthconf.xml will provide the list of sections
         * available that a web client can use to register the consumers.
         * 
         * return string with generated URL. If the user is not connected, will return login URL
         *
         * throws Exception if scope is empty.
         */
		public string buildSignupPromotionUrl(string scope) {
			try {
		        string urlSignupPromotion="";
                bool userCompletePromo = false;
				if (string.IsNullOrEmpty(scope))
                {
					throw new Exception ( "Scope section is empty" );
				}
		        
                
				if (!this.identity.isConnected()) {
					//El usuario no está logado
					urlSignupPromotion = this.getUrlLogin(scope);
				} else {
					//Comprobamos si el usuario tiene completa las sección de la promo
					userCompletePromo = this.identity.checkUserComplete(scope);
					if (!userCompletePromo){
						urlSignupPromotion = this.getUrlCompleteAccount(scope);
					}
				}
				return urlSignupPromotion;
		
			} catch ( Exception e ) {
				throw e;
			}
		}
    }
}
