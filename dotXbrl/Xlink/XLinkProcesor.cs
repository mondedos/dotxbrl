using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;

namespace dotXbrl.xbrlApi.XLink
{
    #region Interfaz
    public interface IXLinkProcesor
    {
        /// <summary>
        /// Indica si se trata de un documento xlink
        /// </summary>
        bool isXLinkDocument { get;}
        /// <summary>
        /// Indica si el elemento y el ambito que define el documento xlink
        /// </summary>
        /// <param name="element">elemento raiz</param>
        /// <returns>cieto si es xlink</returns>
        bool ElementDefineXLinkDocument(XmlElement element);
        /// <summary>
        /// Documento que se esta analizando
        /// </summary>
        XmlDocument Document { set;get;}
        /// <summary>
        /// Coleccion de enlaces simples
        /// </summary>
        ICollection<XLinkSimple> EnlacesSimples { get;}
        /// <summary>
        /// Colección de enlaces extendidos
        /// </summary>
        ICollection<IXLinkExtendedLink> EnlacesExtendidos { get;}
        /// <summary>
        /// Prefijo que adquiere el espacio de nombres de xlink en el documento
        /// </summary>
        string Prefijo { get;}

        void Procesar();
        /// <summary>
        /// Dado un nodo raiz, explora el documento a partir del mismo nodo
        /// </summary>
        /// <param name="nodo">nodo padre a explorar</param>
        void explorarDocumento(XmlNode nodo);


    }

    public interface IXLinkExtendedLink
    {
        ICollection<IXLinkResourceLink> Recursos { get;}
        ICollection<IXLinkArco> Arcos { get;}
        ICollection<IXLinkLocatorLink> Localizadores { get;}
        XmlElement Elemento { get;set;}
        string Rol { get;set;}
        string Titulo { get;set;}
    }

    public interface IXLinkNodoArco
    {
        string Titulo { get;set;}
        string Rol { get;set;}
        string Etiqueta { get;set;}
    }

    public interface IXLinkResourceLink : IXLinkNodoArco
    {
    }

    public interface IXLinkLocatorLink : IXLinkNodoArco
    {
        string Recurso { get;set;}
    }

    public interface IXLinkArco
    {
        string Mostrar { get;set;}
        string Hacia { get;set;}
        string Desde { get;set;}
        string Actuar { get;set;}
        string RolArco { get;set;}
        string Titulo { get;set;}
        XmlElement Elemento { get;}
        ICollection<IXLinkNodoArco> ElementosOrigen { get;}
        ICollection<IXLinkNodoArco> ElementosDestino { get;}
    }
    #endregion

    #region Implementacion

    public class XLinkProcesorProvider : IXLinkProcesor
    {
        #region Definicion tipo

        private string _prefijo;
        private XmlDocument _documento;
        private bool _esXlink = false;
        private ICollection<XLinkSimple> _enlacesSimples;
        private ICollection<IXLinkExtendedLink> _enlacesExtendidos;

        #endregion

        public XLinkProcesorProvider()
        {
            _documento = null;
            _enlacesSimples = new List<XLinkSimple>();
            _enlacesExtendidos = new List<IXLinkExtendedLink>();
        }
        public XLinkProcesorProvider(XmlDocument documento)
            : this()
        {
            _documento = documento;
        }
        #region Metodos

