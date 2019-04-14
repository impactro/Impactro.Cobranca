using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Bradesco
    /// </summary>
    public abstract class Banco_Bradesco
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "2";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cCarteira, string cNossoNumero, string cModalidade)
        {
            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cCarteira = CobUtil.Right(cCarteira, 2);
            cContaNumero = CobUtil.Right(cContaNumero, 7);

            if (CobUtil.GetInt(cCarteira) == 0)
                throw new Exception("Informe a Carteira");

            string DAC = NossoNumero(cCarteira, ref cModalidade, ref cNossoNumero);
            
            string cLivre = cAgenciaNumero +
                cCarteira + // 2
                cModalidade + // 4 / 2
                cNossoNumero + // 7 / 9
                cContaNumero + "0";

            if (cModalidade != "")
                blt.NossoNumeroExibicao = cCarteira + "/" + cModalidade + "/" + cNossoNumero + "-" + DAC;
            else
                blt.NossoNumeroExibicao = cCarteira + "/" + cNossoNumero + "-" + DAC;

            return cLivre;

        }

        /// <summary>
        /// Calcula o digito do nosso numero, já ajustando o comprimento total
        /// </summary>
        public static string NossoNumero(string cCarteira, ref string cModalidade, ref string cNossoNumero)
        {
            // para o digito verificador o nosso numero tem 11 digitos (Carteira+Modalidade+NossoNumero)
            // Mas quando o resto for 10, tem que aparecer 'P'
            // CobUtil.Modulo11Padrao(cCarteira + cModalidade + cNossoNumero, 7);
            // pagina 19

            if (cNossoNumero.Length == 11)
            {
                cModalidade = "";
            }
            else if (cModalidade.Length == 4)
            {
                // Modalidade com 4 digitos!
                cNossoNumero = CobUtil.Right(cNossoNumero, 7);
            }
            else
            {
                // Modalidade com 2 digitos
                cModalidade = CobUtil.Right(cModalidade, 2);
                cNossoNumero = CobUtil.Right(cNossoNumero, 9);
            }

            cCarteira = CobUtil.Right(cCarteira, 2);
            if (cModalidade.Length + cNossoNumero.Length != 11)
                throw new Exception("Combinação Modalidade e Nosso numero invalidos");

            int TotalNumero = CobUtil.Modulo11Total(cCarteira + cModalidade + cNossoNumero, 7);
            TotalNumero *= 10;
            int Resto = TotalNumero % 11;
            if (Resto == 10)
                return "P";
            else
                return Resto.ToString();
        }
    }

}
