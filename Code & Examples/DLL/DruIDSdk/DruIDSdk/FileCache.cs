using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Permissions;
using System.Security;
using System.Security.AccessControl;
using Newtonsoft.Json.Linq;
using log4net;
using System.Reflection;


namespace DruIDSdk
{

    public class FileCache
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /**
		 * Value is pre-pended to the cache, should be the full path to the directory
		 */
        private String cache_dir = "";

        /**
		 * For holding any error messages that may have been raised
		 */
		private String error = null;

		/**
		 * Prefix to the name of file
		 * var String
		 */
		protected String prefix = "";

        /**
         * param string root The root of the file cache.
         */
        public FileCache(String path, String pref = "")
        {
            this.cache_dir = path;
            this.prefix = pref;
            
            if (!Directory.Exists(this.cache_dir))
            {
                throw new Exception($"Cache directory {this.cache_dir} does not exist.");
            }
            else
            {

                if (!CheckFolderPermissions(this.cache_dir,FileSystemRights.Write))
                {
                    throw new Exception($"Cache directory {this.cache_dir} is not writable.");
                }
            }

        }

        /**
         * Saves data to the cache. Anything that evaluates to false, null, '', boolean false, 0 will
         * not be saved.
         * @param string $key An identifier for the data
         * @param mixed $data The data to save
         * @param int $ttl Seconds to store the data
         * @returns boolean True if the save was successful, false if it failed
         */

        public Boolean set(String key, String data, int ttl = 3600)
        {
            bool status = false;
            if (key.Length == 0)
            {
                this.error = "Invalid key";
                return status;
            }

            if (data.Length == 0)
            {
                this.error = "Invalid data";
                return status;
            }

            key = make_file_key(key);
            //String[,] store = new String[2, 2] { {"data", data},{"ttl",ttl.ToString()}};
            
            try
            {
                JObject json = new JObject();
                if (!string.IsNullOrEmpty(data))
                {
                    json = JObject.Parse(data);
                    json.Add("ttl", (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + ttl);
                
                }
                
                FileStream fh = new FileStream(key, FileMode.OpenOrCreate,FileAccess.Write,FileShare.None);
                fh.SetLength(0);
                StreamWriter sw = new StreamWriter(fh);
                sw.Write(json.ToString());
                sw.Close();
                fh.Close();
                status = true;
            }
            catch(Exception e)
            {
                this.error = "Exception caught: " + e.Message;
                return false;
            }

            return status;
        }

        /**
         * Reads the data from the cache
         * @param string $key An identifier for the data
         * @returns mixed Data that was stored
         */
        public JObject get(String key)
        {
            if (key.Length == 0)
            {
                this.error = "Invalid key";
                return null;
            }

            key = this.make_file_key(key);

            if(!File.Exists(key))
            {
                return null;
            }

            String file_content=null;
            try
            {
                FileStream fh = new FileStream(key, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fh);
                file_content = sr.ReadToEnd();
                sr.Close();
                fh.Close();

                // Assuming we got something back...
                if (file_content != null)
                {
                    JObject json = new JObject();
                    if (!string.IsNullOrEmpty(file_content))
                    {
                        json = JObject.Parse(file_content);
                        int ttl=0;
                        if (int.TryParse(json["ttl"].ToString(), out ttl))
                        {
                            if (ttl < (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds)
                            {
                                File.Delete(key);
                                this.error = "Data expired";
                                return null;
                            }
                        }
                        return json;

                    }
                }
                return null;
                
            }
            catch(Exception e)
            {
                this.error = "Exception caught: " + e.Message;
                return null;
            }
        }
        /**
         * Create a key for the cache
         * @todo Beef up the cleansing of the file.
         * @param string $key The key to create
         * @returns string The full path and filename to access
         */
        private String make_file_key(String key)
        {
            String safe_key = GetMD5Hash(key);
            return this.cache_dir + Path.DirectorySeparatorChar + safe_key;
        }

        public static bool CheckFolderPermissions(string directoryPath, FileSystemRights accessType)
        {
            bool hasAccess = true;
            try
            {
                AuthorizationRuleCollection collection = Directory.
                                            GetAccessControl(directoryPath)
                                            .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                foreach (FileSystemAccessRule rule in collection)
                {
                    if ((rule.FileSystemRights & accessType) > 0)
                    {
                        return hasAccess;
                    }
                }
            }
            catch (PlatformNotSupportedException e)
            {
                Log.Info(e.Message + ". this method will return 'true'");
            }
            catch (Exception ex)
            {
                Log.Error(ex.StackTrace);
                hasAccess = false;
            }
            return hasAccess;
        }

        //Equivalent function to php md5
        public string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

    }
}
