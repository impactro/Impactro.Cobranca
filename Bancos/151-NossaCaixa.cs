using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Nossa Caixa
    /// </summary>
    public abstract class Banco_NossaCaixa
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
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cModalidade, string cNossoNumero)
        {
            cNossoNumero = CobUtil.Right(cNossoNumero, 9);      // O Nosso Numero tem que ter sempre 9 posições, sendo a primeira o digito "9"
            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cModalidade = CobUtil.Right(cModalidade, 2);        // (mas será utilizado somente o ultimo digito da modalidade)
            cContaNumero = CobUtil.Right(cContaNumero, 6);      // numero da conta sem o digito

            if (CobUtil.GetInt(cModalidade) == 0)
                throw new Exception("Informe a Modalidade");

            if (cNossoNumero.Substring(0, 1) != "9")
                throw new Exception("Para o Banco 'Nossa Caixa' o 'Nosso Numero' deve ter 9 posições iniciado sempre pelo digito '9'");
            
            string cContaDig = blt.Conta.Split('-')[1];

            //O campo nosso numero tem que iniciar com 9 e ter 9 digitos
            string cLivre = cNossoNumero +
                cAgenciaNumero +
                cModalidade.Substring(1) + // utiliza apenas o ultimo digito da modalidade
                cContaNumero +
                "151";

            int D1 = CobUtil.Modulo10(cLivre);
            int D2 = CobUtil.Modulo11Negativo(cLivre + D1.ToString());
            while (D2 == -1) // Veja a rotina do modulo11
            {
                D1 = D1 + 1;
                D2 = CobUtil.Modulo11Negativo(cLivre + D1.ToString());
            }
            cLivre = cLivre + D1.ToString() + D2.ToString();

            //Calucla o digito do Nosso Numero

            int nTotal = DVNossaCaixa(cAgenciaNumero + cModalidade + "0" + cContaNumero) + int.Parse(cContaDig);
            nTotal = nTotal + DVNossaCaixa(cNossoNumero);

            int nResto = nTotal % 10;
            nResto = 10 - nResto;

            blt.NossoNumeroExibicao = cNossoNumero + "-" + nResto;
            blt.CarteiraExibicao = "CIDENT";
            blt.AgenciaConta = cAgenciaNumero + "/" + cModalidade + "/" + blt.Conta;

            return cLivre;
        }

        /// <summary>
        /// Rotina para Calculo do digito do Nosso numero da Nossa Caixa
        /// </summary>
        /// <param name="cValor">Valor a ser calculado</param>
        /// <returns>digito resultado do calculo</returns>
        private static int DVNossaCaixa(string cValor)
        {

            int nContador, nV, nP;
            string cPeso = "31973197319731973197";
            int nTotal = 0;

            for (nContador = cValor.Length - 1; nContador >= 0; nContador--)
            {
                nV = int.Parse(cValor.Substring(nContador, 1));
                nP = int.Parse(cPeso.Substring(nContador, 1));
                nTotal += (nV * nP);
            }

            return nTotal;

        }
    }

}