        private bool mostrarNodo(XmlNode nodo)
        {
            //buscamos si es un documento XLink

            bool esComplejo = false;

            IXLinkProcesor p = this;

            if (!_esXlink && nodo.NodeType == XmlNodeType.Element && p.ElementDefineXLinkDocument((XmlElement)nodo))
            {
                _esXlink = true;
            }

            if (_esXlink && nodo.NodeType == XmlNodeType.Element)
            {
                esComplejo = makeLink((XmlElement)nodo);
            }

            return esComplejo;
        }
        private bool makeLink(XmlElement xmlElement)
        {
            bool esComplejo = false;
            string atributoABuscar = _prefijo + ":type";

            XmlNode atributoDecisor = xmlElement.Attributes.GetNamedItem(atributoABuscar);

            if (atributoDecisor != null && atributoDecisor.Value.ToLower().Equals("simple"))
            {
                XLinkSimple enlaceSimple = new XLinkSimple(xmlElement);

                enlaceSimple.Actuacion = getValueAtributo(xmlElement.Attributes, "actuate");
                enlaceSimple.Recurso = getValueAtributo(xmlElement.Attributes, "href");
                enlaceSimple.Rol = getValueAtributo(xmlElement.Attributes, "role");
                enlaceSimple.RolArco = getValueAtributo(xmlElement.Attributes, "arcrole");
                enlaceSimple.Titulo = getValueAtributo(xmlElement.Attributes, "title");

                _enlacesSimples.Add(enlaceSimple);
            }
            else if (atributoDecisor != null && atributoDecisor.Value.ToLower().Equals("extended"))
            {
                IXLinkExtendedLink enlaceExtendido = new XLinkExtendedLink(xmlElement);

                enlaceExtendido.Rol = getValueAtributo(xmlElement.Attributes, "role");
                enlaceExtendido.Titulo = getValueAtributo(xmlElement.Attributes, "title");

                esComplejo = xmlElement.HasChildNodes;

                if (esComplejo)
                {
                    //si es complejo y tiene hijos.
                    explorarEnlaceComplejo(xmlElement, enlaceExtendido);
                }

                ((XLinkExtendedLink)enlaceExtendido).dataBind();

                _enlacesExtendidos.Add(enlaceExtendido);
            }

            return esComplejo;
        }
        private void explorarEnlaceComplejo(XmlElement xmlElement, IXLinkExtendedLink enlaceExtendido)
        {
            foreach (XmlElement elementoHijo in xmlElement.ChildNodes)
            {
                XmlAttributeCollection atributos = elementoHijo.Attributes;

                string tipoElemento = getValueAtributo(atributos, "type").ToLower();

                if (tipoElemento.Equals("locator"))
                {
                    //se trata de un localizador
                    IXLinkLocatorLink localizador = new XLinkLocatorLink(elementoHijo);

                    localizador.Etiqueta = getValueAtributo(atributos, "label");
                    localizador.Recurso = getValueAtributo(atributos, "href");
                    localizador.Rol = getValueAtributo(atributos, "role");
                    localizador.Titulo = getValueAtributo(atributos, "title");

                    enlaceExtendido.Localizadores.Add(localizador);
                }
                else if (tipoElemento.Equals("arc"))
                {
                    //se trata de un arco
                    IXLinkArco arco = new XLinkArco(elementoHijo);

                    arco.Actuar = getValueAtributo(atributos, "actuate");
                    arco.Titulo = getValueAtributo(atributos, "title");
                    arco.Mostrar = getValueAtributo(atributos, "show");
                    arco.RolArco = getValueAtributo(atributos, "arcrole");
                    arco.Desde = getValueAtributo(atributos, "from");
                    arco.Hacia = getValueAtributo(atributos, "to");

                    enlaceExtendido.Arcos.Add(arco);
                }
                else if (tipoElemento.Equals("resource"))
                {
                    IXLinkResourceLink recurso = new XLinkResourceLink(elementoHijo);

                    recurso.Etiqueta = getValueAtributo(atributos, "label");
                    recurso.Rol = getValueAtributo(atributos, "role");
                    recurso.Titulo = getValueAtributo(atributos, "title");

                    enlaceExtendido.Recursos.Add(recurso);
                }
            }
        }
        private string getValueAtributo(XmlAttributeCollection coleccion, string valor)
        {
            XmlNode atributoDecisor = coleccion.GetNamedItem(_prefijo + ":" + valor);

            if (atributoDecisor != null)
            {
                return atributoDecisor.Value;
            }
            else
                return "";
        }

        #endregion

        #region IXLinkProcesor Members

