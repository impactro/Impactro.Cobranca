using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Banese
    /// </summary>
    public abstract class Banco_Banese
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
        public static string CampoLivre(Boleto blt, string cAgencia, string cCodCedente, string cNossoNumero )
        {
            string[] cAgenciaParts = cAgencia.Split('-', '/');
            string cAgenciaConta = CobUtil.Right(cAgenciaParts[0], 2);
            string cAgenciaDig = CobUtil.Right(cAgenciaParts[1], 1);
            cCodCedente = CobUtil.Right(cCodCedente, 9); 
            cNossoNumero = CobUtil.Right(cNossoNumero, 8);

            if (Int32.Parse(cCodCedente) == 0)
                throw new Exception("Informe o código do cedente");

            // cAgenciaDig
            string cDAC = CobUtil.Modulo11Padrao(cAgenciaConta + cNossoNumero, 9).ToString();
            blt.NossoNumeroExibicao = cNossoNumero + "-" + cDAC ;
            cNossoNumero = cNossoNumero + cDAC;

            string cLivre = cAgenciaConta + cCodCedente + cNossoNumero + "047";

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
    }

}
