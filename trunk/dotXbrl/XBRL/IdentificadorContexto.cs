using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace dotXbrl.xbrlApi.XBRL
{
    public class Contexto : IContexto
    {
        #region Definicion del tipo

        private string _id;
        private IIdentificadorContexto _identificador;
        private IPeriodo _periodo;
        private ICollection<XmlElement> _segmentos;

        #endregion

        public Contexto()
        {
            _identificador = new IdentificadorContexto();
            _segmentos = new List<XmlElement>();
        }

        #region IContexto Members

        string IContexto.Id
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

        IIdentificadorContexto IContexto.Identificador
        {
            get
            {
                return _identificador;
            }
            set
            {
                _identificador = value;
            }
        }

        #endregion

        #region IContexto Members


        IPeriodo IContexto.Periodo
        {
            get
            {
                return _periodo;
            }
            set
            {
                _periodo=value;
            }
        }

        #endregion

        #region IContexto Members


        ICollection<XmlElement> IContexto.Segmentos
        {
            get { return _segmentos; }
        }

        #endregion
    }
    public class IdentificadorContexto : IIdentificadorContexto
    {
        #region Definicion del tipo

        private string _uri, _descripcion;
        private ICollection<XmlElement> _elementosSegmento;

        #endregion

        public IdentificadorContexto()
        {
            _elementosSegmento = new List<XmlElement>();
        }

        #region IIdentificadorContexto Members

        string IIdentificadorContexto.URI
        {
            get
            {
                return _uri;
            }
            set
            {
                _uri = value;
            }
        }

        string IIdentificadorContexto.Descripcion
        {
            get
            {
                return _descripcion;
            }
            set
            {
                _descripcion = value;
            }
        }

        ICollection<XmlElement> IIdentificadorContexto.Segmento
        {
            get { return _elementosSegmento; }
        }

        #endregion
    }

    public class PeriodoInicioFin : IPeriodoInicioFin
    {
        DateTime _inicio, _fin;

        public PeriodoInicioFin(DateTime inicio, DateTime fin)
        {
            _inicio = inicio;
            _fin = fin;
        }
        public PeriodoInicioFin(string inicio, string fin)
        {
            _inicio = DateTime.Parse(inicio);
            _fin = DateTime.Parse(fin);
        }

        #region IPeriodoInicioFin Members

        DateTime IPeriodoInicioFin.Inicio
        {
            get { return _inicio; }
        }

        DateTime IPeriodoInicioFin.Fin
        {
            get { return _fin; }
        }

        #endregion
    }
    public class PeriodoInstante : IPeriodoInstante
    {
        private DateTime _fecha;

        public PeriodoInstante(DateTime fecha)
        {
            _fecha = fecha;
        }

        public PeriodoInstante(string fecha)
        {
            _fecha = DateTime.Parse(fecha);
        }

        #region IPeriodoInstante Members

        DateTime IPeriodoInstante.Fecha
        {
            get { return _fecha; }
        }

        #endregion
    }
}
