using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace dotXbrl.xbrlApi.Taxonomia
{
    class XBRLTaxonomias : IXBRLDataTaxonomySet
    {
        #region Definicion Tipo

        private XmlSchemaSet _conjuntoTaxonomias;

        private Hashtable _items, _prefijosEspaciosNombres;
        private ICollection<IXBRLTupleDef> _tuplas;
        private bool _generarCodigo = false;
        private ICollection<string> _clases;
        private System.Reflection.Assembly _ensambladoCliente;
        private string _prefijo4Normativo, _prefijoTaxonomíaActual,_nombreDirectorioACrear="";

        public ICollection<string> Clases
        {
            get { return _clases; }
        }
        #endregion

        public XBRLTaxonomias()
        {
            _conjuntoTaxonomias = new XmlSchemaSet();
            _items = new Hashtable();
            _prefijosEspaciosNombres = new Hashtable();
            _tuplas = new List<IXBRLTupleDef>();
            _clases = new List<string>();
        }

        #region Metodos

        void ManejadorEventoEsquemaInvalido(object o, ValidationEventArgs e)
        {
            throw new Exception("Esquema Invalido");
        }

        private bool comprobarSiEsTaxonomia(XmlSchema myEsquemaObjeto)
        {
            bool encontrado = false;

            XmlQualifiedName[] espacios = myEsquemaObjeto.Namespaces.ToArray();

            string target = myEsquemaObjeto.TargetNamespace;

            foreach (XmlQualifiedName e in espacios)
            {
                if (e.Namespace.ToLower().Equals("http://www.xbrl.org/2003/instance"))
                {
                    _prefijo4Normativo = e.Name.ToLower();
                    encontrado = true;
                }
                else if (e.Namespace.Equals(target))
                {
                    _prefijoTaxonomíaActual = e.Name;
                }
            }
            return encontrado;
        }

        private string getValueAtributo(XmlAttribute[] x, string valor)
        {
            foreach (XmlAttribute atributo in x)
            {
                if (atributo.Name.ToLower().Equals(_prefijo4Normativo + ":" + valor))
                {
                    return atributo.Value;
                }
            }

            return "";
        }

        private string getValueAtributo(XmlAttributeCollection coleccion, string valor)
        {
            XmlNode atributoDecisor = coleccion.GetNamedItem(_prefijo4Normativo + ":" + valor);

            if (atributoDecisor != null)
            {
                return atributoDecisor.Value;
            }
            else
                return "";
        }
        private void rellenarPrefijosEspaciosNombres()
        {
            foreach (XmlSchema esquema in _conjuntoTaxonomias.Schemas())
            {
                foreach (XmlQualifiedName espacioNombre in esquema.Namespaces.ToArray())
                {
                    string uri = espacioNombre.Namespace;
                    string prefijo = espacioNombre.Name;

                    if (!_prefijosEspaciosNombres.Contains(uri))
                        _prefijosEspaciosNombres.Add(uri, prefijo);
                }
            }
        }
        /// <summary>
        /// Este metodo nos sirve para extraer los conceptos
        /// </summary>
        /// <param name="xmlSchemaObjectTable"></param>
        private void procesarElementosGlobales(XmlSchemaObjectTable xmlSchemaObjectTable)
        {
            if (_nombreDirectorioACrear.Equals(""))
                _nombreDirectorioACrear = _prefijoTaxonomíaActual;
            Hashtable tuplasPorRevisar = new Hashtable();

            //procesamos los elementos declarados globalmente
            foreach (XmlSchemaElement concepto in xmlSchemaObjectTable.Values)
            {
                string grupoSustitucion = concepto.SubstitutionGroup.Name.ToLower();
                if (grupoSustitucion.Equals("item"))
                {
                    IXBRLItemDef item = new XBLRLItemDefinicion(concepto);

                    item.Id = concepto.Id;
                    item.SemanticaTipo = concepto.SchemaTypeName.Name;
                    item.EspacioNombre = concepto.QualifiedName.Namespace;
                    item.Prefijo = (string)_prefijosEspaciosNombres[concepto.QualifiedName.Namespace];
                    if (_prefijosEspaciosNombres.Contains(item.EspacioNombre))
                        item.Nombre = (string)_prefijosEspaciosNombres[item.EspacioNombre] + ':' + concepto.Name;
                    else
                        item.Nombre = concepto.Name;

                    if (concepto.ElementSchemaType.GetType() == typeof(XmlSchemaComplexType))
                    {
                        XmlSchemaComplexType complexType = (XmlSchemaComplexType)concepto.ElementSchemaType;

                        item.Tipo = complexType.Datatype.ValueType;
                    }

                    item.BalanceAsociado = Balance.Credit;
                    if (getValueAtributo(concepto.UnhandledAttributes, "balance").Equals("debit"))
                        item.BalanceAsociado = Balance.Debit;

                    item.Periodo = Period.Duration;
                    if (getValueAtributo(concepto.UnhandledAttributes, "periodtype").Equals("instant"))
                        item.Periodo = Period.Instant;

                    if (_items.Contains(item.Nombre))
                    {
                        IXBRLItemDef guardado = (IXBRLItemDef)_items[item.Nombre];
                    }

                    _items.Add(item.Nombre, item);

                    GeneradorClases.GeneradorClase gc = GeneradorClases.GeneradorClase.getInstance(_ensambladoCliente);
                    gc.NombreDirectorio = _nombreDirectorioACrear;

                    if (_generarCodigo)
                        gc.generarCodigo(item);
                    else
                        _clases.Add(gc.generarClase(item));
                }
                else if (grupoSustitucion.Equals("tuple"))
                {
                    tuplasPorRevisar.Add(concepto.Name, concepto);
                }
            }

            //miramos las tuplas que no fueron terminadas de rellenar
            foreach (XmlSchemaElement elementoTuple in tuplasPorRevisar.Values)
            {
                IXBRLTupleDef tupla = new XBRLTupleDefinicion(elementoTuple);

                tupla.Nombre = elementoTuple.Name;
                tupla.Id = elementoTuple.Id;
                tupla.Prefijo = (string)_prefijosEspaciosNombres[elementoTuple.QualifiedName.Namespace];


                if (elementoTuple.SchemaType.GetType() == typeof(XmlSchemaComplexType))
                {
                    XmlSchemaComplexType tipo = (XmlSchemaComplexType)elementoTuple.SchemaType;

                    if (tipo.ContentTypeParticle.GetType() == typeof(XmlSchemaSequence))
                    {
                        XmlSchemaSequence elementos = (XmlSchemaSequence)tipo.ContentTypeParticle;

                        //para cada elemento de la tupla
                        foreach (XmlSchemaElement elemento in elementos.Items)
                        {
                            IXBRLItemDef item = buscarItem(elemento.QualifiedName);

                            if (item != null)
                            {
                                tupla.Celdas.Add(item);
                            }
                        }
                    }

                    _tuplas.Add(tupla);
                    GeneradorClases.GeneradorClase gc = GeneradorClases.GeneradorClase.getInstance(_ensambladoCliente);
                    gc.NombreDirectorio = _nombreDirectorioACrear;

                    if (_generarCodigo)
                        gc.generarCodigo(tupla);
                    else
                        _clases.Add(gc.generarClase(tupla));
                }
            }
        }

        #endregion
        #region IXBRLTaxonomia Members

        bool IXBRLDataTaxonomySet.Insertar(Uri url)
        {
            IXBRLDataTaxonomySet p = this;
            return p.Insertar(url.AbsoluteUri);
        }

        bool IXBRLDataTaxonomySet.Insertar(string url)
        {
            XmlTextReader lector = new XmlTextReader(url);
            XmlSchema myXmlSchema = XmlSchema.Read(lector, new ValidationEventHandler(ManejadorEventoEsquemaInvalido));//ManejadorEventoEsquemaInvalido

            if (comprobarSiEsTaxonomia(myXmlSchema))
            {
                _conjuntoTaxonomias.Add(myXmlSchema);
                lector.Close();
                return true;
            }
            else
            {
                lector.Close();
                return false;
            }
        }

        void IXBRLDataTaxonomySet.Procesar()
        {
            bool anterior = _generarCodigo;
            _generarCodigo = false;
            _conjuntoTaxonomias.Compile();

            rellenarPrefijosEspaciosNombres();

            procesarElementosGlobales(_conjuntoTaxonomias.GlobalElements);
            _generarCodigo = anterior;
        }

        void IXBRLDataTaxonomySet.MapearConceptosEnCodigo()
        {
            bool anterior = _generarCodigo;
            _generarCodigo = true;
            _conjuntoTaxonomias.Compile();

            rellenarPrefijosEspaciosNombres();

            procesarElementosGlobales(_conjuntoTaxonomias.GlobalElements);
            _generarCodigo = anterior;
        }
        void IXBRLDataTaxonomySet.MapearConceptosEnCodigo(string nombreDirectorio)
        {
            _nombreDirectorioACrear = nombreDirectorio;
            bool anterior = _generarCodigo;
            _generarCodigo = true;
            _conjuntoTaxonomias.Compile();

            rellenarPrefijosEspaciosNombres();

            procesarElementosGlobales(_conjuntoTaxonomias.GlobalElements);
            _generarCodigo = anterior;
        }
        private IXBRLItemDef buscarItem(XmlQualifiedName xmlQualifiedName)
        {
            string nombre = "";
            string espacio = xmlQualifiedName.Namespace;
            if (_prefijosEspaciosNombres.Contains(espacio))
                nombre = (string)_prefijosEspaciosNombres[espacio] + ':' + xmlQualifiedName.Name;

            foreach (IXBRLItemDef item in _items.Values)
            {
                if (item.Nombre.Equals(nombre) && item.EspacioNombre.Equals(espacio))
                {
                    return item;
                }
            }

            return null;
        }

        Hashtable IXBRLDataTaxonomySet.Items
        {
            get { return _items; }
        }

        ICollection<IXBRLTupleDef> IXBRLDataTaxonomySet.Tuplas
        {
            get { return _tuplas; }
        }

        string IXBRLDataTaxonomySet.Prefijo
        {
            get { return _prefijoTaxonomíaActual; }
        }

        XmlSchemaSet IXBRLDataTaxonomySet.Esquemas
        {
            get { return _conjuntoTaxonomias; }
        }
        void IXBRLDataTaxonomySet.OptimizarEnsamblado(System.Reflection.Assembly ensambladoCliente)
        {
            _ensambladoCliente = ensambladoCliente;
        }

        #endregion
    }

    public class XBRLTupleDefinicion : IXBRLTupleDef
    {
        #region Definicion de tipo

        private string _nombre, _id, _prefijo;
        private XmlSchemaElement _elemento;

        private ICollection<IXBRLItemDef> _celdas;

        #endregion

        public XBRLTupleDefinicion(XmlSchemaElement elemento)
        {
            _elemento = elemento;
            _celdas = new List<IXBRLItemDef>();
        }

        #region IXBRLTupleDef Members

        string IXBRLTupleDef.Nombre
        {
            get
            {
                return _nombre;
            }
            set
            {
                _nombre = value;
            }
        }

        string IXBRLTupleDef.Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        ICollection<IXBRLItemDef> IXBRLTupleDef.Celdas
        {
            get { return _celdas; }
        }

        string IXBRLTupleDef.Prefijo
        {
            get
            {
                return _prefijo;
            }
            set
            {
                _prefijo = value;
            }
        }
        XmlSchemaElement IXBRLTupleDef.ElementoTupla
        {
            get { return _elemento; }
        }

        #endregion
    }
    public class XBLRLItemDefinicion : IXBRLItemDef
    {
        #region Definicion de tipo

        private string _nombre, _id, _semanticaTipo, _espacioNombre, _prefijo;
        private XmlSchemaElement _elemento;
        private Period _periodo;
        private Balance _balance;

        private Type _tipo;

        public XBLRLItemDefinicion(XmlSchemaElement elemento)
        {
            _elemento = elemento;
        }
        #endregion

        #region IXBRLItemDef Members

        string IXBRLItemDef.Nombre
        {
            get
            {
                return _nombre;
            }
            set
            {
                _nombre = value;
            }
        }

        string IXBRLItemDef.Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        Type IXBRLItemDef.Tipo
        {
            get
            {
                return _tipo;
            }
            set
            {
                _tipo = value;
            }
        }

        string IXBRLItemDef.SemanticaTipo
        {
            get
            {
                return _semanticaTipo;
            }
            set
            {
                _semanticaTipo = value;
            }
        }

        Balance IXBRLItemDef.BalanceAsociado
        {
            get
            {
                return _balance;
            }
            set
            {
                _balance = value;
            }
        }

        Period IXBRLItemDef.Periodo
        {
            get
            {
                return _periodo;
            }
            set
            {
                _periodo = value;
            }
        }

        string IXBRLItemDef.EspacioNombre
        {
            get
            {
                return _espacioNombre;
            }
            set
            {
                _espacioNombre = value;
            }
        }

        string IXBRLItemDef.Prefijo
        {
            get
            {
                return _prefijo;
            }
            set
            {
                _prefijo = value;
            }
        }

        XmlSchemaElement IXBRLItemDef.ElementoItem
        {
            get { return _elemento; }
        }

        #endregion
    }

}
