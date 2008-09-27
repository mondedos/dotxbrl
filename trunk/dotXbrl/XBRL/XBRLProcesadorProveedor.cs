using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using dotXbrl.xbrlApi.XLink;
using dotXbrl.xbrlApi.Taxonomia;
using System.Reflection;
using dotXbrl.GeneradorClases;

namespace dotXbrl.xbrlApi.XBRL
{
    public class XBRLProcesadorProveedor : IXBLRProcesador
    {
        private static string firma = "Generado por dotXBRL api";
        #region Definicion del tipo

        private XmlDocument _document;
        private IXLinkProcesor _xlinkProcesador;
        private IXBRLDataTaxonomySet _taxonomias;
        private ICollection<XmlElement> _instanciasConceptosPorProcesar = null;
        private System.Collections.Hashtable _contextos;
        private System.Reflection.Assembly _ensambladoCiente;
        private string _directorio = "";
        private StringBuilder _baseUri;
        private bool _esValido = true, _generarCodigo = false;


        #endregion

        public XBRLProcesadorProveedor(Uri url)
        {
            leerInstancia(url);
        }
        void leerInstancia(Uri documentoInstancia)
        {
            _esValido = true;
            _document = new XmlDocument();


            //leemos el xml
            _document.Load(documentoInstancia.OriginalString);

            _xlinkProcesador = new XLinkProcesorProvider();

            _taxonomias = new XBRLTaxonomias();
            _instanciasConceptosPorProcesar = new List<XmlElement>();

            _contextos = new System.Collections.Hashtable();
            _taxonomias.OptimizarEnsamblado(_ensambladoCiente);
        }

        #region Metodos Privados

