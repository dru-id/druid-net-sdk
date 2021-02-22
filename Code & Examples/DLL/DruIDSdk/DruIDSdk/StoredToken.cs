using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;

namespace DruIDSdk
{
 /**
 * Conjunto de clases para almacenar los datos de los tokens almacenados.
 *
 * @author Ismael Salgado
 * @since 2011-09-08
 * @package jqc
 * @subpackage apis
 */

    public abstract class StoredToken
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const string CLIENT_TOKEN = "__ucs";
        public const string ACCESS_TOKEN = "__uas";
        public const string REFRESH_TOKEN = "__urs";

        public StoredToken(string value, int expires_in, DateTime expires_at = default(DateTime), string path = "/")
        {
            this.setValue(value);
            this.setExpiresIn(expires_in);
            this.setExpiresAt(expires_at);
            this.setPath(path);
            this.setName();
        }


        /** @var string $name El nombre del token. */
		protected string name = "";
		/** @var string $value El contenido del token. */
		protected string value = "";
		/** @var integer $expires_in Numero de segundos que tardara en expirar el token. */
		protected int expires_in = 0;
		/** @var integer $expires_at Unix timestamp para indicar cuando expira el token.*/
		protected DateTime expires_at = default(DateTime);
		/** @var string $path */
		protected string path = "/";


        /**
         * Asigna el tipo de token de que se trata. Cualquiera de los valores definidos en {@link iTokenTypes}
         *
         * esta funcion fuerza a las clases hija a asignar un valor a la variable "$this->type" para que a la hora de serializar
         * el contenido de esta clase sepamos de cual se trataba.
         *
         * @return void
         */
        abstract protected void setName();
        /**
		 * Devuelve el nombre del token.
		 *
		 * Permite determinar el tipo de token que estamos tratando para cuando es serializado el contenido.
		 *
		 * @return string El nombre del token. Cualquiera de los valores de {@link iTokenTypes}
		 */
		public string getName()
		{
			return name;
		}

		/**
		 * Asigna el valor del token.
		 *
		 * @param string $value El valor del token.
		 * @return void
		 */
		public void setValue(string value)
		{
			this.value = value.Trim();
		}
		/**
		 * Devuelve el valor
		 */
		public string getValue()
		{
			return this.value;
		}
		
        public void setExpiresIn(int expires_in)
		{
			this.expires_in = expires_in;
		}
		
        public int getExpiresIn()
		{
			return this.expires_in;
		}

		public void setExpiresAt(DateTime expires_at)
		{
			this.expires_at = expires_at;
		}
		
        public DateTime getExpiresAt()
		{
			return this.expires_at;
		}

		public void setPath(string path)
		{
			this.path = path.Trim();
		}
		
        public string getPath()
		{
			return this.path;
		}

		/**
		 * Genera el objeto apropiado, en funcion del nombre, con los valores indicados.
		 *
		 * @param string $value El valor del token.
		 * @param integer $expires_in El numero de segundos que faltan para que expire el token.
		 * @param integer $expires_at Unix timestamp para indicar cuando expira el token.
		 * @param string $path La ruta para la cookie, en caso de necesitarlo.
		 *
		 * @return mixed Un objeto {@link StoredToken} o FALSE si no ha podido crearlo.
		 */
		public static StoredToken factory(string name, string value, int expires_in, DateTime expires_at, string path)
		{
			value = value.Trim();
			path = path.Trim();
            
			switch (name.Trim()) {
				case ACCESS_TOKEN: return new AccessToken(value, expires_in, expires_at, path);
				case CLIENT_TOKEN: return new ClientToken(value, expires_in, expires_at, path);
				case REFRESH_TOKEN: return new RefreshToken(value, expires_in, expires_at, path);
			}
			return  null;
		}
	}
    
	/**
	 * Permite almacenar los datos para el "client_token".
	 *
	 * @author Ismael Salgado
	 * @since 2011-09-08
	 * @package jqc
	 * @subpackage apis
	 */
	public class ClientToken : StoredToken
	{
		/**
		 * @param mixed $value El valor del token.
		 * @param integer $expires_in El numero de segundos que faltan para que expire.
		 * @param integer $expires_at Unix timestamp para indicar cuando expira la cookie.
		 * @param string $path La ruta para la cookie, en caso de necesitarlo.
		 */
		public ClientToken(string value, int expires_in , DateTime expires_at, string path = "/") : base( value,  expires_in,  expires_at,  path = "/")
		{
			//parent::__construct($value, $expires_in, $expires_at, $path);
		}

		/**
		 * Asigna el tipo de token de que se trata. Cualquiera de los valores definidos en {@link iTokenTypes}
		 *
		 * esta funcion fuerza a las clases hija a asignar un valor a la variable "$this->type" para que a la hora de serializar
		 * el contenido de esta clase sepamos de cual se trataba.
		 *
		 * @return void
		 */
		override protected void setName()
		{
			this.name =  CLIENT_TOKEN;
		}
	}

    /**
     * Permite almacenar los datos para el "access_token".
     *
     * @author Ismael Salgado
     * @since 2011-09-08
     * @package jqc
     * @subpackage apis
     */
    public class AccessToken : StoredToken
    {
		/**
		 * @param mixed $value El valor del token.
		 * @param integer $expires_in El numero de segundos que faltan para que expire.
		 * @param integer $expires_at Unix timestamp para indicar cuando expira la cookie.
		 * @param string $path La ruta para la cookie, en caso de necesitarlo.
		 */
		public AccessToken(string value, int expires_in = 0, DateTime expires_at = default(DateTime), string path = "/") : base( value,  expires_in = 0,  expires_at = default(DateTime),  path = "/")
		{
			//parent::__construct($value, $expires_in, $expires_at, $path);
		}

		/**
		 * Asigna el tipo de token de que se trata. Cualquiera de los valores definidos en {@link iTokenTypes}
		 *
		 * esta funcion fuerza a las clases hija a asignar un valor a la variable "$this->type" para que a la hora de serializar
		 * el contenido de esta clase sepamos de cual se trataba.
		 *
		 * @return void
		 */
		override protected void setName()
		{
			this.name = ACCESS_TOKEN;
		}
    }

    /**
     * Permite almacenar los datos para el "refresh_token".
     *
     * @author Ismael Salgado
     * @since 2011-09-08
     * @package jqc
     * @subpackage apis
     */
    public class RefreshToken : StoredToken
    {
        /**
         * @param mixed $value El valor del token.
         * @param integer $expires_in El numero de segundos que faltan para que expire.
         * @param integer $expires_at Unix timestamp para indicar cuando expira la cookie.
         * @param string $path La ruta para la cookie, en caso de necesitarlo.
         */
        public RefreshToken(string value, int expires_in = 0, DateTime expires_at = default(DateTime), string path = "/")
            : base(value, expires_in = 0, expires_at = default(DateTime), path = "/")
        {
            //parent::__construct($value, $expires_in, $expires_at, $path);
        }

        /**
         * Asigna el tipo de token de que se trata. Cualquiera de los valores definidos en {@link iTokenTypes}
         *
         * esta funcion fuerza a las clases hija a asignar un valor a la variable "$this->type" para que a la hora de serializar
         * el contenido de esta clase sepamos de cual se trataba.
         *
         * @return void
         */
        override protected void setName()
        {
            this.name = REFRESH_TOKEN;
        }
    }
}
