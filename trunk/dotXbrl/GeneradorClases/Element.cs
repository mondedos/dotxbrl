using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace dotXbrl.xbrlApi.XBRL
{
    /// <summary>
    /// Esta clase nos será bastante util para la reflexion de clases generadas por el componente
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct)]
    public class Element : System.Attribute
    {
        private string _name, _prefix, _qualifiedName, _uriName;
        public double version;
        /// <summary>
        /// Constructor del atributo
        /// </summary>
        /// <param name="name">Nombre del atributo</param>
        /// <param name="prefix">Prefijo del elemento en XBRL</param>
        /// <param name="qualifiedName">Nombre cualificado en XML</param>
        /// <param name="UriName">Direccion del recurso</param>
        public Element(string name, string prefix, string qualifiedName, string UriName)
        {
            _name = name;
            _prefix = prefix;
            _uriName = UriName;
            _qualifiedName = qualifiedName;
            version = 1.0;
        }
        /// <summary>
        /// Obtiene el nombre del atributo
        /// </summary>
        /// <returns>Nombre</returns>
        public string getNombre()
        {
            return _name;
        }
        /// <summary>
        /// Obtiene el nombre cualificado del elemento
        /// </summary>
        /// <returns>Nombre cualificado</returns>
        public string getQualifiedName()
        {
            return _qualifiedName;
        }
        /// <summary>
        /// Obtiene el URI del recurso
        /// </summary>
        /// <returns>URI</returns>
        public string getUriName()
        {
            return _uriName;
        }
        /// <summary>
        /// Obtiene el prefijo del elemento en XBRL
        /// </summary>
        /// <returns>Prefijo</returns>
        public string getPrefix()
        {
            return _prefix;
        }
    }
}