        XmlDocument IXLinkProcesor.Document
        {
            get
            {
                return _documento;
            }
            set
            {
                _documento = value;
            }
        }
        bool IXLinkProcesor.ElementDefineXLinkDocument(XmlElement element)
        {
            bool loEs = false;

            XmlAttributeCollection atributos = element.Attributes;

            foreach (XmlAttribute atributo in atributos)
            {
                if (atributo.Value == "http://www.w3.org/1999/xlink")
                {
                    string[] name = atributo.Name.Split(':');
                    if (name.Length > 1)
                        _prefijo = name[1];
                    else
                        _prefijo = name[0];

                    loEs = true;
                    break;
                }
            }
            return loEs;
        }
        string IXLinkProcesor.Prefijo
        {
            get { return _prefijo; }
        }
        bool IXLinkProcesor.isXLinkDocument
        {
            get { return _esXlink; }
        }
        void IXLinkProcesor.Procesar()
        {
            IXLinkProcesor p = this;
            p.explorarDocumento(_documento.DocumentElement);
        }
        ICollection<XLinkSimple> IXLinkProcesor.EnlacesSimples
        {
            get
            {
                return _enlacesSimples;
            }
        }
        ICollection<IXLinkExtendedLink> IXLinkProcesor.EnlacesExtendidos
        {
            get { return _enlacesExtendidos; }
        }
        void IXLinkProcesor.explorarDocumento(XmlNode nodo)
        {
            bool esComplejo = false;
            IXLinkProcesor p = this;
            if (nodo != null)
                esComplejo = mostrarNodo(nodo);

            if (!esComplejo)
            {
                foreach (XmlNode nod in nodo.ChildNodes)
                {
                    p.explorarDocumento(nod);
                }
            }
        }

        #endregion
    }

    public class XLinkExtendedLink : IXLinkExtendedLink
    {
        #region Declaracion tipo

        private XmlElement _elemento;
        private string _title, _role;
        private ICollection<IXLinkLocatorLink> _localizadores;
        private ICollection<IXLinkArco> _arcos;
        private ICollection<IXLinkResourceLink> _recursos;

        #endregion


        public XLinkExtendedLink(XmlElement elemento)
        {
            _localizadores = new List<IXLinkLocatorLink>();
            _arcos = new List<IXLinkArco>();
            _recursos = new List<IXLinkResourceLink>();
            _elemento = elemento;
        }

        #region IXLinkExtendedLink Members

        ICollection<IXLinkResourceLink> IXLinkExtendedLink.Recursos
        {
            get { return _recursos; }
        }
        ICollection<IXLinkArco> IXLinkExtendedLink.Arcos
        {
            get { return _arcos; }
        }
        ICollection<IXLinkLocatorLink> IXLinkExtendedLink.Localizadores
        {
            get { return _localizadores; }
        }
        XmlElement IXLinkExtendedLink.Elemento
        {
            get
            {
                return _elemento;
            }
            set
            {
                _elemento = value;
            }
        }
        string IXLinkExtendedLink.Rol
        {
            get
            {
                return _role;
            }
            set
            {
                _role = value;
            }
        }
        string IXLinkExtendedLink.Titulo
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        #endregion


        #region Metodos
        internal void dataBind()
        {
            Hashtable cacheRecursos = new Hashtable();

            rellenarCache(cacheRecursos);

            foreach (IXLinkArco arco in _arcos)
            {
                string from = arco.Desde;
                string to = arco.Hacia;

                if (cacheRecursos.ContainsKey(from))
                {
                    foreach (IXLinkNodoArco elemento in (ICollection<IXLinkNodoArco>)cacheRecursos[from])
                    {
                        arco.ElementosOrigen.Add(elemento);
                    }
                }
                if (cacheRecursos.ContainsKey(to))
                {
                    foreach (IXLinkNodoArco elemento in (ICollection<IXLinkNodoArco>)cacheRecursos[from])
                    {
                        arco.ElementosDestino.Add(elemento);
                    }
                }
            }
        }
        private void rellenarCache(Hashtable cacheRecursos)
        {
            foreach (IXLinkLocatorLink localizador in _localizadores)
            {
                string etiqueta = localizador.Etiqueta;

                if (cacheRecursos.ContainsKey(etiqueta))
                {
                    ICollection<IXLinkNodoArco> lista = (ICollection<IXLinkNodoArco>)cacheRecursos[etiqueta];
                    lista.Add(localizador);
                }
                else
                {
                    ICollection<IXLinkNodoArco> lista = new List<IXLinkNodoArco>();

                    lista.Add(localizador);

                    cacheRecursos.Add(etiqueta, lista);
                }
            }
            foreach (IXLinkResourceLink recurso in _recursos)
            {
                string etiqueta = recurso.Etiqueta;

                if (cacheRecursos.ContainsKey(etiqueta))
                {
                    ICollection<IXLinkNodoArco> lista = (ICollection<IXLinkNodoArco>)cacheRecursos[etiqueta];
                    lista.Add(recurso);
                }
                else
                {
                    ICollection<IXLinkNodoArco> lista = new List<IXLinkNodoArco>();

                    lista.Add(recurso);

                    cacheRecursos.Add(etiqueta, lista);
                }
            }
        }
        #endregion
    }

