using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace DruIDSdk
{    
    public class User
    {
		public class Valued
		{
			public String value { get; set; }
		}

		public class Field : Valued
		{
			public String label { get; set; }
		}

		public class IdField: Field
		{
			[JsonProperty("is_social")]
			public Boolean social { get; set; }

			[JsonConverter(typeof(JsonGenericDictionaryOrArrayConverter))]
			public Dictionary<String, Field> session;
		}

		[JsonProperty("oid")]
		public String id { get; set; }
		public Boolean confirmed { get; set; }
		public String app { get; set; }
		[JsonProperty("entry-point")]
		public String entrypoint { get; set; }
		[JsonProperty("created_on")]
		public String createdOn { get; set; }

		[JsonProperty("user_ids")]
		public Dictionary<String, IdField> ids;
		[JsonProperty("user_data")]
		public Dictionary<String, Field> datas;

		[JsonProperty("user_assertions")]
        [JsonConverter(typeof(JsonGenericDictionaryOrArrayConverter))]
        public Dictionary<String, Dictionary<String, Valued>> assertions;

        [JsonConverter(typeof(JsonGenericDictionaryOrArrayConverter))]
        public Dictionary<String, Valued> typologies;

		public JObject raw { get; set; }

		/**
		 * @return the id
		 */
		public String getId() {
			return this.id;
		}
	
		
	
		/**
		 * @return the email
		 */
		public String getEmail() {
            return this.ids["email"].value;
		}
	
		/**
		 * @return the birthday
		 */
		public String getBirthday() {
			return this.datas["birthday"].value;
		}

		/**
		 * @return the $cw
		 */
		public String getApp() {
			return this.app;
		}
	


		/**
		 * @return the $section
		 */
		public String getEntrypoint() {
			return this.entrypoint;
		}
	
				
		/**
		 * @return the $confirmed
		 */
		public Boolean getConfirmed() {
			return this.confirmed;
		}

		
    }
}
