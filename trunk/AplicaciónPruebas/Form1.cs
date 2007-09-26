using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using dotXbrl.xbrlApi.XBRL;

namespace AplicaciónPruebas
{
    public partial class Form1 : Form
    {
        IXBLRProcesador _procesador;
        private string _nombreDocumento = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void salirToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pulsameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem menu = (ToolStripDropDownItem)sender;
            //creamos la ventana que contrendrá los datos
            ConceptoContenedorFormulario c =
            new ConceptoContenedorFormulario(_procesador, menu.Text);
            c.Show();
            c.MdiParent = this;
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            _nombreDocumento = this.toolStripTextBox1.Text;
            try
            {
                //leemos el documento
                _procesador = new XBRLProcesadorProveedor(new Uri(_nombreDocumento));
                //le decimos al componente que tenemos las clases generadas
                _procesador.OptimizarEnsamblado(System.Reflection.Assembly.GetExecutingAssembly());
                //procesamos el documento
                _procesador.Procesar();
                //obtenemos las instancias
                IXBRLContenedorInstanciasObjetos contenedor = _procesador.ContenedorInstanciasConceptos;
                this.conceptosMenu.DropDownItems.Clear();
                //obtenemos los conceptos existentes en las instancias
                foreach (string nombreConcepto in contenedor.Conceptos)
                {
                    //por cada concepto creamos un submenu
                    EventHandler manejadorEvento = new EventHandler(pulsameToolStripMenuItem_Click);
                    this.conceptosMenu.DropDownItems.Add(nombreConcepto, null, manejadorEvento);
                }
            }
            catch { }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                _nombreDocumento = this.toolStripTextBox1.Text;
                //leemos el documento
                _procesador = new XBRLProcesadorProveedor(new Uri(_nombreDocumento));
                //le decimos al componente que tenemos las clases generadas
                _procesador.OptimizarEnsamblado(System.Reflection.Assembly.GetExecutingAssembly());
                //procesamos el documento
                _procesador.Procesar();
                //obtenemos las instancias
                IXBRLContenedorInstanciasObjetos contenedor = _procesador.ContenedorInstanciasConceptos;

                this.conceptosMenu.DropDownItems.Clear();
                //obtenemos los conceptos existentes en las instancias
                foreach (string nombreConcepto in contenedor.Conceptos)
                {
                    //por cada concepto creamos un submenu
                    EventHandler manejadorEvento = new EventHandler(pulsameToolStripMenuItem_Click);
                    this.conceptosMenu.DropDownItems.Add(nombreConcepto, null, manejadorEvento);
                }
            }
            catch { }
        }
    }
}