    public class XLinkSimple
    {
        #region Declaracion tipo

        private string _href;
        private string _arcrole, _actuate;
        private XmlElement _elemento;
        private string _title, _role;

        public XmlElement Elemento
        {
            get
            {
                return _elemento;
            }
            set
            {
                _elemento = value;
            }
        }
        public string Rol
        {
            get
            {
                return _role;
            }
            set
            {
                _role = value;
            }
        }
        public string Titulo
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        public string Actuacion
        {
            get
            {
                return _actuate;
            }
            set
            {
                _actuate = value;
            }
        }
        public string RolArco
        {
            get
            {
                return _arcrole;
            }
            set { _arcrole = value; }
        }
        public string Recurso
        {
            get
            {
                return _href;
            }
            set
            {
                _href = value;
            }
        }

        #endregion

        public XLinkSimple(XmlElement elemento)
        {
            _elemento = elemento;
        }
    }

    public class XLinkResourceLink : IXLinkResourceLink
    {
        #region Definicion de tipo

        private XmlElement _elemento;
        private string _label, _role, _title;

        public XmlElement Elemento
        {
            get
            {
                return _elemento;
            }
        }

        #endregion

        public XLinkResourceLink(XmlElement elemento)
        {
            _elemento = elemento;
        }

        #region IXLinkNodoArco Members

        string IXLinkNodoArco.Titulo
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        string IXLinkNodoArco.Rol
        {
            get
            {
                return _role;
            }
            set
            {
                _role = value;
            }
        }
        string IXLinkNodoArco.Etiqueta
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
            }
        }

        #endregion
    }

    public class XLinkArco : IXLinkArco
    {
        #region Definicion del tipo

        private XmlElement _elemento;
        private string _arcrole, _title, _show, _actuate, _from, _to;
        ICollection<IXLinkNodoArco> _elementosOrigen, _elementosDestino;

        #endregion

        public XLinkArco(XmlElement elemento)
        {
            _elemento = elemento;
            _elementosDestino=new List<IXLinkNodoArco>();
            _elementosOrigen = new List<IXLinkNodoArco>();
        }

        #region IXLinkArco Members

        string IXLinkArco.Mostrar
        {
            get
            {
                return _show;
            }
            set
            {
                _show = value;
            }
        }
        string IXLinkArco.Hacia
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
            }
        }
        string IXLinkArco.Desde
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
            }
        }
        string IXLinkArco.Actuar
        {
            get
            {
                return _actuate;
            }
            set
            {
                _actuate = value;
            }
        }
        string IXLinkArco.RolArco
        {
            get
            {
                return _arcrole;
            }
            set
            {
                _arcrole = value;
            }
        }
        string IXLinkArco.Titulo
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        XmlElement IXLinkArco.Elemento
        {
            get
            {
                return _elemento;
            }
        }
        ICollection<IXLinkNodoArco> IXLinkArco.ElementosOrigen
        {
            get { return _elementosOrigen; }
        }
        ICollection<IXLinkNodoArco> IXLinkArco.ElementosDestino
        {
            get { return _elementosDestino; }
        }

        #endregion
    }

    /// <summary>
    /// Esta clase representa los localizadores en los enlaces extendidos de XLink
    /// </summary>
    public class XLinkLocatorLink : IXLinkLocatorLink
    {
        #region Definicion de tipo

        private XmlElement _elemento;
        private string _href, _role, _title, _label;

        public XmlElement Elemento
        {
            get { return _elemento; }
        }

        #endregion

        public XLinkLocatorLink(XmlElement elemento)
        {
            _elemento = elemento;
        }

        #region IXLinkLocatorLink Members

        string IXLinkLocatorLink.Recurso
        {
            get
            {
                return _href;
            }
            set
            {
                _href = value;
            }
        }

        #endregion

        #region IXLinkNodoArco Members

        string IXLinkNodoArco.Titulo
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        string IXLinkNodoArco.Rol
        {
            get
            {
                return _role;
            }
            set
            {
                _role = value;
            }
        }
        string IXLinkNodoArco.Etiqueta
        {
            get { return _label; }
            set
            {
                _label = value;
            }
        }

        #endregion
    }

    #endregion
}
