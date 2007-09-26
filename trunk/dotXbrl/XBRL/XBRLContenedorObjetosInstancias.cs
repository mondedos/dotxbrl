using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using dotXbrl.xbrlApi.XLink;
using dotXbrl.xbrlApi.Taxonomia;
using dotXbrl.GeneradorClases;

namespace dotXbrl.xbrlApi.XBRL
{
    public class XBRLContenedorObjetosInstancias : IXBRLContenedorInstanciasObjetos
    {
        #region Definicion Singleton

        private static IXBRLContenedorInstanciasObjetos _instance = null;

        private static object o = new object();

        private XBRLContenedorObjetosInstancias()
        {
            _map = new System.Collections.Hashtable();
        }

        public static IXBRLContenedorInstanciasObjetos ObtenerInstancia()
        {
            lock (o)
            {
                if (_instance == null)
                    _instance = new XBRLContenedorObjetosInstancias();
            }
            return _instance;
        }

        #endregion

        #region Definicion tipo

        private System.Collections.Hashtable _map;

        #endregion


        #region IXBRLContenedorInstanciasObjetos Members

        ICollection<object> IXBRLContenedorInstanciasObjetos.ObtenerInstanciaObjetosPorConcepto(string nombreConceptoClase)
        {
            List<object> res = null;
            if (!_map.Contains(nombreConceptoClase))
                return new List<object>();

            res = (List<object>)_map[nombreConceptoClase];

            return res;
        }

        void IXBRLContenedorInstanciasObjetos.InsertarObjeto(IConcepto objeto)
        {
            string tipo = objeto.GetType().Name;

            IXBRLContenedorInstanciasObjetos p = this;

            ICollection<object> col = p.ObtenerInstanciaObjetosPorConcepto(tipo);

            col.Add(objeto);

            if (_map.Contains(tipo))
            {
                _map[tipo] = col;
            }
            else
            {
                _map.Add(tipo, col);
            }
        }
        ICollection<string> IXBRLContenedorInstanciasObjetos.Conceptos
        {
            get
            {
                List<string> lista = new List<string>();
                foreach (object concepto in _map.Keys)
                {
                    lista.Add((string)concepto);
                }
                return lista;
            }
        }
        void IXBRLContenedorInstanciasObjetos.BorrarTodasInstancias()
        {
            _map.Clear();
        }
        void IXBRLContenedorInstanciasObjetos.BorrarInstancias(string nombreConcepto)
        {
            if (_map.ContainsKey(nombreConcepto))
            {
                _map.Remove(nombreConcepto);
            }
        }

        #endregion
    }
}