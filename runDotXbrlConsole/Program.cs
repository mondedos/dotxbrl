using System;
using System.Collections.Generic;
using System.Text;
using dotXbrl.xbrlApi.XLink;
using dotXbrl.xbrlApi.XBRL;

using System.IO;

namespace runDotXbrlConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28-presentation.xml";

            //Validator validador = new Validator(new Uri("http://www.xbrl.org/us/fr/gaap/ci/2005-02-28/us-gaap-ci-2005-02-28-presentation.xml"));

            //validador.Validate();


            IXBLRProcesador procesador = new XBRLProcesadorProveedor(new Uri("http://www.bde.es/cenbal/taxonomia/es-be-cb-2006-04-30/Informes/CBAN-Informes/03-Perdidas.xbrl"));

            //IXBLRProcesador procesador = new XBRLProcesadorProveedor(new Uri("http://www.bapepam.go.id/pasar_modal/publikasi_pm/info_pm/xbrl/xbrl/icm-instance-1.xbrl"));

            //IXBLRProcesador procesador = new XBRLProcesadorProveedor(new Uri("http://about.reuters.com/investors/results/archive/documents/XBRL_2006_Preliminary_Results/IFS-Reuters-2006-12-31.xbrl"));
            //procesador.OptimizarEnsamblado(System.Reflection.Assembly.GetExecutingAssembly());
            //procesador.Procesar();
            procesador.MapearAObjetos("");
            //reflexion();

            //GeneradorClases cg = new GeneradorClases();

            //cg.generarClase();
            /*
            IXBRLContenedorInstanciasObjetos contenedor = procesador.ContenedorInstanciasConceptos;

            contenedor.BorrarTodasInstancias();

            dotXbrl.AccruedVacation vacation = new dotXbrl.AccruedVacation();

            vacation.Valor = 400;

            contenedor.InsertarObjeto(vacation);
            //procesador.ContenedorInstanciasConceptos.ObtenerInstanciaObjetosPorConcepto("");
            //procesador.ContenedorInstanciasConceptos.InsertarObjeto(new object());
            procesador.GuardarDocumento(new Uri("c:\\instancia.xml"));

            //Console.ReadLine();


            */

        }


    }
}
