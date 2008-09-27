using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.CSharp;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using dotXbrl.xbrlApi.Taxonomia;
using System.IO;
using dotXbrl;

namespace dotXbrl.GeneradorClases
{
    public class GeneradorClase
    {
#if DEBUG
        public static string fichero = "..\\..\\..\\xbrlCodeGenerator\\bin\\Debug\\xbrlCodeGenerator.exe";
#else
        public static string fichero = "..\\..\\..\\xbrlCodeGenerator\\bin\\Release\\xbrlCodeGenerator.exe";
#endif
        #region Siglenton

        private static Assembly _assembly = Assembly.LoadFrom(fichero);

        private Hashtable _assemblies;
        private Assembly _ensamblado = null;
        private static GeneradorClase _instance = null;

        public static GeneradorClase getInstance(Assembly ensamblado)
        {
            lock (_assembly)
            {
                if (_instance == null)
                    _instance = new GeneradorClase(ensamblado);
            }
            return _instance;
        }

        #endregion
        #region Definicion del tipo

        private IXBRLTupleDef _tupla;
        private IXBRLItemDef _item;
        private static string _nombreDirectorio = "";

        private GeneradorClase(Assembly ensamblado)
        {
            _assemblies = new Hashtable();
            _ensamblado = ensamblado;
        }

        public string NombreDirectorio
        {
            set { _nombreDirectorio = value; }
            get { return _nombreDirectorio; }
        }

        #endregion

