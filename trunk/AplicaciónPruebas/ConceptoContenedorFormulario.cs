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
    public partial class ConceptoContenedorFormulario : Form
    {
        private string _nombreConcepto = "";
        private IXBRLContenedorInstanciasObjetos _contenedorInstancias;

        public ConceptoContenedorFormulario(IXBLRProcesador procesador, string NombreConcepto)
        {
            _contenedorInstancias = procesador.ContenedorInstanciasConceptos;
            _nombreConcepto = NombreConcepto;
            InitializeComponent();
            inicializar();
        }

        private void inicializar(){
            //establecer el titulo de la aplicacion
            this.Text = _nombreConcepto;

            ICollection<object> col = _contenedorInstancias.ObtenerInstanciaObjetosPorConcepto(_nombreConcepto);

            dataGridView1.DataSource = col;
                dataGridView1.AutoSizeRowsMode =
            DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

        }
    }
}