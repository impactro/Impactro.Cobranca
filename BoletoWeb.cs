using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Drawing;
using Impactro.Cobranca;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Text;

namespace Impactro.WebControls
{
    /// <summary>
    /// Compronente de Boleto para ASP.Net.
    /// </summary>
    [ComVisible(false)]
    [ToolboxData("<{0}:BoletoWeb runat=server></{0}:BoletoWeb>")]
    [ToolboxBitmap(typeof(BoletoWeb))]
    public class BoletoWeb : System.Web.UI.WebControls.WebControl
    {

        /// <summary>
        /// Use este vento quando desejar customizar a geração do campo livre
        /// </summary>
        public event BoletoMontaCampoLivre MontaCampoLivre;

        /// <summary>
        /// Define o metodo para a configuração da Table HTML onde serão inserido as celulas(campos)
        /// </summary>
        public event ConfigureTable ConfigureTable;
        /// <summary>
        /// Define se deve aparecer a área de recibo do Sacado
        /// (por padrão é VERDADEIRO), mude para "False" para criar um "Topo" customizado
        /// </summary>
        
        private Boleto blt;
        private String imagePath = "Imagens/";
        private String imageLogo = "";
        private String imageCorte = "";
        private String cssCell = "";
        private String cssField = "";
        private String _BarCod = "";
        private Color _CellEspecialColor = Color.LightGray;
        private int _CellEspecialSize = 0;
        private String _TextoDataProcessamento = "Data Processamento";
#if ASP11
        private BoletoImageType imgType=BoletoImageType.gif;
#else
		private BoletoImageType imgType=BoletoImageType.embebed;
#endif
        private bool imgCodBar = true;
        private static string cRenderImage = null;
        /// <summary>
        /// Cria uma nova instancia do WebControl do Boleto
        /// </summary>
        public BoletoWeb()
        {
            try
            {
                blt = new Boleto();

                // Lê do web.config a opção padrão de renderização HTML/IMAGEM
                // <add key = "BoletoRenderImage" value = "true" />
                if (cRenderImage == null)
                    RenderCountImage = CobUtil.GetInt(System.Configuration.ConfigurationManager.AppSettings["BoletoRenderImage"]);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Permite configurar uma função para o evento de geração de código de barras personalizado.
        /// Util para a geração de boletos para bancos ou carteiras originalmente não implementados.
        /// </summary>
        /// <param name="func"></param>
        public void SetBoletoMontaCampoLivre(BoletoMontaCampoLivre func)
        {
            if (blt != null)
                blt.onMontaCampoLivre += func;
        }

        /// <summary>
        /// Configura o boleto e calcula o código de barras e linha digitavel com base nas informações passadas
        /// </summary>
        /// <param name="Cedente">São as dados de que Emite o Boleto (Quem irá receber)</param>
        /// <param name="Sacado">São os dados do pagador do Boleto (Quem irá pagar)</param>
        /// <param name="Boleto">São os dados da caobrança em si: Valores, Datas, Descrições e Informações</param>
        public void MakeBoleto(CedenteInfo Cedente, SacadoInfo Sacado, BoletoInfo Boleto)
        {
           
            if (MontaCampoLivre != null)
                blt.onMontaCampoLivre += MontaCampoLivre;
            
            if (blt != null)
                blt.MakeBoleto(Cedente, Sacado, Boleto);

            blt.CalculaBoleto();
        }


        #region "Encapsulamentos"

		/// <summary>
		/// Contem o caminho do diretório de imagens
		/// </summary>
        [Bindable(true), Category("Campos"), DefaultValue("")]
        public string ImagePath
        {
            get
            {
                if (imagePath == null)
                    imagePath = "";
                return imagePath;
            }
            set { imagePath = value; }
        }

        /// <summary>
        /// Define o tipo de imagem a ser usado
        /// </summary>
        [Bindable(true), Category("Campos"), DefaultValue("embebed")]
        public BoletoImageType ImageType
        {
            get{ return imgType; }
            set { imgType = value; }
        }

        [Bindable(true), Category("Campos"), DefaultValue("true")]
        public bool ImageCodBar
        {
            get { return imgCodBar; }
            set { imgCodBar = value; }
        }

        [Bindable(true), Category("Campos"), DefaultValue("")]
        public string ImageLogo
        {
            get
            {
                if (imageLogo == null)
                    imageLogo = "";
                return imageLogo;
            }
            set { imageLogo = value; }
        }


		/// <summary>
		/// configura a cor ara as celulas especiais (Especie, Vencimento e Vvalor)
		/// </summary>
        [Bindable(true), Category("Campos"), DefaultValue("LightGray")]
        public Color EspecialColor
        {
            get { return _CellEspecialColor; }
            set { _CellEspecialColor = value; }
        }

        [Bindable(true), Category("Campos"), DefaultValue("")]
        public int EspecialSize
        {
            get { return _CellEspecialSize; }
            set { _CellEspecialSize = value; }
        }

		/// <summary>
		/// configura o texto do campo data de processamento, pois em algumas casos o texto padrão não cabe
		/// </summary>
        [Bindable(true), Category("Campos"), DefaultValue("Data Processamento")]
        public String TextoDataProcessamento
        {
            get { return _TextoDataProcessamento; }
            set { _TextoDataProcessamento = value; }
        }

		/// <summary>
		/// Define o caminho da imagem que será usada para separar as vias do boletos
		/// </summary>
        [Bindable(true), Category("Campos"), DefaultValue("")]
        public string ImageCorte { set { imageCorte = value; } get { return imageCorte; } }

		/// <summary>
		/// Define a classe CSS 'class' dos titulos dos campos do boleto
		/// </summary>
        [Bindable(true), Category("Campos"), DefaultValue("")]
        public string CssCell { set { cssCell = value; } get { return cssCell; } }

		/// <summary>
		/// Define a classe CSS 'class' dos valores dos campos do boleto
		/// </summary>
        [Bindable(true), Category("Campos"), DefaultValue("")]
        public string CssField { set { cssField = value; } get { return cssField; } }

        /// <summary>
        /// Define se o boleto deve ser renderizado como imagem em vez de HTML
        /// </summary>
        [Bindable(true), Category("Campos"), DefaultValue(0)]
        public int RenderCountImage { set; get; }

        [Bindable(true), Category("Campos"), DefaultValue("")]
        public string ImageCodBarUrl { set { _BarCod = value; } get { return _BarCod; } }

        //[Bindable(true), Category("Campos"), DefaultValue("false"), Obsolete("Use o nome ExibeReciboLinhaDigitavel")]
        //public bool ExibeReciboIPTE { set { blt.ExibeReciboIPTE = value; } get { return blt.ExibeReciboIPTE; } }

        [Bindable(true), Category("Campos"), DefaultValue("false")]
        public bool ExibeReciboLinhaDigitavel { set { blt.ExibeReciboLinhaDigitavel = value; } get { return blt.ExibeReciboLinhaDigitavel; } }

        [Bindable(true), Category("Campos"), DefaultValue("true")]
        public bool ExibeReciboSacado { set { blt.ExibeReciboSacado = value; } get { return blt.ExibeReciboSacado; } }

        [Bindable(true), Category("Campos"), DefaultValue("true")]
        public bool ExibeEnderecoReciboSacado { set { blt.ExibeEnderecoReciboSacado = value; } get { return blt.ExibeEnderecoReciboSacado; } }

        /// <summary>
		/// Instancia da classe BOLETO que processa as informações
		/// A classe BoletoWeb apenas deseha o boleto previamente processado em HTML
		/// </summary>
        public Boleto Boleto { get { return blt; } }

        #endregion

        protected override void Render(HtmlTextWriter output)
        {
            if (RenderCountImage > 0)
            {
                output.Write("<center><div style='width: 650px;'>");

                Bitmap img = Boleto.ImageBoleto();
                Graphics g;
                if (RenderCountImage == 1)
                {
                    output.Write(CobUtil.ToBase64ImageTag(img, ImageFormat.Png));
                }
                else
                {
                    Bitmap img1 = new Bitmap(img.Width / 2, img.Height);
                    g = Graphics.FromImage(img1);
                    g.DrawImage(img, 0, 0);
                    output.Write(CobUtil.ToBase64ImageTag(img1, ImageFormat.Png));

                    Bitmap img2 = new Bitmap(img.Width / 2, img.Height);
                    g = Graphics.FromImage(img2);
                    g.DrawImage(img, 0, 0, new Rectangle(img.Width / 2, 0, img.Width / 2, img.Height), GraphicsUnit.Pixel);
                    output.Write(CobUtil.ToBase64ImageTag(img2, ImageFormat.Png));
                }

                output.Write("</div></center>");
                return;
            }

            if (this.CssField == null)
                this.CssField = "";
            if (this.CssCell == null)
                this.CssCell = "";

            // blt.CalculaBoleto();

            string cLinhaDigitavel = blt.LinhaDigitavel;
            string cBarras = CobUtil.BarCode(blt.CodigoBarras);

            // compatibilidade XHTML
            output.WriteLine("<div class='BoletoWeb'>");

            TableRow row;
            TableCell cell;

            #region "Linha digitavel"

            Table tbLinha = new Table();
            tbLinha.CellPadding = 0;
            tbLinha.CellSpacing = 0;
            tbLinha.Width = new Unit("640");

            row = new TableRow();

            cell = new TableCell();
            if( imageLogo=="" )
                cell.Text = String.Format("<img src='{0}' style='width:149px;height:38px;margin:1px'/>", GetImage(ImageGetType.Banco));
            else
                cell.Text = String.Format("<img src='{0}{1}' class='BoletoWebLogo' />", this.ImagePath, this.imageLogo );
            cell.ColumnSpan = 3;
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.Text = String.Format("<img src='{0}' width='2' height='30' align='right' />", GetImage(ImageGetType.p));
            cell.VerticalAlign = VerticalAlign.Bottom;
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.HorizontalAlign = HorizontalAlign.Center;
            cell.VerticalAlign = VerticalAlign.Bottom;
            cell.Style.Add("padding-bottom", "5px;");
            cell.Style.Add("font-size", "7pt");
            cell.Style.Add("font-family", "Verdana,Arial"); 
            cell.Text = "Banco<br/><font style='font-size: 11pt; font-weight: bold; font-family: Arial;'>" + blt.BancoCodigo + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.VerticalAlign = VerticalAlign.Bottom;
            cell.Text = String.Format("<img src='{0}' width='2' height='30' />", GetImage(ImageGetType.p));
            cell.Width = new Unit("2px");
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = this.CssField;
            cell.HorizontalAlign = HorizontalAlign.Right;
            cell.VerticalAlign = VerticalAlign.Bottom;
            cell.Style.Add("padding-bottom", "5px;");
            cell.Wrap = false;
            cell.Width = new Unit("400");
            cell.ColumnSpan = 8;
            row.Cells.Add(cell);

            tbLinha.Rows.Add(row);

            if( blt.ExibeReciboLinhaDigitavel )
                tbLinha.Rows[0].Cells[4].Text = BoletoTextos.Recibo + "<br/><font style='font-size: 11pt; font-weight: bold; font-family: Arial;'>" + cLinhaDigitavel + "</font>"; 
            else
                tbLinha.Rows[0].Cells[4].Text = BoletoTextos.Recibo;

            #endregion

            // Recibo do Sacado
            #region "Boleto parte 1"

            if (blt.ExibeReciboSacado)
            {
                tbLinha.RenderControl(output);

                Table tbBol1 = new Table();
                tbBol1.Width = new Unit("640");
                if (ConfigureTable == null)
                {
                    tbBol1.BorderWidth = new Unit("1");
                    tbBol1.BorderStyle = BorderStyle.Solid;
                    tbBol1.GridLines = GridLines.Both;
                    tbBol1.BorderColor = Color.Black;
                    tbBol1.CellPadding = 1;
                    tbBol1.CellSpacing = 0;
                }
                else
                    ConfigureTable(tbBol1);
#if NET2 || NET4
                tbBol1.Attributes.Add("bordercolordark", "#000000");
                tbBol1.Attributes.Add("bordercolorlight", "#000000");
#endif
                // Linha 1

                row = new TableRow();

                cell = new TableCell();
                cell.ColumnSpan = 4;
                cell.Width = new Unit("350"); ;
                cell.CssClass = CssCell;
                cell.Text = BoletoTextos.Cedente + ":<br/>" +
                    "<font class=" + CssField + ">&nbsp;" +
                    blt.Cedente +
                    (" - " + blt.CedenteDocumento) +
                    ("</font><br/><font class=" + CssCell + ">Endereço: </font><font class=" + CssField + ">" + blt.CedenteEndereco) + "</font>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.Width = new Unit("160"); ;
                cell.Wrap = false;
                cell.CssClass = CssCell;
                cell.Text = BoletoTextos.CedenteConta + "<br/>" + "<div align=center class=" + CssField + ">&nbsp;" +
                     blt.AgenciaConta + "</div>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.Width = new Unit("130");
                cell.CssClass = CssCell;
                cell.BackColor = _CellEspecialColor;
                cell.Text = "Vencimento<br>" + "<div align='right' class='" + CssField + "'";
                if (_CellEspecialSize > 0)
                    cell.Text += "style='font-size:" + _CellEspecialSize.ToString() + "pt;'";

                cell.Text += string.Format(">{0:dd/MM/yyyy}", blt.DataVencimento) + "</div>";
                row.Cells.Add(cell);

                tbBol1.Rows.Add(row);

                // Linha 2

                row = new TableRow();

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.ColumnSpan = 4;
                if(ExibeEnderecoReciboSacado)
                    cell.Text = BoletoTextos.Sacado + "<br/>" + "<font class=" + CssField + ">" +
                        blt.SacadoCOD + (blt.SacadoCOD == "" ? "" : ": ") + blt.Sacado + " " + blt.SacadoDocumento + "<br>" +
                        blt.SacadoEndereco + "<br>" +
                        blt.Bairro + " - " + blt.Cidade + "<br>" +
                        "CEP: " + blt.Cep + " - " + blt.UF + "</font>";
                else
                    cell.Text = BoletoTextos.Sacado + "<br/>" + "<font class=" + CssField + ">&nbsp;" +
                        blt.Sacado + "</font>";
                
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Text = "Nº Documento<br/>" + "<div align=center class=" + CssField + ">" +
                    blt.NumeroDocumento + "</div>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Wrap = false;
                cell.Text = "Nosso Número<br/>" + "<div align=right class=" + CssField + ">" +
                    blt.NossoNumeroExibicao + "</div>";
                row.Cells.Add(cell);

                tbBol1.Rows.Add(row);

                // Linha 3

                row = new TableRow();

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.BackColor = _CellEspecialColor;
                cell.Text = "Espécie Moeda<br>" + "<font class=" + CssField + ">&nbsp;" +
                    blt.MoedaEspecie + "</font>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Text = "Parcela<br>" + "<div align=center class=" + CssField + ">&nbsp;" +
                    ((blt.ParcelaNumero == 0) ? "" : blt.ParcelaNumero.ToString()+
                     ((blt.ParcelaTotal == 0) ? "" : " / " + blt.ParcelaTotal.ToString())) + "</div>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Text = "Qtde Moeda<br>" + "<div align=center class=" + CssField + ">&nbsp;" +
                    ((blt.Quantidade == 0) ? "" : blt.Quantidade.ToString()) + "</div>";
                row.Cells.Add(cell); 
                
                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Text = "(x)Valor<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                    ((blt.ValorUnitario == 0) ? "" : String.Format("{0:C}", blt.ValorUnitario)) + "</div>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Text = "(-)Descontos/Abatim.<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                    ((blt.ValorDesconto == 0) ? "" : String.Format("{0:C}", blt.ValorDesconto)) + "</div>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.BackColor = _CellEspecialColor;
                cell.Text = "(=)Valor Documento<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                    String.Format("{0:C}", blt.ValorDocumento) + "</div>";
                row.Cells.Add(cell);

                tbBol1.Rows.Add(row);

                // Linha 5

                row = new TableRow();

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.VerticalAlign = VerticalAlign.Bottom;
                cell.ColumnSpan = 4;
                cell.Text = "Demonstrativo";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Text = "(+)Outros Acréscimos<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                    ((blt.ValorAcrescimo == 0) ? "" : String.Format("{0:C}", blt.ValorAcrescimo)) + "</div>";
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.Text = "(=)Valor Cobrado<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                    ((blt.ValorCobrado == 0) ? "" : String.Format("{0:C}", blt.ValorCobrado)) + "</div>";
                row.Cells.Add(cell);

                tbBol1.Rows.Add(row);

                // Linha 6

                row = new TableRow();

                cell = new TableCell();
                cell.CssClass = CssCell;
                cell.VerticalAlign = VerticalAlign.Top;
                if (string.IsNullOrEmpty(blt.Demonstrativo))
                    cell.Height = new Unit("20");
                if (string.IsNullOrEmpty(blt.Demonstrativo))
                    cell.Height = new Unit("50");
                cell.ColumnSpan = 6;
                if (blt.Demonstrativo != null)
                    cell.Text = "<div class=" + CssField + ">" + blt.Demonstrativo.Replace("\r\n", "<br/>") + "</div>";
                row.Cells.Add(cell);

                tbBol1.Rows.Add(row);

                if (!string.IsNullOrEmpty(blt.Informacoes))
                {
                    row = new TableRow();

                    cell = new TableCell();
                    cell.CssClass = CssCell;
                    cell.VerticalAlign = VerticalAlign.Top;
                    cell.Height = new Unit("50");
                    cell.ColumnSpan = 6;
                    if (blt.Demonstrativo != null)
                        cell.Text = "<div class=" + CssField + " align='center'>" + blt.Informacoes.Replace("\r\n", "<br>") + "</div>";
                    row.Cells.Add(cell);

                    tbBol1.Rows.Add(row);
                }
                
                tbBol1.RenderControl(output);

                if (this.ImageCorte != "")
                    output.Write("<img src='" + this.ImagePath + this.ImageCorte + "' class='BoletoWebCorte'>");
                else
                    output.Write("<img src='" + GetImage(ImageGetType.corte) + "' class='BoletoWebCorte'>");

                output.Write("<br><br>");
            }

            #endregion
            
            tbLinha.Rows[0].Cells[0].Text = String.Format("<img src='{0}' style='width:149px;height:38px;margin:1px'/>", GetImage(ImageGetType.Banco));
            tbLinha.Rows[0].Cells[4].Text = "<font style='font-size: 11pt; font-weight: bold; font-family: Arial;'>" + cLinhaDigitavel + "</font>";
            tbLinha.RenderControl(output);

            // Boleto Padrão IPTE

            #region "Boleto Parte 2"
            
            Table tbBol2 = new Table();
            tbBol2.Width = new Unit("640");
            if (ConfigureTable == null)
            {
                tbBol2.BorderWidth = new Unit("1");
                tbBol2.BorderStyle = BorderStyle.Solid;
                tbBol2.GridLines = GridLines.Both;
                tbBol2.BorderColor = Color.Black;
#if NET2 || NET4
                tbBol2.Attributes.Add("bordercolordark", "#000000");
                tbBol2.Attributes.Add("bordercolorlight", "#000000");
#endif
                tbBol2.CellPadding = 1;
                tbBol2.CellSpacing = 0;
            }
            else
                ConfigureTable(tbBol2);

            // Linha 1
            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Width = new Unit("480");
            cell.ColumnSpan = 6;
            cell.Text = "Local de pagamento<br>" + "<font class=" + CssField + ">&nbsp;" +
                blt.LocalPagamento + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Width = new Unit("130");
            cell.BackColor = _CellEspecialColor;
            cell.Text = "Vencimento<br>" + "<div align='right' class='" + CssField + "'";
            if (_CellEspecialSize > 0)
                cell.Text += "style='font-size:" + _CellEspecialSize.ToString() + "pt;'";

            cell.Text += string.Format(">{0:dd/MM/yyyy}", blt.DataVencimento) + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 2
            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.ColumnSpan = 6;
            cell.Text = BoletoTextos.Cedente + ":<br/>" +
                "<font class=" + CssField + ">&nbsp;" +
                blt.Cedente +
                (" - " + blt.CedenteDocumento) +
                ("</font><br/><font class=" + CssCell + ">Endereço: </font><font class=" + CssField + ">" + blt.CedenteEndereco) + "</font>"; 
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Wrap = false;
            cell.Text = BoletoTextos.CedenteConta + "<br/>" + "<div align=right class=" + CssField + ">" +
                blt.AgenciaConta + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 3

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Width = new Unit("120");
            cell.Text = "Data Documento<br>" + "<font class=" + CssField + ">&nbsp;" +
                string.Format("{0:dd/MM/yyyy}", blt.DataDocumento) + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Width = new Unit("120");
            cell.ColumnSpan = 2;
            cell.Text = "Nº Documento<br>" + "<font class=" + CssField + ">&nbsp;" +
                blt.NumeroDocumento + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = BoletoTextos.EspecieDoc + "<br/>" + "<font class=" + CssField + ">&nbsp;" +
                Boleto.Especie + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "Aceite<br>" + "<font class=" + CssField + ">&nbsp;" +
                Boleto.Aceite + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Width = new Unit("110");
            cell.Text = _TextoDataProcessamento + "<br>" + "<font class=" + CssField + ">&nbsp;" +
                string.Format("{0:dd/MM/yyyy}", blt.DataProcessamento) + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Wrap = false;
            cell.Text = "Nosso Número<br>" + "<div align=right class=" + CssField + ">" +
                blt.NossoNumeroExibicao + "</div>";
            row.Cells.Add(cell);
            tbBol2.Rows.Add(row);

            // Linha 4

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            if(blt.CIP != "") // quando houve o campo
            {
                cell.Text = "Uso do Banco<br>" + "<font class=" + CssField + ">&nbsp;" +
                blt.UsoBanco + " CIP: " + blt.CIP + "</font>";
                row.Cells.Add(cell);
                cell = new TableCell();
            }
            else
            {
                cell = new TableCell();
                cell.ColumnSpan = 2;
            }
            cell.CssClass = CssCell;
            cell.Text = "Carteira<br>" + "<font class=" + CssField + ">&nbsp;" +
                blt.CarteiraExibicao + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.BackColor = _CellEspecialColor;
            cell.Text = "Espécie<br>" + "<font class=" + CssField + ">&nbsp;" +
                blt.MoedaEspecie + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "Parcela<br>" + "<div align=center class=" + CssField + ">&nbsp;" +
                ((blt.ParcelaNumero == 0) ? "" : blt.ParcelaNumero.ToString() +
                 ((blt.ParcelaTotal == 0) ? "" : " / " + blt.ParcelaTotal.ToString())) + "</div>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "Qtde Moeda<br>" + "<font class=" + CssField + ">&nbsp;" +
                ((blt.Quantidade == 0) ? "" : blt.Quantidade.ToString()) + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "(x)Valor<br>" + "<font class=" + CssField + ">&nbsp;" +
                ((blt.ValorUnitario == 0) ? "" : String.Format("{0:C}", blt.ValorUnitario)) + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.BackColor = _CellEspecialColor;
            cell.Text = "(=)Valor Documento<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                String.Format("{0:C}", blt.ValorDocumento) + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 5

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.ColumnSpan = 6;
            cell.RowSpan = 5;
            cell.VerticalAlign = VerticalAlign.Top;
            if (blt.Instrucoes != null)
                cell.Text = BoletoTextos.Instrucoes + "<br/>" + "<font class=" + CssField + ">" +
                    blt.Instrucoes.Replace("\r\n", "<br/>") + "</font>";
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "(-)Descontos/Abatim.<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                ((blt.ValorDesconto == 0) ? "" : String.Format("{0:C}", blt.ValorDesconto)) + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 6

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "(-)Outras Deduções<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                ((blt.ValorOutras == 0) ? "" : String.Format("{0:C}", blt.ValorOutras)) + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 7

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "(+)Mora/Multa<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                ((blt.ValorMoraMulta == 0) ? "" : String.Format("{0:C}", blt.ValorMoraMulta)) + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 8

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "(+)Outros Acréscimos<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                ((blt.ValorAcrescimo == 0) ? "" : String.Format("{0:C}", blt.ValorAcrescimo)) + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 9

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Text = "(=)Valor Cobrado<br>" + "<div align=right class=" + CssField + ">&nbsp;" +
                ((blt.ValorCobrado == 0) ? "" : String.Format("{0:C}", blt.ValorCobrado)) + "</div>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            // Linha 9

            row = new TableRow();

            cell = new TableCell();
            cell.CssClass = CssCell;
            cell.Height = new Unit("70");
            cell.ColumnSpan = 7;
            cell.VerticalAlign = VerticalAlign.Top;
            if (blt.SacadoEndereco == "")
                cell.Text = BoletoTextos.Sacado + "<br/>" + "<font class=" + CssField + ">" +
                    blt.SacadoCOD + (blt.SacadoCOD == "" ? "" : ": ") + blt.Sacado + " " + blt.SacadoDocumento+ "</font>";
            else
                cell.Text = BoletoTextos.Sacado + "<br/>" + "<font class=" + CssField + ">" +
                    blt.SacadoCOD + (blt.SacadoCOD == "" ? "" : ": ") + blt.Sacado + " " + blt.SacadoDocumento + "<br>" +
                    blt.SacadoEndereco + "<br>" +
                    blt.Bairro + " - " + blt.Cidade + "<br>" +
                    "CEP: " + blt.Cep + " - " + blt.UF + "</font>";
            row.Cells.Add(cell);

            tbBol2.Rows.Add(row);

            tbBol2.RenderControl(output);

            #endregion

            // Código de Barras

            #region "Separador / Autenticação"

            Table tbFicha = new Table();
            tbFicha.CellPadding = 0;
            tbFicha.CellSpacing = 0;
            tbFicha.Width = new Unit("640");

            row = new TableRow();

            cell = new TableCell();
            cell.Text = (blt.Avalista == "") ? "" : (BoletoTextos.Avalista + ": ") + blt.Avalista;
            cell.CssClass = this.cssCell;
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.Text = "Autenticação Mecânica - FICHA DE COMPENSAÇÃO";
            cell.CssClass = this.cssCell;
            cell.HorizontalAlign = HorizontalAlign.Right;
            row.Cells.Add(cell);

            tbFicha.Rows.Add(row);
            tbFicha.RenderControl(output);

            //output.Write("<table border=1 width='640'><tr><td class='" + this.CssCell + "'>" +
            //    ((blt.Avalista == "") ? "" : (BoletoTextos.Avalista + ": ") + blt.Avalista) +
            //    "</td><td align=right class=" + this.CssCell + ">");
            //output.Write("Autenticação Mecânica - FICHA DE COMPENSAÇÃO</td></tr></table>");
            //output.Write("<table border=1 width='640'><tr><td>");

            #endregion

            #region "Código de barras"

            Table tbcodBar = new Table();
            tbcodBar.CellPadding = 0;
            tbcodBar.CellSpacing = 0;
            tbcodBar.Width = new Unit("640");

            row = new TableRow();
            cell = new TableCell();

            if (_BarCod == "")
            {
                StringBuilder sb = new StringBuilder();
                string cP = GetImage(ImageGetType.p);
                string cB = GetImage(ImageGetType.b);
                for (int i = 0; i < cBarras.Length; i += 2)
                {
                    switch (cBarras.Substring(i, 2))
                    {
                        case "bf":
                            if (imgCodBar)
                                sb.Append("<img src='" + cB + "' border='0' height='50' width='1' />");
                            else
                                sb.Append("<div style='display:inline-block;height:50px;width:1px;background-color:fff;'></div>");
                            break;

                        case "pf":
                            if (imgCodBar)
                                sb.Append("<img src='" + cP + "' border='0' height='50' width='1' />");
                            else
                                sb.Append("<div style='display:inline-block;height:50px;width:1px;background-color:000;'></div>");
                            break;

                        case "bl":
                            if (imgCodBar)
                                sb.Append("<img src='" + cB + "' border='0' height='50' width='3' />");
                            else
                                sb.Append("<div style='display:inline-block;height:50px;width:3px;background-color:fff;'></div>");
                            break;

                        case "pl":
                            if (imgCodBar)
                                sb.Append("<img src='" + cP + "' border='0' height='50' width='3' />");
                            else
                                sb.Append("<div style='display:inline-block;height:50px;width:3px;background-color:000;'></div>");
                            break;
                    }
                }
                cell.Text = sb.ToString();
            }
            else
                cell.Text = string.Format("<img src='{0}' border='0' />", _BarCod);

            row.Cells.Add(cell);
            tbcodBar.Rows.Add(row);

            if (!string.IsNullOrEmpty(blt.Informacoes))
            {
                //    output.Write("<tr><td align='center'>" + blt.Informacoes + "</td></tr>");
                
                row = new TableRow();
                cell = new TableCell();
                cell.CssClass = CssField;
                cell.HorizontalAlign = HorizontalAlign.Center;

                cell.Text = blt.Informacoes;
                row.Cells.Add(cell);
                tbcodBar.Rows.Add(row);
            }

            //output.Write("</table>");

            tbcodBar.RenderControl(output);

            #endregion

            output.WriteLine("</div>");

        }

          

        private string GetImage(ImageGetType tp)
        {
            string img;

            if (tp == ImageGetType.Banco)
                img=String.Format("{0:000}",blt.BancoNumero);
            else
                img=tp.ToString();

            if (imgType == BoletoImageType.embebed)
#if NET2 || NET4
                // Atenção: Não suportado com MVC
                img = this.Page.ClientScript.GetWebResourceUrl(typeof(BoletoWeb), "Impactro.Resources." + img + ".gif");
#else
				throw new Exception("Recurso disponivel a partir do .Net 2.0");
#endif
            else if(tp==ImageGetType.b ||tp==ImageGetType.p)
                img = String.Format("{0}{1}.gif", this.ImagePath, img);
            else
                img = String.Format("{0}{1}.{2}", this.ImagePath, img, imgType.ToString());
            return img;
        }

    }

    /// <summary>
    /// Tipo de obtenção da imagem do Boleto
    /// </summary>
    public enum BoletoImageType
    {
        /// <summary>
        /// Obtem as imagens dos Logo de Bancos, Código de barras e corte de dentro da DLL (Padrão)
        /// </summary>
        embebed=0,

        /// <summary>
        /// Arquivos .GIF externos
        /// </summary>
        gif=1,

        /// <summary>
        /// Arquivos .JPG externos
        /// </summary>
        jpg=2,

        /// <summary>
        /// Arquivos .PNG externos
        /// </summary>
        png=3
    }

    enum ImageGetType
    {
        Banco,
        corte,
        p,
        b
    }
}
