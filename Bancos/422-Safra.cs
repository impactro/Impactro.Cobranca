using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Safra
    /// </summary>
    public abstract class Banco_Safra
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "7";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cCarteira, string cCodCedente, string cAgencia, string cConta, string cNossoNumero)
        {
            string cLivre;

            if (CobUtil.GetInt(cCarteira) == 0)
                throw new Exception("Informe a Carteira");

            if (cCarteira == "1" || cCarteira == "2")
            {
                cAgencia = CobUtil.Right(cAgencia.Replace("-", ""), 5);
                cConta = CobUtil.Right(cConta.Replace("-", ""), 9);
                cNossoNumero = CobUtil.Right(cNossoNumero, 9);

                cLivre = "7" +
                    cAgencia +
                    cConta +
                    cNossoNumero +
                    cCarteira;
            }
            else if (cCarteira == "4")
            {
                cCodCedente = CobUtil.Right(cCodCedente, 7);
                cNossoNumero = CobUtil.Right(cNossoNumero, 16);

                if (CobUtil.GetInt(cCodCedente) == 0)
                    throw new Exception("Informe o Código de Cedente");

                cLivre = "7" +
                    cCodCedente +
                    cNossoNumero +
                    "4";
            }
            else
                throw new Exception("Este banco suporta apenas carteiras '1', '2' ou '4'");

            blt.NossoNumeroExibicao = cNossoNumero;

            return cLivre;

        }
    }

}
