using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace dotXbrl.xbrlApi.XBRL
{
    /// <summary>
    /// Interface que define un concepto, se abien tupla o item
    /// </summary>
    public interface IConcepto
    {
        /// <summary>
        /// Devuelve el contexto en el que tiene sentido el concepto
        /// </summary>
        /// <returns></returns>
        IContexto getContexto();
        /// <summary>
        /// Establece un contexto al concepto actual
        /// </summary>
        /// <param name="contexto"></param>
        void setContexto(IContexto contexto);
    }
    public interface IContexto
    {
        string Id { get;set;}
        IIdentificadorContexto Identificador { get;set;}
        IPeriodo Periodo { get;set;}
        ICollection<XmlElement> Segmentos { get;}
    }
    public interface IPeriodo
    {
    }
    public interface IPeriodoInicioFin : IPeriodo
    {
        DateTime Inicio { get;}
        DateTime Fin { get;}
    }
    public interface IPeriodoInstante : IPeriodo
    {
        DateTime Fecha { get;}
    }
    public interface IIdentificadorContexto
    {
        string URI { get;set;}
        string Descripcion { get;set;}
        ICollection<XmlElement> Segmento { get;}
    }
}
