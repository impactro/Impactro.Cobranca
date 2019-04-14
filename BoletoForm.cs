using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Impactro.Cobranca;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Impactro.WindowsControls
{
    /// <summary>
    /// Componente de Boleto para Windows Form.
    /// </summary>
    [ComVisible(false)]
    [ToolboxBitmap(typeof(BoletoForm))]
    public class BoletoForm : System.Windows.Forms.UserControl, IDisposable
    {
        private System.ComponentModel.Container components = null;


        /// <summary>
        /// Informa o Tipo de impressão, para ajuste de resolução
        /// </summary>
        public PrintTypes PrintType;

        /// <summary>
        /// Componente de Renderização do Boleto para Windows Control
        /// </summary>
        public BoletoForm()
        {
            try
            {
                // TODO em tempo de desenvolvimento achar o DPI correto
                InitializeComponent();
                Boleto = new Boleto();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Construtor já com a instancia do boleto
        /// </summary>
        public BoletoForm(Boleto oBoleto)
        {
            Boleto = oBoleto;
        }

        /// <summary>
        /// Retorna a instancia da classe Boleto
        /// </summary>
        public Boleto Boleto { get; private set; }

        /// <summary>
        /// Configura o boleto, passando os 3 objetos que contem os dados para a geração do boleto para variáveis internas, mas não calcula: veja: Calculaboleto()
        /// </summary>
        /// <param name="oCedente">Dados do Cedente</param>
        /// <param name="oSacado">Dados do Sacado</param>
        /// <param name="oBoleto">Dados do Boleto</param>
        public void MakeBoleto(CedenteInfo oCedente, SacadoInfo oSacado, BoletoInfo oBoleto)
        {
            if (Boleto == null)
                return;

            Boleto.MakeBoleto(oCedente, oSacado, oBoleto);
            Boleto.CalculaBoleto();
        }

        /// <summary>
        /// Calcula os dados do boleto previamente foncigurado
        /// </summary>
        public void CalculaBoleto()
        {
            Boleto.CalculaBoleto();
        }

        /// <summary>
        /// Elimina o objeto da memória
        /// </summary>
        /// <param name="disposing">Informa se sera eliminado da memória os componentes internos</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.BackColor = System.Drawing.Color.White;
            this.Name = "BoletoForm";
            this.Size = new System.Drawing.Size(640, 680);
        }

        #endregion

        /// <summary>
        /// Metodo de Renderização padrão
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            PrintType = PrintTypes.Image;
            try
            {
                Print(e.Graphics);
            }
            catch (Exception ex)
            {
                e.Graphics.DrawString(ex.Message + "\r\n" + ex.StackTrace, new Font("Arial", 12), Brushes.Black, new RectangleF(0, 0, 640, 670));
            }
        }

        /// <summary>
        /// Imprime o boleto em uma dispositivo grafico (imagem ou impressora)
        /// </summary>
        public bool Print(Graphics g)
        {
            // Seleciona as margens e escada de acordo com o tipo de documento
            if (PrintType == PrintTypes.Documet)
            {
                g.PageUnit = GraphicsUnit.Document;
                Boleto.Escala = 300d / 25.4d;
                // diciona uma margem de 2cm
                g.TranslateTransform((float)(20 * Boleto.Escala), (float)(20 * Boleto.Escala)); // + Boleto.CedenteLogoHeight - 9));
            }
            else
            {
                g.PageUnit = GraphicsUnit.Pixel;
                g.Clear(Color.White);
                //Boleto.Escala = 640 / 170d;
            }

            Boleto.Render(g);

            return true;
        }
    }
}