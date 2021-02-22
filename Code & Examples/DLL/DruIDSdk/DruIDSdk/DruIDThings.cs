using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DruIDSdk
{
    public class DruIDThings
    {
		// Almacenta el client_token - StoredToken instance of ClientToken
		public  ClientToken clientToken = null;
		// Almacenta el access_token - StoredToken instance of AccessToken
        private  AccessToken userToken = null;
		// web session token of logged user
        private  String ssoCookie = "";
		// LoginStatus
        private LoginStatus loginStatus = null;
		// Refresh token
        private  RefreshToken refreshToken = null;

        /**
		 * @return the loginStatus
		 */
        public LoginStatus getLoginStatus()
        {
			return this.loginStatus;
		}

        			/**
		 * @param field_type loginStatus
		 */
        public void setLoginStatus(LoginStatus paramloginStatus)
        {
            this.loginStatus = paramloginStatus;
		}

		/**
		 * @return the clientToken
		 */
         public ClientToken getClientToken()
        {
            return this.clientToken;
		}
	
		/**
		 * @return the userToken
		 */
         public AccessToken getUserToken()
        {
            return this.userToken;
		}
	
		/**
		 * @return the ssoCookie
		 */
        public String getSsoCookie()
        {
            return this.ssoCookie;
		}
	
		/**
		 * @param field_type clientToken
		 */
        public void setClientToken(ClientToken paramclientToken)
        {
            this.clientToken = paramclientToken;
		}
	
		/**
		 * @param field_type userToken
		 */
        public void setUserToken(AccessToken paramuserToken)
        {
            this.userToken = paramuserToken;
		}
	
		/**
		 * @param field_type ssoCookie
		 */
        public void setSsoCookie(String paramssoCookie)
        {
            this.ssoCookie = paramssoCookie;
		}
		
		/**
		 * @return the refreshToken
		 */
        public RefreshToken getRefreshToken()
        {
            return this.refreshToken;
		}
	
		/**
		 * @param field_type refreshToken
		 */
        public void setRefreshToken(RefreshToken paramrefreshToken)
        {
            this.refreshToken = paramrefreshToken;
		}	
    }
}
