using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{

    /// <summary>
    /// Rotinas para o Banco Mercantil
    /// </summary>
    public abstract class Banco_Mercantil
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "1";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cCodCedente, string cModalidade, string cNossoNumero)
        {

            cAgenciaNumero = CobUtil.Right(cAgenciaNumero.Split('-')[0], 4);
            cCodCedente = CobUtil.Right(cCodCedente, 9);
            cModalidade = CobUtil.Right(cModalidade, 1); //Indicador de desconto
            cNossoNumero = CobUtil.Right(cNossoNumero, 11);

            if (CobUtil.GetInt(cCodCedente) == 0)
                throw new Exception("Informe o Código de Cedente");
            if (CobUtil.GetInt(cModalidade) == 0)
                throw new Exception("Informe a Modalidade");

            string cLivre = cAgenciaNumero +
                            cNossoNumero +
                            cCodCedente +
                            cModalidade;

            //No codigo de barras o nosso numero te 9 posições, mas no calculo do digito do nosso numero há 10 posições
            string cDAC = CobUtil.Modulo11Padrao(cAgenciaNumero + "0" + cNossoNumero, 9).ToString();
            blt.NumeroDocumento = cNossoNumero + "-" + cDAC;
            blt.NossoNumeroExibicao = cNossoNumero;

            return cLivre;

        }
    }
}
