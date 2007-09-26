using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace dotXbrl.xbrlApi.XLink
{

    #region Interfaz
    public interface IXLinkHandler
    {
        void endLocator(
            string namespaceURI,
            string sName,
            string qName
            );
        void startArc(
                    string namespaceURI,
                    string lName,
                    string qName,
                    XmlAttributeCollection attrs,
                    string from,
                    string to,
                    string arcrole,
                    string title,
                    string show,
                    string actuate
            );

        void endArc(
                string namespaceURI,
                string sName,
                string qName
                );

        void xmlBaseStart(string value);

        void xmlBaseEnd();

        void error(string namespaceURI, string lName, string qName,
               XmlAttributeCollection attrs, string message);

        void warning(string namespaceURI, string lName, string qName,
               XmlAttributeCollection attrs, string message);

        void startLocator(
            string namespaceURI,
            string lName,
            string qName,
            XmlAttributeCollection attrs,
            string href,
            string role,
            string title,
            string label
            );
        void endResource(
            string namespaceURI,
            string sName,
            string qName
            );
        void startResource(
            string namespaceURI,
            string lName,
            string qName,
            XmlAttributeCollection attrs,
            string role,
            string title,
            string label
        );
        void endExtendedLink(
            string namespaceURI,
            string sName,
            string qName
            );
        void startExtendedLink(
            string namespaceURI,
            string lName,
            string qName,
            XmlAttributeCollection attrs,
            string role,
            string title
        );
        void titleCharacters(char[] buf, int offset, int len);
        void endTitle(
            string namespaceURI,
            string sName,
            string qName
            );
        void startTitle(
            string namespaceURI,
            string lName,
            string qName,
            XmlAttributeCollection attrs
            );
        void endSimpleLink(
                    string namespaceURI,
                    string sName,
                    string qName
                    );
        /// <summary>
        /// Procesa los enlaces simples
        /// </summary>
        /// <param name="namespaceURI"></param>
        /// <param name="lName"></param>
        /// <param name="qName"></param>
        /// <param name="attrs"></param>
        /// <param name="href"></param>
        /// <param name="role"></param>
        /// <param name="arcrole"></param>
        /// <param name="title"></param>
        /// <param name="show"></param>
        /// <param name="actuate"></param>
        void startSimpleLink(
            string namespaceURI,
            string lName,
            string qName,
            XmlAttributeCollection attrs,
            string href,
            string role,
            string arcrole,
            string title,
            string show,
            string actuate);
    }

    #endregion

    #region Implementacion

    public class XlinkHandlerProvider : IXLinkHandler
    {

        public XlinkHandlerProvider() { }

        #region IXLinkHandler Members

        void IXLinkHandler.endLocator(string namespaceURI, string sName, string qName)
        {
        }

        void IXLinkHandler.startArc(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs, string from, string to, string arcrole, string title, string show, string actuate)
        {
        }

        void IXLinkHandler.endArc(string namespaceURI, string sName, string qName)
        {
        }

        void IXLinkHandler.xmlBaseStart(string value)
        {
        }

        void IXLinkHandler.xmlBaseEnd()
        {
        }

        void IXLinkHandler.error(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs, string message)
        {
        }

        void IXLinkHandler.warning(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs, string message)
        {
        }

        void IXLinkHandler.startLocator(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs, string href, string role, string title, string label)
        {
        }

        void IXLinkHandler.endResource(string namespaceURI, string sName, string qName)
        {
        }

        void IXLinkHandler.startResource(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs, string role, string title, string label)
        {
        }

        void IXLinkHandler.endExtendedLink(string namespaceURI, string sName, string qName)
        {
        }

        void IXLinkHandler.startExtendedLink(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs, string role, string title)
        {
        }

        void IXLinkHandler.titleCharacters(char[] buf, int offset, int len)
        {
        }

        void IXLinkHandler.endTitle(string namespaceURI, string sName, string qName)
        {
        }

        void IXLinkHandler.startTitle(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs)
        {
        }

        void IXLinkHandler.endSimpleLink(string namespaceURI, string sName, string qName)
        {
        }

        void IXLinkHandler.startSimpleLink(string namespaceURI, string lName, string qName, XmlAttributeCollection attrs, string href, string role, string arcrole, string title, string show, string actuate)
        {
        }

        #endregion
    }

    #endregion
}
