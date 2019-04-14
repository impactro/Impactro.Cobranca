using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Sudameris
    /// </summary>
    public abstract class Banco_Sudameris
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "6";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cNossoNumero)
        {

            cNossoNumero = CobUtil.Right(cNossoNumero, 13);
            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cContaNumero = CobUtil.Right(cContaNumero, 7);

            string cDAC = CobUtil.Modulo10(cNossoNumero + cAgenciaNumero + cContaNumero).ToString();

            string cLivre = cAgenciaNumero +
                cContaNumero +
                cDAC +
                cNossoNumero;

            cContaNumero = cContaNumero + "-" + cDAC;

            blt.NossoNumeroExibicao = cNossoNumero;

            return cLivre;

        }
    }
}
