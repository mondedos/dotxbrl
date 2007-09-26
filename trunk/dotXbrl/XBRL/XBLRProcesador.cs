using System;
using System.Collections.Generic;
using System.Text;

namespace dotXbrl.xbrlApi.XBRL
{
    public interface IXBLRProcesador
    {
        /// <summary>
        /// Procesa el documento intancia XBRL asociado
        /// </summary>
        void Procesar();
        /// <summary>
        /// Indica la fuente de datos (documento instancia XBRL) que se pretende procesar
        /// </summary>
        /// <param name="documento">documento instancia XBRL</param>
        void LeerXBRLInstancia(Uri documento);
        /// <summary>
        /// Genera a partir de los conceptos del conjuto DTS una serie de ficheros de texto .cs
        /// con la codificación en clases de los conceptos encontrados
        /// 
        /// Para regenerar los ficheros, el programador deberá primero borrar los ficheros generados,
        /// en caso contrario, no se sobreescribirán los ficheros
        /// </summary>
        void MapearAObjetos();
        /// <summary>
        /// Genera a partir de los conceptos del conjuto DTS una serie de ficheros de texto .cs
        /// con la codificación en clases de los conceptos encontrados
        /// 
        /// Para regenerar los ficheros, el programador deberá primero borrar los ficheros generados,
        /// en caso contrario, no se sobreescribirán los ficheros
        /// </summary>
        /// <param name="nombreDirectorio">nombre del directorio donde se almacenaran los ficheros</param>
        void MapearAObjetos(string nombreDirectorio);
        /// <summary>
        /// Almacena un documento en la ruta especificada, no modificando aquellos elementos del documento
        /// instancia que no sean los conceptos
        /// </summary>
        /// <param name="documento"></param>
        void GuardarDocumento(Uri documento);
        /// <summary>
        /// Nos devuelve un contenedor de instancias de conceptos XBRL
        /// </summary>
        IXBRLContenedorInstanciasObjetos ContenedorInstanciasConceptos { get;}
        /// <summary>
        /// Indica si un documento es válido o no
        /// </summary>
        bool EsDocumentoValido { get;}
        /// <summary>
        /// Esta funcion se utiliza si tenemos las clases mapeadas como fichero .cs
        /// 
        /// Con esto se consigue, que cada vez que procesemos un fichero, no compilar las clases de nuevo
        /// </summary>
        /// <param name="ensamblado"></param>
        void OptimizarEnsamblado(System.Reflection.Assembly ensamblado);
    }

    public interface IXBRLContenedorInstanciasObjetos
    {
        /// <summary>
        /// Devuelve una colección de objetos que responden a las instancias correspondiente a un concepto
        /// </summary>
        /// <param name="nombreConceptoClase">Nombre del concepto</param>
        /// <returns><list type="object">Instancias de conceptos</list></returns>
        ICollection<object> ObtenerInstanciaObjetosPorConcepto(string nombreConceptoClase);
        /// <summary>
        /// Inserta al contenedor, una instancia de un objeto cuyo objeto sea un IConcepto
        /// </summary>
        /// <param name="objeto"></param>
        void InsertarObjeto(IConcepto objeto);
        /// <summary>
        /// Elimina todas las intancias del contenedor
        /// </summary>
        void BorrarTodasInstancias();
        /// <summary>
        /// Borra todas las instancias referente al conecpto especificado
        /// </summary>
        /// <param name="nombreConcepto">nombre del concepto</param>
        void BorrarInstancias(string nombreConcepto);
        /// <summary>
        /// Obtiene una colección de los conceptos instanciados e el contenedor
        /// </summary>
        ICollection<string> Conceptos { get;}
    }
}
