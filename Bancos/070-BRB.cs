using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Nossa Caixa
    /// </summary>
    public abstract class Banco_BRB
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
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cCarteira, string cNossoNumero)
        {
            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 3);
            cContaNumero = CobUtil.Right(cContaNumero, 7);
            cCarteira = CobUtil.Right(cCarteira, 1);
            cNossoNumero = CobUtil.Right(cNossoNumero, 6);

            if (CobUtil.GetInt(cCarteira) == 0)
                throw new Exception("Informe a Carteira");

            //O campo nosso numero tem que iniciar com 9 e ter 9 digitos
            string cLivre = "000" +
                cAgenciaNumero +
                cContaNumero + 
                cCarteira +
                cNossoNumero +
                "070";

            int D1 = CobUtil.Modulo10(cLivre);
            int D2 = CobUtil.Modulo11Negativo(cLivre + D1.ToString());
            while (D2 == -1) // Veja a rotina do modulo11
            {
                D1 = D1 + 1;
                D2 = CobUtil.Modulo11Negativo(cLivre + D1.ToString());
            }
            cLivre = cLivre + D1.ToString() + D2.ToString();

            //Monta o Nosso Numero
            blt.NossoNumeroExibicao = cCarteira + cNossoNumero + "070" + D1.ToString() + D2.ToString();

            // Monta a Exibição da Agencia/Conta
            blt.AgenciaConta = "000 " + cAgenciaNumero + " " + cContaNumero ;

            return cLivre;
        }
    }
}
