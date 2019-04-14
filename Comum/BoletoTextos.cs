using System;
using System.Collections.Generic;
using System.Text;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Textos estaticos que podem ser alterados, validos para qualquer instancia
    /// </summary>
    public class BoletoTextos
    {
        public static string LocalPagamento = "Até o vencimento pagável em qualquer banco";
        public static string Cedente = "Beneficiário";
        public static string CedenteConta = "Agência/Código do Beneficiário";
        public static string Sacado = "Pagador";
        public static string Recibo = "Recibo do Pagador";
        public static string Instrucoes = "Instruções (Texto de responsabilidade do beneficiário)";
        public static string EspecieDoc = "Espécie Doc.";
        public static string Avalista = "Avalista";
    }
}
