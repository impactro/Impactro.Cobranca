using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Real
    /// </summary>
    public abstract class Banco_Real
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "5";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cNossoNumero)
        {
            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cContaNumero = CobUtil.Right(cContaNumero, 7);
            cNossoNumero = CobUtil.Right(cNossoNumero, 13);

            string cDAC = CobUtil.Modulo10(cNossoNumero + cAgenciaNumero + cContaNumero).ToString();

            string cLivre = cAgenciaNumero +
                cContaNumero +
                cDAC +
                cNossoNumero;

            blt.AgenciaConta = cAgenciaNumero + "/" + cContaNumero + "-" + cDAC;
            blt.NossoNumeroExibicao = cNossoNumero;

            return cLivre;

        }
    }

}
