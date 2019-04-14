using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Unibanco
    /// </summary>
    public abstract class Banco_Unibanco
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "0";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cCodCedente, string cModalidade, string cNossoNumero)
        {

            cCodCedente = CobUtil.Right(cCodCedente, 7);

            if (CobUtil.GetInt(cCodCedente) == 0)
                throw new Exception("Informe o Código de Cedente");

            if (cModalidade == "14")
                cNossoNumero = CobUtil.Right(cNossoNumero, 14);
            else
            
                cNossoNumero = cCodCedente + CobUtil.Right(cNossoNumero, 7);

            string cLivre = "5" +
                   cCodCedente +
                   "00" +
                   cNossoNumero;

            string cDAC = CobUtil.Modulo11Especial(cNossoNumero, 9).ToString();
            cLivre = cLivre + cDAC;

            blt.NossoNumeroExibicao = cNossoNumero + "-" + cDAC;

            return cLivre;

        }
    }
}
