using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace dotXbrl.xbrlApi.XLink
{
    public class Validator : XlinkHandlerProvider
    {
        #region Definicion del tipo

        private IXLinkProcesor _xlinkProcessor;
        private XmlDocument _document;

        public XmlDocument Result
        {
            get
            {
                return _document;
            }
        }

        #endregion

        public Validator(Uri url)
            : base()
        {
            _document = new XmlDocument();

            //leemos el xml

            _document.Load(url.OriginalString);
        }


        #region Metodos

        public bool Validate()
        {
            IXLinkHandler manejador = new XlinkHandlerProvider();

            _xlinkProcessor = new XLinkProcesorProvider();

            _xlinkProcessor.Document = _document;

            _xlinkProcessor.Procesar();

            return false;
        }
        private void ValidationCallBack(object sender, System.Xml.Schema.ValidationEventArgs args)
        {
            // The xml does not match the schema.
            Console.WriteLine("chungo pastel");

        }

        #endregion

        #region Metodos auxiliares



        #endregion
    }
}
