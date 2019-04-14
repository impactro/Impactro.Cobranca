using System;
using System.Collections.Generic;
using System.Text;
using Impactro.Cobranca;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Impactro.Cobranca
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("89E8A1B3-1BC1-43F6-A396-3807E419DE4B")]
    [ProgId("BoletoCarne")]
    public class BoletoCarne : BoletoNormal
    {
        public override void MakeFields(Boleto blt)
        {
            Fields = new List<FieldDraw>();

            // Tamanho reservado para o logo
            //blt.CedenteLogoHeight = 9;

            // Parte 1
            int p1Width = 47;
            Fields.Add(new FieldDraw(0, 0, null, null, 35, 9) { Image = blt.ImageBanco });
            Fields.Add(new FieldDraw(35, 2, null, blt.BancoCodigo, p1Width - 35, 7, StringAlignment.Center, 0x01));
            Fields.Add(new FieldDraw(0, 9, "Parcela/Plano", blt.ParcelaTotal > 0 ? (blt.ParcelaNumero + " de " + blt.ParcelaTotal) : "", 20, 7, StringAlignment.Near, 0x0F));
            Fields.Add(new FieldDraw(20, 9, "Vencimento", blt.DataVencimento.ToString("dd/MM/yyyy"), p1Width - 20) { Destaque = true });
            Fields.Add(new FieldDraw(0, 16, BoletoTextos.CedenteConta, blt.AgenciaConta, p1Width));
            Fields.Add(new FieldDraw(0, 23, "Nosso Número", blt.NossoNumeroExibicao, p1Width));
            Fields.Add(new FieldDraw(0, 30, "Número do Documento", blt.NumeroDocumento, p1Width));
            Fields.Add(new FieldDraw(0, 37, "Espécie", blt.MoedaEspecie, 10, 7, StringAlignment.Center) { Destaque = true });
            Fields.Add(new FieldDraw(10, 37, "Qtd.", blt.Quantidade.ToString(), 10, 7, StringAlignment.Center));
            Fields.Add(new FieldDraw(20, 37, "Valor Unitário", blt.ValorUnitario == 0 ? "" : blt.ValorUnitario.ToString("C"), p1Width - 20));
            Fields.Add(new FieldDraw(0, 44, "Valor do Documento", blt.ValorDocumento.ToString("C"), p1Width) { Destaque = true });
            Fields.Add(new FieldDraw(0, 51, "(-) Desconto/Abatimento", blt.ValorDesconto == 0 ? "" : blt.ValorDesconto.ToString("C"), p1Width));
            Fields.Add(new FieldDraw(0, 58, "(+) Mora/Multa", blt.ValorMoraMulta == 0 ? "" : blt.ValorMoraMulta.ToString("C"), p1Width));
            double nValor = blt.ValorDocumento - blt.ValorDesconto - blt.ValorOutras + blt.ValorMoraMulta + blt.ValorAcrescimo;
            Fields.Add(new FieldDraw(0, 65, "(=) Valor Cobrado", nValor > 0 && nValor != blt.ValorDocumento ? nValor.ToString("C") : "", p1Width));
            Fields.Add(new FieldDraw(0, 72, BoletoTextos.Sacado, blt.Sacado + "\r\n" + blt.SacadoDocumento + "\r\n" + blt.SacadoEndereco, p1Width, 14, StringAlignment.Near));

            // Pocição inicial do array
            int i = Fields.Count;
            blt.ExibeReciboSacado = false;
            
            base.MakeFields(blt);

            for (; i < Fields.Count; i++)
                Fields[i].X += p1Width + 3;

            Width += p1Width + 3;
        }
    }
}