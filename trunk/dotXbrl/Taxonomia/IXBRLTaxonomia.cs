using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml.Schema;

namespace dotXbrl.xbrlApi.Taxonomia
{
    public interface IXBRLDataTaxonomySet
    {
        /// <summary>
        /// Procesa una taxonomía entrayendo sus items y tuples
        /// </summary>
        /// <param name="url">URI</param>
        bool Insertar(Uri url);
        /// <summary>
        /// Procesa una taxonomía entrayendo sus items y tuples
        /// </summary>
        /// <param name="url"></param>
        bool Insertar(string url);
        /// <summary>
        /// Procesa el DTS
        /// </summary>
        void Procesar();
        void MapearConceptosEnCodigo();
        void MapearConceptosEnCodigo(string nombreDirectorio);
        /// <summary>
        /// Conjunto de esquemas que forman las taxonomias
        /// </summary>
        System.Xml.Schema.XmlSchemaSet Esquemas { get;}
        /// <summary>
        /// Esta funcion se utiliza si tenemos las clases mapeadas como fichero .cs
        /// 
        /// Con esto se consigue, que cada vez que procesemos un fichero, no compilar las clases de nuevo
        /// </summary>
        /// <param name="ensambladoCliente"></param>
        void OptimizarEnsamblado(System.Reflection.Assembly ensambladoCliente);
        string Prefijo { get;}
        /// <summary>
        /// Conceptos items extraidos del DTS
        /// </summary>
        Hashtable Items { get;}
        /// <summary>
        /// Conceptos Tuplas extraidos del DTS
        /// </summary>
        ICollection<IXBRLTupleDef> Tuplas { get;}

    }

    public interface IXBRLItemDef
    {
        string Nombre { get;set;}
        string Id { get;set;}
        string EspacioNombre { set;get;}
        string Prefijo { set;get;}
        Type Tipo { get;set;}
        string SemanticaTipo { get;set;}
        Balance BalanceAsociado { get;set;}
        XmlSchemaElement ElementoItem { get;}
        Period Periodo { get;set;}
    }

    public interface IXBRLTupleDef
    {
        string Nombre { get;set;}
        string Id { set;get;}
        string Prefijo { set;get;}
        XmlSchemaElement ElementoTupla { get;}
        ICollection<IXBRLItemDef> Celdas { get;}
    }

    public enum Period
    {
        Instant,
        Duration
    }

    public enum Balance
    {
        Credit,
        Debit
    }
}
