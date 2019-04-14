using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Banestes
    /// </summary>
    public abstract class Banco_Banestes
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "3";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cCodCedente, string cModalidade, string cNossoNumero )
        {
            cCodCedente = CobUtil.Right(cCodCedente, 11);
            cModalidade = CobUtil.Right(cModalidade, 1);

            if (CobUtil.GetInt(cCodCedente) == 0)
                throw new Exception("Informe o código do cedente");
            if (CobUtil.GetInt(cModalidade) == 0)
                throw new Exception("Informe o a modalidade");

            string cDAC12 = NossoNumero(ref cNossoNumero);
            string cLivre = cNossoNumero + cCodCedente + cModalidade + "021";
            blt.NossoNumeroExibicao = cNossoNumero + "-" + cDAC12;

            // (Corresponde à Chave ASBACE)
            int nD1 = CobUtil.Modulo10(cLivre);
            int nD2 = CobUtil.Modulo11Negativo(cLivre + nD1.ToString());
            if (nD2 == -1) // Veja a rotina do modulo11
            {
                nD1 = nD1 + 1;
                nD2 = CobUtil.Modulo11Negativo(cLivre + nD1.ToString());
            }

            cLivre = cLivre + nD1.ToString() + nD2.ToString();
            return cLivre;
        }

        public static string NossoNumero(ref string cNossoNumero)
        {
            cNossoNumero = CobUtil.Right(cNossoNumero, 8);
            string cD1 = CobUtil.Modulo11Especial(cNossoNumero, 9).ToString();
            string cD2 = CobUtil.Modulo11Especial(cNossoNumero + cD1, 10).ToString();
            return cD1 + cD2;
        }
    }

}
