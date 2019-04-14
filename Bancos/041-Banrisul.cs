using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco do Banrisul
    /// </summary>
    public abstract class Banco_Banrisul
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "8";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cCodCedente, string cNossoNumero)
        {
            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 3);
            cCodCedente = CobUtil.Right(cCodCedente, 7);
            cNossoNumero = CobUtil.Right(cNossoNumero, 8);

            if (Int32.Parse(cCodCedente) == 0)
                throw new Exception("Informe o código do cedente");

            string cLivre = "21" +
                cAgenciaNumero +
                cCodCedente +
                cNossoNumero +
                "041";

            string cDV = CobUtil.Modulo10(cLivre).ToString() + CobUtil.Modulo11Padrao(cLivre, 7).ToString();
            cLivre = cLivre + cDV;

            string cDAC = CobUtil.Modulo10(cNossoNumero).ToString();
            cDAC = cDAC + CobUtil.Modulo11Padrao(cNossoNumero + cDAC, 7);
            blt.NossoNumeroExibicao = cNossoNumero + "-" + cDAC;

            return cLivre;
        }
    }
}
