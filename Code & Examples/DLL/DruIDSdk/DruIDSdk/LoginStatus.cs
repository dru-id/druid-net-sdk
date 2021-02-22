using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DruIDSdk
{
    public class LoginStatus
    {
         public String uid = null;
		 public String connect_state = null;
		
		/**
		 * @return the $uid
		 */
		public String getUid() {
			return uid;
		}
	
		/**
		 * @return the $connect_state
		 */
		public String getConnect_state() {
			return this.connect_state;
		}
	
		/**
		 * @param field_type $uid
		 */
		public void setUid(String paramuid) {
			this.uid = paramuid;
		}
	
		/**
		 * @param field_type $connect_state
		 */
		public void setConnect_state(String paramconnect_state) {
			this.connect_state = paramconnect_state;
		}
    }
}
