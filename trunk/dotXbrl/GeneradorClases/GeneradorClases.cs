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

        public string generarCodigo(IXBRLTupleDef tupla)
        {
            string nombreClase, codigo;
            generarCodigoClase(tupla, out nombreClase, out codigo, true);

            escribirFicheroCodigo(new StringBuilder(codigo), nombreClase);

            return nombreClase;
        }
        public string generarCodigo(IXBRLItemDef item)
        {
            string nombreClase, codigo;
            generarCodigoClase(item, out nombreClase, out codigo, true);

            escribirFicheroCodigo(new StringBuilder(codigo), nombreClase);
            escribirFicheroCodigo(new StringBuilder(generarAtributeClass()), "Elemento");

            return nombreClase;
        }
        public string generarClase(IXBRLItemDef item)
        {
            string nombreClase, codigo;
            generarCodigoClase(item, out nombreClase, out codigo, false);

            Type t = null;
            if (_ensamblado != null)
                t = _ensamblado.GetType("dotXbrl." + nombreClase);
            if (t == null)
                compilarClase(codigo, nombreClase);

            return nombreClase;
        }
        private static void escribirFicheroCodigo(StringBuilder codigo, string nombreClase)
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
            sbFile.Append(@"\" + _nombreDirectorio + @"\" + nombreClase);
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
        public string generarClase(IXBRLTupleDef tupla)
        {

            string nombreClase, codigo;
            generarCodigoClase(tupla, out nombreClase, out codigo, false);

            compilarClase(codigo, nombreClase);

            return nombreClase;
        }
        private void generarCodigoClase(IXBRLItemDef item, out string nombreClase, out string codigo, bool generarMetodosEstaticos)
        {
            _item = item;
            nombreClase = obtenerNombreClaseITem();

            string atributos = "", geterSeter = "";
            StringBuilder metodosEstaticos = new StringBuilder();
            generadorMetodosEstaticos(nombreClase, metodosEstaticos);
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
            string atributeClass = "";
            if (!generarMetodosEstaticos)
            {
                comentarioFin = "*/";
                comentarioIniio = "/*";
                atributeClass = generarAtributeClass();
            }

            obtenerAtributosItem(ref atributos, ref geterSeter);
            #region Codigo a generar
            codigo = @"using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using dotXbrl.xbrlApi.XBRL;

namespace dotXbrl{
    [Element(" + atribNombreClase.ToString() + @")]
    public class " + nombreClase + @":IConcepto
    {
private dotXbrl.xbrlApi.XBRL.IContexto _contexto = null;
#region Atributos
        " + atributos + @"
#endregion
#region Propiedades
        " + geterSeter + @"
#endregion
        public " + nombreClase + @"(){}
    #region IConcepto Members
    dotXbrl.xbrlApi.XBRL.IContexto dotXbrl.xbrlApi.XBRL.IConcepto.getContexto() { return _contexto; }
    void dotXbrl.xbrlApi.XBRL.IConcepto.setContexto(dotXbrl.xbrlApi.XBRL.IContexto contexto) { _contexto = contexto; }
    #endregion
    " + comentarioIniio + @"
        " + metodosEstaticos.ToString() + @"
    " + comentarioFin + @"
    }
}

";
            #endregion
        }
        private string generarAtributeClass()
        {
            return @"
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
}";
        }
        private void obtenerAtributosItem(ref string atributos, ref string geterSeter)
        {
            atributos += String.Format("private {0} {1};\n\r\t", _item.Tipo.Name, "valor");

            char izq = '{', der = '}';

            string src = "";
            src += String.Format("public {0} {1}{2}\n\r\t", _item.Tipo.Name, "Valor", izq);

            src += String.Format("get{1} return {0}; {2}", "valor", izq, der);
            src += String.Format("set{1} {0} = value;{2}{2}", "valor", izq, der) + "\n\r\t";

            geterSeter += src;
        }
        private void generarCodigoClase(IXBRLTupleDef tupla, out string nombreClase, out string codigo, bool generarMetodosEstaticos)
        {
            _tupla = tupla;
            nombreClase = obtenerNombreClaseTupla();

            string atributos = "", geterSeter = "";
            StringBuilder metodosEstaticos = new StringBuilder();
            generadorMetodosEstaticos(nombreClase, metodosEstaticos);
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
            string atributeClass = "";
            if (!generarMetodosEstaticos)
            {
                comentarioFin = "*/";
                comentarioIniio = "/*";
                atributeClass = generarAtributeClass();
            }

            obtenerAtributosTuple(ref atributos, ref geterSeter);
            #region Codigo a generar
            codigo = @"using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using dotXbrl.xbrlApi.XBRL;

namespace dotXbrl{
[Element(" + atribNombreClase.ToString() + @")]
public class " + nombreClase + @":dotXbrl.xbrlApi.XBRL.IConcepto
{
private dotXbrl.xbrlApi.XBRL.IContexto _contexto = null;
#region Atributos
    " + atributos + @"
#endregion
#region Propiedades
    " + geterSeter + @"
#endregion
    public " + nombreClase + @"(){}

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

" + comentarioIniio + @"
    " + metodosEstaticos.ToString() + @"
" + comentarioFin + @"
}
}
" + atributeClass + @"
";
            #endregion
        }

        private void compilarClase(string codigo, string nombreClase)
        {
            foreach (Type type in _assembly.GetTypes())
            {
                if (type.IsClass == true)
                {

                    object[] param = new object[1];
                    param[0] = codigo;

                    Assembly a = (Assembly)type.InvokeMember("GenerarObjeto", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, param);
                    if (a != null)
                    {
                        if (!_assemblies.Contains(nombreClase))
                            _assemblies.Add(nombreClase, a);
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
            if (_assemblies.ContainsKey(clase))
            {
                Assembly a = (Assembly)_assemblies[clase];
                Type helloType = Type.GetType(clase);
                return a.CreateInstance("dotXbrl." + clase);
            }
            else
            {
                if (_ensamblado != null)
                {
                    Type t = _ensamblado.GetType("dotXbrl." + clase);
                    if (t != null)
                        return _ensamblado.CreateInstance("dotXbrl." + clase);
                    else
                        return null;
                }
                else
                    return null;
            }
            //MethodInfo mi = helloType.GetMethod("HolaMundo");

            //mi.Invoke(hello, new object[] { });
        }

        private void obtenerAtributosTuple(ref string atributos, ref string getterSetter)
        {
            StringBuilder atributosSb = new StringBuilder(), getterSetterSb = new StringBuilder();
            foreach (IXBRLItemDef item in _tupla.Celdas)
            {
                atributosSb.Append(generarAtributoTuple(item));
                generarGetterSetterTuple(item, getterSetterSb);
            }
            atributos = atributosSb.ToString();
            getterSetter = getterSetterSb.ToString();
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
            src.Append(String.Format("set{1} {0} = value;{2}{2}", nombreAtributo, izq, der) + "\n\r\t");
        }

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
        private string obtenerNombreClaseITem()
        {
            string[] e = _item.Nombre.Split(':');
            string nombreTupla = e[e.Length - 1].Replace('-', '_');
            return nombreTupla;
        }
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