        public StringBuilder generarCodigo(IXBRLTupleDef tupla)
        {
            StringBuilder nombreClase = new StringBuilder(), codigo = new StringBuilder();
            generarCodigoClase(tupla, nombreClase, codigo, true);

            escribirFicheroCodigo(codigo, nombreClase);

            return nombreClase;
        }
        public StringBuilder generarCodigo(IXBRLItemDef item)
        {
            StringBuilder nombreClase , codigo = new StringBuilder();
            generarCodigoClase(item,out nombreClase, codigo, true);

            escribirFicheroCodigo(codigo, nombreClase);
            escribirFicheroCodigo(generarAtributeClass(), new StringBuilder( "Elemento"));

            return nombreClase;
        }
        public StringBuilder generarClase(IXBRLItemDef item)
        {
            StringBuilder nombreClase = new StringBuilder(), codigo = new StringBuilder();
            generarCodigoClase(item,out nombreClase, codigo, false);

            Type t = null;
            if (_ensamblado != null)
                t = _ensamblado.GetType("dotXbrl." + nombreClase.ToString());
            if (t == null)
                compilarClase(codigo.ToString(), nombreClase.ToString());

            return nombreClase;
        }
        /// <summary>
        /// Genera un fichero con extensión .cs cuyo contenido es el código pasado por referencia.
        /// </summary>
        /// <param name="codigo">codigo c#</param>
        /// <param name="nombreClase">nombre del fichero sin extensión</param>
        private static void escribirFicheroCodigo(StringBuilder codigo, StringBuilder nombreClase)
        {
            StringBuilder sbFile = new StringBuilder();
            StreamWriter swWriter;
            string fileName, sDirLog;

            //obtenemos el directorio actual donde se está ejecutando la aplicación
            sDirLog = Directory.GetCurrentDirectory();
            //obtenemos la información del directorio
            DirectoryInfo informacionDirectorio = new DirectoryInfo(sDirLog + @"\" + _nombreDirectorio);
            //comprobamos si existe, si no exisitiese lo creamos
            if (!informacionDirectorio.Exists)
                informacionDirectorio.Create();
            //construimos la ruta del fichero a crear
            sbFile.Append(sDirLog);
            sbFile.Append(@"\").Append(_nombreDirectorio).Append(@"\").Append(nombreClase);
            sbFile.Append(".cs");
            fileName = sbFile.ToString();
            //si existe no hacemos nada
            if (!File.Exists(fileName))
            {
                //si no existe lo creamos
                swWriter = File.AppendText(fileName);
                //escribimos el contenido
                swWriter.WriteLine(codigo.ToString());
                //y cerramos el fichero
                swWriter.Close();
            }
        }
        public StringBuilder generarClase(IXBRLTupleDef tupla)
        {

            StringBuilder nombreClase = new StringBuilder(), codigo = new StringBuilder();
            generarCodigoClase(tupla, nombreClase, codigo, false);

            compilarClase(codigo, nombreClase);

            return nombreClase;
        }
        private void generarCodigoClase(IXBRLItemDef item,out StringBuilder nombreClase, StringBuilder codigo, bool generarMetodosEstaticos)
        {
            _item = item;
            nombreClase = obtenerNombreClaseITem();

            StringBuilder atributos = new StringBuilder(), geterSeter = new StringBuilder();
            StringBuilder metodosEstaticos = new StringBuilder();
            generadorMetodosEstaticos(nombreClase.ToString(), metodosEstaticos);
            string comentarioIniio = "", comentarioFin = "";
            StringBuilder atribNombreClase = new StringBuilder("\"");

            if (!item.Prefijo.Equals(""))
            {
                atribNombreClase.Append(nombreClase);
                atribNombreClase.Append("\",\"");
                atribNombreClase.Append(item.Prefijo);
                atribNombreClase.Append("\",\"");
                atribNombreClase.Append(item.ElementoItem.QualifiedName.Name);
                atribNombreClase.Append("\",\"");
                atribNombreClase.Append(item.ElementoItem.QualifiedName.Namespace);
                atribNombreClase.Append("\"");
            }
            StringBuilder atributeClass = new StringBuilder();
            if (!generarMetodosEstaticos)
            {
                comentarioFin = "*/";
                comentarioIniio = "/*";
                atributeClass = generarAtributeClass();
            }

            obtenerAtributosItem(atributos, geterSeter);
            #region Codigo a generar
            codigo.Append(@"using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using dotXbrl.xbrlApi.XBRL;

namespace dotXbrl{
    [Element(").Append(atribNombreClase).Append(@")]
    public class ").Append(nombreClase).Append(@":IConcepto
    {
private dotXbrl.xbrlApi.XBRL.IContexto _contexto = null;
#region Atributos
        ").Append(atributos).Append(@"
#endregion
#region Propiedades
        ").Append(geterSeter).Append(@"
#endregion
        public ").Append(nombreClase).Append(@"(){}
    #region IConcepto Members
    dotXbrl.xbrlApi.XBRL.IContexto dotXbrl.xbrlApi.XBRL.IConcepto.getContexto() { return _contexto; }
    void dotXbrl.xbrlApi.XBRL.IConcepto.setContexto(dotXbrl.xbrlApi.XBRL.IContexto contexto) { _contexto = contexto; }
    #endregion
    ").Append(comentarioIniio).Append(@"
        ").Append(metodosEstaticos).Append(@"
    ").Append(comentarioFin).Append(@"
    }
}

");
            #endregion
        }
        private StringBuilder generarAtributeClass()
        {
            return new StringBuilder(@"
[System.AttributeUsage(System.AttributeTargets.Class |
                   System.AttributeTargets.Struct)]
public class Element : System.Attribute
{
    private string _name, _prefix, _qualifiedName, _uriName;
    public double version;

    public Element(string name, string prefix,string qualifiedName,string UriName)
    {
        _name = name;
        _prefix = prefix;
        _uriName=UriName;
        _qualifiedName=qualifiedName;
        version = 1.0;
    }
    public string getNombre()
    {
        return _name;
    }
    public string getQualifiedName()
    {
        return _qualifiedName;
    }
    public string getUriName()
    {
        return _uriName;
    }
    public string getPrefix()
    {
        return _prefix;
    }
}");
        }
        private void obtenerAtributosItem(StringBuilder atributos, StringBuilder geterSeter)
        {
            atributos.Append(String.Format("private {0} {1};\n\r\t", _item.Tipo.Name, "valor"));

            char izq = '{', der = '}';
            geterSeter.Append(String.Format("public {0} {1}{2}\n\r\t", _item.Tipo.Name, "Valor", izq));

            geterSeter.Append(String.Format("get{1} return {0}; {2}", "valor", izq, der));
            geterSeter.Append(String.Format("set{1} {0} = value;{2}{2}", "valor", izq, der)).Append("\n\r\t");

        }
        private void generarCodigoClase(IXBRLTupleDef tupla,  StringBuilder nombreClase, StringBuilder codigo, bool generarMetodosEstaticos)
        {
            _tupla = tupla;
            nombreClase = new StringBuilder(obtenerNombreClaseTupla());

            StringBuilder atributos = new StringBuilder(), geterSeter = new StringBuilder();
            StringBuilder metodosEstaticos = new StringBuilder();
            generadorMetodosEstaticos(nombreClase.ToString(), metodosEstaticos);
            string comentarioIniio = "", comentarioFin = "";
            StringBuilder atribNombreClase = new StringBuilder("\"");

            if (!tupla.Prefijo.Equals(""))
            {
                atribNombreClase.Append(nombreClase);
                atribNombreClase.Append("\",\"");
                atribNombreClase.Append(tupla.Prefijo);
                atribNombreClase.Append("\",\"");
                atribNombreClase.Append(tupla.ElementoTupla.QualifiedName.Name);
                atribNombreClase.Append("\",\"");
                atribNombreClase.Append(tupla.ElementoTupla.QualifiedName.Namespace);
                atribNombreClase.Append("\"");
            }
            StringBuilder atributeClass = new StringBuilder();
            if (!generarMetodosEstaticos)
            {
                comentarioFin = "*/";
                comentarioIniio = "/*";
                atributeClass = generarAtributeClass();
            }

            obtenerAtributosTuple(atributos, geterSeter);
            #region Codigo a generar
            codigo.Append(@"using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using dotXbrl.xbrlApi.XBRL;

namespace dotXbrl{
[Element(").Append(atribNombreClase).Append(@")]
public class ").Append(nombreClase).Append(@":dotXbrl.xbrlApi.XBRL.IConcepto
{
private dotXbrl.xbrlApi.XBRL.IContexto _contexto = null;
#region Atributos
    ").Append(atributos).Append(@"
#endregion
#region Propiedades
    ").Append(geterSeter).Append(@"
#endregion
    public ").Append(nombreClase).Append(@"(){}

    #region IConcepto Members

    dotXbrl.xbrlApi.XBRL.IContexto dotXbrl.xbrlApi.XBRL.IConcepto.getContexto()
    {
        return _contexto;
    }

    void dotXbrl.xbrlApi.XBRL.IConcepto.setContexto(dotXbrl.xbrlApi.XBRL.IContexto contexto)
    {
        _contexto = contexto;
    }

    #endregion

").Append(comentarioIniio).Append(@"
    ").Append(metodosEstaticos).Append(@"
").Append(comentarioFin).Append(@"
}
}
").Append(atributeClass).Append(@"
");
            #endregion
        }
        private void compilarClase(StringBuilder codigo, StringBuilder nombreClase)
        {
            compilarClase(codigo.ToString(), nombreClase.ToString());
        }
        private void compilarClase(string codigo, string nombreClase)
        {
            foreach (Type type in _assembly.GetTypes())
            {
                if (type.IsClass == true)
                {

                    object[] param = new object[] { codigo };

                    Assembly ensamblado = (Assembly)type.InvokeMember("GenerarObjeto", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, param);
                    if (ensamblado != null)
                    {
                        if (!_assemblies.Contains(nombreClase))
                            _assemblies.Add(nombreClase, ensamblado);
                    }
                }
                break;
            }
        }
        /// <summary>
        /// Obtiene una instancia de una clase XBRL de las tuplas definidas en el DTS
        /// </summary>
        /// <param name="clase">nombre concepto</param>
        /// <returns>instancia del concepto</returns>
        public object ObtenerInstancia(string clase)
        {
            string fullNameClass = "dotXbrl." + clase;
            if (_assemblies.ContainsKey(clase))
            {
                Assembly a = (Assembly)_assemblies[clase];
                Type helloType = Type.GetType(clase);
                return a.CreateInstance(fullNameClass);
            }
            else
            {
                if (_ensamblado != null)
                {
                    Type t = _ensamblado.GetType(fullNameClass);
                    if (t != null)
                        return _ensamblado.CreateInstance(fullNameClass);
                    else
                        return null;
                }
                else
                    return null;
            }
            //MethodInfo mi = helloType.GetMethod("HolaMundo");

            //mi.Invoke(hello, new object[] { });
        }

        private void obtenerAtributosTuple(StringBuilder atributos, StringBuilder getterSetter)
        {
            foreach (IXBRLItemDef item in _tupla.Celdas)
            {
                atributos.Append(generarAtributoTuple(item));
                generarGetterSetterTuple(item, getterSetter);
            }
        }

        private void generarGetterSetterTuple(IXBRLItemDef item, StringBuilder src)
        {
            string[] div = item.Nombre.Split(':');
            string nombreAtributo = item.Nombre.Replace(':', '_').Replace('-', '_');
            string nombreTipo = item.Tipo.Name;
            string nombrePropiedad = div[div.Length - 1];

            char izq = '{', der = '}';

            src.Append(String.Format("public {0} {1}{2}\n\r\t", nombreTipo, nombrePropiedad, izq));
            src.Append(String.Format("get{1} return {0}; {2}", nombreAtributo, izq, der));
            src.Append(String.Format("set{1} {0} = value;{2}{2}", nombreAtributo, izq, der)).Append("\n\r\t");
        }
        /// <summary>
        /// Genera el código asociado al atributo privado de un atributo de una tupla XBRL
        /// </summary>
        /// <param name="item">Definición de item</param>
        /// <returns>código c#</returns>
        private string generarAtributoTuple(IXBRLItemDef item)
        {
            string nombreAtributo = item.Nombre.Replace(':', '_').Replace('-', '_');
            string nombreTipo = item.Tipo.Name;

            return String.Format("private {0} {1};\n\r\t", nombreTipo, nombreAtributo);
        }

        private string obtenerNombreClaseTupla()
        {
            string nombreTupla = _tupla.Nombre.Replace(':', '_').Replace('-', '_');
            return nombreTupla;
        }
        private StringBuilder obtenerNombreClaseITem()
        {
            string[] e = _item.Nombre.Split(':');
            string nombreTupla = e[e.Length - 1].Replace('-', '_');
            return new StringBuilder(nombreTupla);
        }
        /// <summary>
        /// Genera los métodos estáticos de la nueva clase que permitirán obtener instancias de las tuplas
        /// asociadas a su clase a partir de los ficheros XBRL instancia.
        /// </summary>
        /// <param name="nombreClase"></param>
        /// <param name="codigo"></param>
        private void generadorMetodosEstaticos(string nombreClase, StringBuilder codigo)
        {
            #region Definicion de codigo

            codigo.Append(@"public static ICollection<");
            codigo.Append(nombreClase);
            codigo.Append(@"> DevolverTodo()
    {
        IXBRLContenedorInstanciasObjetos a = XBRLContenedorObjetosInstancias.ObtenerInstancia();
        ICollection<object> listaObject = a.ObtenerInstanciaObjetosPorConcepto(""");
            codigo.Append(nombreClase);
            codigo.Append(@""");

        List<");
            codigo.Append(nombreClase);
            codigo.Append(@"> lista = new List<");
            codigo.Append(nombreClase);
            codigo.Append(@">();

        foreach (object o in listaObject)
        {
            lista.Add(Transformar(o));
        }
        return lista;
    }

    private static ");
            codigo.Append(nombreClase);
            codigo.Append(@" Transformar(object o)
    {
        ");
            codigo.Append(nombreClase);
            codigo.Append(@" negocio = new ");
            codigo.Append(nombreClase);
            codigo.Append(@"();

        Type tipo = o.GetType();
        Type tipoAtransformar = negocio.GetType();

        foreach (MethodInfo metodo in tipo.GetMethods())
        {
            string []metodoName=metodo.Name.Split('_');
            
            if (metodoName.Length>1&&metodoName[0].Equals(""get""))
            {
                string nombreMetodoABuscar = metodoName[1];
                MethodInfo informacionTipoATransformar = tipoAtransformar.GetMethod(""set_"" + nombreMetodoABuscar);
                object param = metodo.Invoke(o, new object[] { });
                informacionTipoATransformar.Invoke(negocio, new object[] { param });
            }
        }

        return negocio;
    }");
            #endregion
        }
    }

}