        private void guardarDocumento(Uri documento)
        {
            XmlDocument nuevoDocumento = new XmlDocument();

            XmlElement elementoRaiz = copiarDocumentoSinConceptos(nuevoDocumento, _document);

            IXBLRProcesador p = this;
            rellenarDocumentoConInstancias(elementoRaiz, nuevoDocumento, p.ContenedorInstanciasConceptos);

            nuevoDocumento.Save(documento.OriginalString);
        }
        private void rellenarDocumentoConInstancias(XmlElement elementoRaiz, XmlDocument nuevoDocumento, IXBRLContenedorInstanciasObjetos iXBRLContenedorInstanciasObjetos)
        {
            foreach (string concepto in iXBRLContenedorInstanciasObjetos.Conceptos)
            {
                ICollection<object> conceptosDelMismoTipo = iXBRLContenedorInstanciasObjetos.ObtenerInstanciaObjetosPorConcepto(concepto);

                rellenarDocumentoConInstanciasConceptosMismoTipo(elementoRaiz, nuevoDocumento, conceptosDelMismoTipo);
            }
        }
        private void rellenarDocumentoConInstanciasConceptosMismoTipo(XmlElement elementoRaiz, XmlDocument nuevoDocumento, ICollection<object> conceptosDelMismoTipo)
        {
            string nombre = "", qualifiedName = "", uriName = "", prefijo = "";
            bool primero = true;
            foreach (object instancia in conceptosDelMismoTipo)
            {
                if (primero)
                    obtenerAtributosParametros(ref nombre, ref prefijo, ref qualifiedName, ref uriName, instancia);
                primero = false;

                XmlNode element = nuevoDocumento.CreateElement(prefijo, qualifiedName, uriName);
                rellenarElementoAPartirDeInstancia(element, instancia, nuevoDocumento);
                elementoRaiz.AppendChild(element);
            }
        }
        private void rellenarElementoAPartirDeInstancia(XmlNode element, object instancia, XmlDocument nuevoDocumento)
        {
            Type tipo = instancia.GetType();
            try
            {
                MethodInfo metodo = tipo.GetMethod("get_Valor");

                object valor = metodo.Invoke(instancia, new object[] { });

                XmlText textoValor = null;
                if (valor != null)
                    textoValor = nuevoDocumento.CreateTextNode(valor.ToString());
                else
                    textoValor = nuevoDocumento.CreateTextNode("");
                element.AppendChild(textoValor);
            }
            catch (NullReferenceException)
            {
                string nombre = "", qualifiedName = "", uriName = "", prefijo = "";
                obtenerAtributosParametros(ref nombre, ref prefijo, ref qualifiedName, ref uriName, instancia);
                foreach (MethodInfo metodo in tipo.GetMethods())
                {
                    string nombreMetodo = metodo.Name;
                    if (nombreMetodo.Substring(0, 4).Equals("get_"))
                    {
                        qualifiedName = nombreMetodo.Substring(4);
                        XmlNode subElement = nuevoDocumento.CreateElement(prefijo, qualifiedName, uriName);

                        object valor = metodo.Invoke(instancia, new object[] { });
                        XmlText textoValor = null;
                        if (valor != null)
                            textoValor = nuevoDocumento.CreateTextNode(valor.ToString());
                        else
                            textoValor = nuevoDocumento.CreateTextNode("");

                        subElement.AppendChild(textoValor);

                        element.AppendChild(subElement);
                    }
                }
            }

            IConcepto conceptoInstancia = (IConcepto)instancia;

            IContexto contexto = conceptoInstancia.getContexto();
            if (contexto != null)
            {
                XmlAttribute idContexto = nuevoDocumento.CreateAttribute("contextRef");
                idContexto.Value = contexto.Id;

                element.Attributes.Append(idContexto);
            }
        }
        private void obtenerAtributosParametros(ref string nombre, ref string prefijo, ref string qualifiedName, ref string uriName, object instancia)
        {
            Type tipo = instancia.GetType();
            object[] parametros = new object[] { };
            foreach (Attribute atributo in Attribute.GetCustomAttributes(tipo))
            {
                Type tipoAtributo = atributo.GetType();
                System.Reflection.MethodInfo metodo = tipoAtributo.GetMethod("getNombre");
                nombre = (string)metodo.Invoke(atributo, parametros);
                metodo = tipoAtributo.GetMethod("getQualifiedName");
                qualifiedName = (string)metodo.Invoke(atributo, parametros);
                metodo = tipoAtributo.GetMethod("getUriName");
                uriName = (string)metodo.Invoke(atributo, parametros);
                metodo = tipoAtributo.GetMethod("getPrefix");
                prefijo = (string)metodo.Invoke(atributo, parametros);
                break;
            }
        }
        private XmlElement copiarDocumentoSinConceptos(XmlDocument nuevoDocumento, XmlDocument document)
        {
            XmlElement elementoRaiz = null;
            XmlNode nodo = document.FirstChild;

            while (nodo != null)
            {
                switch (nodo.NodeType)
                {
                    case XmlNodeType.Comment:
                        nuevoDocumento.AppendChild(nuevoDocumento.CreateComment(firma));
                        break;
                    case XmlNodeType.Element:
                        elementoRaiz = LimpiarNuevoDocumento(nuevoDocumento, (XmlElement)nodo);
                        break;
                    default:
                        XmlNode minodo = nuevoDocumento.ImportNode(nodo, true);

                        nuevoDocumento.AppendChild(minodo);
                        break;
                }

                nodo = nodo.NextSibling;
            }
            return elementoRaiz;
        }
        private XmlElement LimpiarNuevoDocumento(XmlDocument nuevoDocumento, XmlElement nodo)
        {
            XmlElement elemento = (XmlElement)nuevoDocumento.ImportNode(nodo, true);

            nuevoDocumento.AppendChild(elemento);
            nuevoDocumento.Schemas = _taxonomias.Esquemas;
            nuevoDocumento.Validate(ShowCompileErrors);

            IList<XmlNode> listaHjos = new List<XmlNode>();

            foreach (XmlNode hijo in elemento.ChildNodes)
            {
                string grupoSustitucion = hijo.SchemaInfo.SchemaElement.SubstitutionGroup.Name.ToLower();
                if (!grupoSustitucion.Equals("item") && !grupoSustitucion.Equals("tuple"))
                {
                    listaHjos.Add(hijo.Clone());
                }
            }

            borrarTodosHijos(elemento);



            foreach (XmlNode hijo in listaHjos)
            {
                elemento.AppendChild(hijo);
            }

            return elemento;
        }
        private static void borrarTodosHijos(XmlElement elemento)
        {
            //borramos todos los hijos
            while (elemento.ChildNodes.Count > 0)
            {
                XmlNode hijo = elemento.ChildNodes.Item(0);

                elemento.RemoveChild(hijo);
            }
        }
        void xbrlElementDefinition(XmlElement xbrl)
        {
            _baseUri = new StringBuilder();

            Uri uri = new Uri(xbrl.BaseURI);

            _baseUri.Append(uri.Scheme).Append("://").Append(uri.Host);

            int uriSegmentsLength=uri.Segments.Length;
            for (int i = 0; i < uriSegmentsLength - 1; i++)
            {
                _baseUri.Append(uri.Segments[i]);
            }

            //buscamos las taxonomías
            _xlinkProcesador.explorarDocumento(xbrl);
            CargarTaxonomias(_xlinkProcesador.EnlacesSimples);

            //extraemos informacion de las taxonomías
            if (_generarCodigo)
                _taxonomias.MapearConceptosEnCodigo(_directorio);
            else
                _taxonomias.Procesar();

            //validar el documento
            _document.Schemas = _taxonomias.Esquemas;
            _document.Validate(ShowCompileErrors);

            //realiza el procesado del documento instancia
            if (!_generarCodigo)
                extraerInformaciónDocumento(xbrl);

        }
        private void extraerInformaciónDocumento(XmlElement xbrl)
        {
            foreach (XmlElement elemento in xbrl.ChildNodes)
            {
                string[] partes = elemento.Name.Split(':');
                string nombre = partes[partes.Length - 1].ToLower();

                switch (nombre)
                {
                    case "schemaref":
                        break;
                    case "context":
                        procesarContexto(elemento);
                        break;
                    case "unit":
                        break;
                    default:
                        // el resto de elementos, que precisamente serán instancias de conceptos
                        _instanciasConceptosPorProcesar.Add(elemento);
                        break;
                }
            }

            //procesamos las instancias de conceptos
            foreach (XmlElement instancia in _instanciasConceptosPorProcesar)
            {
                procesarTuple(instancia);
            }
        }
        private void procesarTuple(XmlElement elemento)
        {
            string[] partes = elemento.Name.Split(':');
            string nombreTupla = partes[partes.Length - 1].Replace('-', '_');

            GeneradorClase generador = GeneradorClase.getInstance(_ensambladoCiente);

            object objetoInstancia = generador.ObtenerInstancia(nombreTupla);

            Type tipoInstancia = objetoInstancia.GetType();
            try
            {
                foreach (XmlElement hijo in elemento.ChildNodes)
                {
                    string valor = hijo.FirstChild.Value;
                    string[] hijoPartes = hijo.Name.Split(':');
                    string nombreHijo = hijoPartes[hijoPartes.Length - 1];

                    rellenarAtributo(objetoInstancia, tipoInstancia, valor, nombreHijo);
                }
            }
            catch (InvalidCastException)
            {
                string valor = elemento.FirstChild.Value;
                rellenarAtributo(objetoInstancia, tipoInstancia, valor, "Valor");
            }

            //buscamos el contexto
            string idContext = elemento.GetAttribute("contextRef");
            IContexto contexto = (IContexto)_contextos[idContext];

            IConcepto conceptoInstancia = (IConcepto)objetoInstancia;

            conceptoInstancia.setContexto(contexto);
            IXBRLContenedorInstanciasObjetos contenedor = XBRLContenedorObjetosInstancias.ObtenerInstancia();

            contenedor.InsertarObjeto((IConcepto)objetoInstancia);
        }
        private static void rellenarAtributo(object objetoInstancia, Type tipoInstancia, string valor, string nombreHijo)
        {
            System.Reflection.MethodInfo metodo = tipoInstancia.GetMethod("set_" + nombreHijo);

            string tipoParaetro = metodo.GetParameters()[0].ParameterType.Name;

            object parametro = null;

            switch (tipoParaetro)
            {//completar con los tipos basicos de XML
                case "Decimal":
                    parametro = Decimal.Parse(valor);
                    break;
            }

            metodo.Invoke(objetoInstancia, new object[] { parametro });
        }
        private void procesarContexto(XmlElement elemento)
        {
            IContexto contexto = new Contexto();

            contexto.Id = elemento.GetAttribute("id");

            foreach (XmlElement estructura in elemento.ChildNodes)
            {
                string[] s = estructura.Name.Split(':');
                string e = s[s.Length - 1].ToLower();

                if (e.Equals("entity"))
                {
                    XmlNode identificador = estructura.FirstChild;

                    IIdentificadorContexto contextoIdentificador = contexto.Identificador;
                    contextoIdentificador.Descripcion = identificador.FirstChild.Value;
                    contextoIdentificador.URI = identificador.Attributes[0].Value;

                    //extraccion de segmentos
                    XmlNode segmentos = identificador.NextSibling;

                    if (segmentos != null)
                    {
                        XmlElement segmentos = contexto.Segmentos;
                        foreach (XmlElement segmento in segmentos.ChildNodes)
                        {
                            segmentos.Add(segmento);
                        }
                    }
                }
                else if (e.Equals("period"))
                {
                    XmlNode primerHijo = estructura.FirstChild;
                    string elem = primerHijo.Name.ToLower();

                    if (elem.Equals("startdate"))
                    {
                        IPeriodoInicioFin periodo = new PeriodoInicioFin(primerHijo.FirstChild.Value,
                            primerHijo.NextSibling.FirstChild.Value);

                        contexto.Periodo = periodo;
                    }
                    else
                    {
                        IPeriodoInstante periodo = new PeriodoInstante(primerHijo.FirstChild.Value);

                        contexto.Periodo = periodo;
                    }
                }
                //else if (e.Equals("scenario"))
                //{
                //}
            }

            _contextos.Add(contexto.Id, contexto);
            //throw new Exception("The method or operation is not implemented.");
        }
        private void CargarTaxonomias(ICollection<XLinkSimple> iCollection)
        {
            foreach (XLinkSimple encaleSimple in iCollection)
            {
                string nombre = encaleSimple.Elemento.Name;

                string[] nombrePartido = nombre.Split(':');

                if (nombrePartido.Length > 1 && nombrePartido[1].ToLower().Equals("schemaref"))
                {
                    //estamos en la referencia a una taxonomía
                    _taxonomias.Insertar(_baseUri.ToString() + encaleSimple.Recurso);
                }
            }
        }
        /// <summary>
        /// Muestra por consola si se ha producido algún error en el proceso de compilación.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowCompileErrors(object sender, ValidationEventArgs e)
        {
            _esValido = false;

            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Schema Validation Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Schema Validation Warning: {0}", e.Message);
                    break;
            }
        }
        #endregion

