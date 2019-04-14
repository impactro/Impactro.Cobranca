using Impactro.Cobranca;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Implementação do Render dos campos do layout do padrão
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("A95C9DC6-9A80-4668-99EE-F2211DCDA255")]
    [ProgId("BoletoNormal")]
    public class BoletoNormal : BoletoLayout
    {
        /// <summary>
        /// Retorna o array de campos a serme renderizados nas posicições corretas
        /// </summary>
        /// <param name="blt">Instancia de todoas as váriáveis parametrizadas</param>
        public override void MakeFields(Boleto blt)
        {
            string cTextoAdd = "";
            if (Fields==null)
                Fields = new List<FieldDraw>();

            if (blt.ExibeReciboSacado)
            {
                // Logo do Cedente/Recebedor
                if (blt.CedenteLogo != null)
                {
                    // Calcula a proporção para centralizar a imagem no local correto
                    // fw? -> fh9
                    //   w -> h
                    // fw? = w*fh9/fh
                    int h = blt.CedenteLogoHeight; // 9
                    int w = (int)((double)(h * blt.CedenteLogo.Width) / (double)blt.CedenteLogo.Height);
                    int x = (35 - w) / 2;
                    Fields.Add(new FieldDraw(x, 0, null, null, w, h) { Image = blt.CedenteLogo });
                    Height = h-9;
                }
                else if (blt.ImageBanco != null)
                {
                    // Logo padrão do Banco
                    Fields.Add(new FieldDraw(0, 0, null, null, 35, 9) { Image = blt.ImageBanco });
                    Height = 0;
                }

                // Linha Superior
                Fields.Add(new FieldDraw(38, 3 + Height, null, blt.BancoCodigo, 15, 6, StringAlignment.Center, 0x05));
                if (blt.ExibeReciboLinhaDigitavel)
                    Fields.Add(new FieldDraw(54, 3 + Height, null, blt.LinhaDigitavel, 115, 6, StringAlignment.Far, 0x00));
                else
                    Fields.Add(new FieldDraw(54, 3 + Height, null, BoletoTextos.Recibo, 115, 6, StringAlignment.Far, 0x00));

                // Linha 1: Centente, CNPJ, Vencimento
                Fields.Add(new FieldDraw(0, 9 + Height, BoletoTextos.Cedente, blt.Cedente, 94, 7, StringAlignment.Near, 0x0B));
                Fields.Add(new FieldDraw(94, 9 + Height, blt.CedenteDocumentoTipo, blt.CedenteDocumento, 35) { Linhas = 0x0B });
                Fields.Add(new FieldDraw(129, 9 + Height, "Vencimento", blt.DataVencimento.ToString("dd/MM/yyyy")) { Linhas = 0x0F, Destaque = true });

                // Linha 2: Endereço, Sacado
                Fields.Add(new FieldDraw(0, 16 + Height, "Endereço", blt.CedenteEndereco, 94) { Align = StringAlignment.Near });
                Fields.Add(new FieldDraw(94, 16 + Height, BoletoTextos.Sacado, blt.Sacado, 75) { Align = StringAlignment.Near, Linhas = 0x07 });
                // TODO: Endereço do Savado ?  blt.ExibeEnderecoReciboSacado ???

                // Linha 3: Nosso Numero, Carteira, Espécie, Quantidade, Valor
                Fields.Add(new FieldDraw(0, 23 + Height, "Nosso Número", blt.NossoNumeroExibicao));
                Fields.Add(new FieldDraw(40, 23 + Height, "Carteira", blt.CarteiraExibicao, 20) { Align = StringAlignment.Center });
                Fields.Add(new FieldDraw(60, 23 + Height, BoletoTextos.EspecieDoc, blt.Especie.ToString(), 20) { Align = StringAlignment.Center });
                Fields.Add(new FieldDraw(80, 23 + Height, "Quantidade", blt.Quantidade == 0 ? "" : blt.Quantidade.ToString(), 20) { Align = StringAlignment.Center });
                Fields.Add(new FieldDraw(100, 23 + Height, "Valor", blt.ValorUnitario == 0 ? "" : blt.ValorUnitario.ToString("C"), 29));
                Fields.Add(new FieldDraw(129, 23 + Height, BoletoTextos.CedenteConta, blt.AgenciaConta) { Linhas = 0x07 });

                // Linha 4: Data, Numero, Aceite, Especie do documento, Aceite, Parcelamento, Valor Documento
                Fields.Add(new FieldDraw(0, 30 + Height, "Data do Documento", blt.DataDocumento.ToString("dd/MM/yyyy")));
                Fields.Add(new FieldDraw(40, 30 + Height, "Número do Documento", blt.NumeroDocumento));
                Fields.Add(new FieldDraw(80, 30 + Height, "Aceite", blt.Aceite, 20) { Align = StringAlignment.Center });
                Fields.Add(new FieldDraw(100, 30 + Height, "Parcela", blt.ParcelaTotal > 0 ? (blt.ParcelaNumero + " de " + blt.ParcelaTotal) : "", 29) { Align = StringAlignment.Center });
                Fields.Add(new FieldDraw(129, 30 + Height, "(=)Valor Documento", blt.ValorDocumento.ToString("C")) { Linhas = 0x07, Destaque = true });

                // Linha 5: Demostrativo
                Fields.Add(new FieldDraw(0, 37 + Height, "Demonstrativo", cTextoAdd + CobUtil.ClearHTML(blt.Demonstrativo), 169, 11, StringAlignment.Near, 0x07));

                Fields.Add(new FieldDraw(142, 48 + Height, "Autenticação Mecânica", null) { Linhas = 0x00 });

                if (blt.ImageCorte != null)
                    Fields.Add(new FieldDraw(1, 53 + Height, null, null, 167, 3) { Image = blt.ImageCorte });

                Height += 60;
            }
            else
                Height = 0;

            // Segunda parte do boleto

            Width = 170;

            // Segunda parte
            Fields.Add(new FieldDraw(0, Height, null, null, 35, 9) { Image = blt.ImageBanco });
            Fields.Add(new FieldDraw(38, 3 + Height, null, blt.BancoCodigo, 15, 6, StringAlignment.Center, 0x05));
            Fields.Add(new FieldDraw(54, 3 + Height, null, blt.LinhaDigitavel, 115, 6, StringAlignment.Far, 0x00));

            Fields.Add(new FieldDraw(0, 9 + Height, "Local de Pagamento", blt.LocalPagamento, 129, 7, StringAlignment.Near, 0x0B));
            Fields.Add(new FieldDraw(129, 9 + Height, "Vencimento", blt.DataVencimento.ToString("dd/MM/yyyy")) { Linhas = 0x0F, Destaque = true });

            Fields.Add(new FieldDraw(0, 16 + Height, BoletoTextos.Cedente, blt.Cedente, 129) { Align = StringAlignment.Near });
            Fields.Add(new FieldDraw(129, 16 + Height, BoletoTextos.CedenteConta, blt.AgenciaConta) { Linhas = 0x07 });

            Fields.Add(new FieldDraw(0, 23 + Height, "Data Documento", blt.DataDocumento.ToString("dd/MM/yyyy"), 30));
            Fields.Add(new FieldDraw(30, 23 + Height, "Número do Documento", blt.NumeroDocumento, 35));
            Fields.Add(new FieldDraw(65, 23 + Height, BoletoTextos.EspecieDoc, blt.Especie.ToString(), 19) { Align = StringAlignment.Center });
            Fields.Add(new FieldDraw(84, 23 + Height, "Aceite", blt.Aceite, 12) { Align = StringAlignment.Center });
            Fields.Add(new FieldDraw(96, 23 + Height, "Data Processamento", blt.DataProcessamento.ToString("dd/MM/yyyy"), 33));
            Fields.Add(new FieldDraw(129, 23 + Height, "Nosso Número", blt.NossoNumeroExibicao) { Linhas = 0x07 });

            if( blt.CIP!="")
            {
                Fields.Add(new FieldDraw(0, 30 + Height, "Uso do Banco", "", 20)); // TODO: A ser descontinuado!
                Fields.Add(new FieldDraw(20, 30 + Height, "CIP", blt.CIP, 10) { Align = StringAlignment.Center });
                Fields.Add(new FieldDraw(30, 30 + Height, "Carteira", blt.CarteiraExibicao, 21) { Align = StringAlignment.Center });
            }
            else
                Fields.Add(new FieldDraw(0, 30 + Height, "Carteira", blt.CarteiraExibicao, 51) { Align = StringAlignment.Center });

            Fields.Add(new FieldDraw(51, 30 + Height, "Espécie", "R$", 14) { Align = StringAlignment.Center, Destaque = true });
            Fields.Add(new FieldDraw(65, 30 + Height, "Quantidade", blt.Quantidade == 0 ? "" : blt.Quantidade.ToString(), 31) { Align = StringAlignment.Center });
            Fields.Add(new FieldDraw(96, 30 + Height, "(x)Valor", blt.ValorUnitario == 0 ? "" : blt.ValorUnitario.ToString("C"), 33));
            Fields.Add(new FieldDraw(129, 30 + Height, "(=)Valor Documento", blt.ValorDocumento.ToString("C")) { Linhas = 0x07, Destaque = true });

            Fields.Add(new FieldDraw(0, 37 + Height, BoletoTextos.Instrucoes, cTextoAdd + CobUtil.ClearHTML(blt.Instrucoes), 129, 35, StringAlignment.Near));
            Fields.Add(new FieldDraw(129, 37 + Height, "(-)Descontos/Abatimentos", blt.ValorDesconto > 0 ? blt.ValorDesconto.ToString("C") : "") { Linhas = 0x07 });
            Fields.Add(new FieldDraw(129, 44 + Height, "(-)Outras Deduções", blt.ValorOutras > 0 ? blt.ValorOutras.ToString("C") : "") { Linhas = 0x07 });
            Fields.Add(new FieldDraw(129, 51 + Height, "(+)Mora/Multa", blt.ValorMoraMulta > 0 ? blt.ValorMoraMulta.ToString("C") : "") { Linhas = 0x07 });
            Fields.Add(new FieldDraw(129, 58 + Height, "(+)Outros Acréscimos", blt.ValorAcrescimo > 0 ? blt.ValorAcrescimo.ToString("C") : "") { Linhas = 0x07 });
            double nValor = blt.ValorDocumento - blt.ValorDesconto - blt.ValorOutras + blt.ValorMoraMulta + blt.ValorAcrescimo;
            Fields.Add(new FieldDraw(129, 65 + Height, "(=)Valor", nValor > 0 && nValor != blt.ValorDocumento ? nValor.ToString("C") : "") { Linhas = 0x07 });

            Fields.Add(new FieldDraw(0, 72 + Height, BoletoTextos.Sacado, blt.Sacado + " - " + blt.SacadoDocumento +
                "\r\n" + blt.SacadoEndereco +
                "\r\n" + blt.Bairro + " - " + blt.Cidade +
                " - " + blt.Cep + " - " + blt.UF, 169, 14, StringAlignment.Near, 0x0F));

            if (!string.IsNullOrEmpty(blt.Avalista))
                Fields.Add(new FieldDraw(79, 79 + Height, BoletoTextos.Avalista, blt.Avalista, 79, 7, StringAlignment.Near, 0));

            Fields.Add(new FieldDraw(114, 86 + Height, "Autenticação Mecânica / Ficha de compensação", null) { Linhas = 0x00 });

            if (!string.IsNullOrEmpty(blt.CodigoBarras))
                Fields.Add(new FieldDraw(5, 88 + Height, null, null, 0, 9) { Image = CobUtil.BarCodeImage(blt.CodigoBarras, 3, blt.DPI) });

            Height += 85 + 15;
        }
    }
}