        #region XBLRProcesador Members

        void IXBLRProcesador.LeerXBRLInstancia(Uri documento)
        {
            leerInstancia(documento);
        }
        void IXBLRProcesador.Procesar()
        {
            XmlElement elemento = _document.DocumentElement;

            string[] raiz = elemento.Name.Split(':');

            string nombreRaiz = raiz[raiz.Length - 1].ToLower();

            if (nombreRaiz.Equals("xbrl"))
            {
                xbrlElementDefinition(elemento);
            }
            else
                throw new XBRLException("No es un documento XBRL");
        }
        IXBRLContenedorInstanciasObjetos IXBLRProcesador.ContenedorInstanciasConceptos
        {
            get { return XBRLContenedorObjetosInstancias.ObtenerInstancia(); }
        }
        bool IXBLRProcesador.EsDocumentoValido
        {
            get { return _esValido; }
        }
        void IXBLRProcesador.MapearAObjetos()
        {
            _generarCodigo = true;

            IXBLRProcesador proc = this;
            proc.Procesar();
        }
        void IXBLRProcesador.GuardarDocumento(Uri documento)
        {
            guardarDocumento(documento);
        }
        void IXBLRProcesador.OptimizarEnsamblado(System.Reflection.Assembly ensamblado)
        {
            _ensambladoCiente = ensamblado;
            _taxonomias.OptimizarEnsamblado(ensamblado);
        }


        void IXBLRProcesador.MapearAObjetos(string nombreDirectorio)
        {
            _generarCodigo = true;
            _directorio = nombreDirectorio;
            IXBLRProcesador proc = this;
            proc.Procesar();
        }

        #endregion
    }

    public class XBRLException : Exception
    {
        public XBRLException(string e)
            : base(e)
        {
        }
    }
}
