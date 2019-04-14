using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{   
    /// <summary>
    /// Rotinas para o Banco Sicredi
    /// </summary>
    public abstract class Banco_SICOOB
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
        public static string CampoLivre(Boleto blt, string cCarteira, string cParcela, string cConvenio, string cModalidade, string cCodCedente, string cNossoNumero)
        {

            //Ver página 6 da documentação
            cCarteira = CobUtil.Right(cCarteira, 1);            //Código da carteira
            cModalidade = CobUtil.Right(cModalidade, 2);        //Modalidade
            cParcela = CobUtil.Right(cParcela, 3);              //Número da Parcela

            string cDV = NossoNumero(ref cConvenio, ref cCodCedente, ref cNossoNumero);
            cNossoNumero += cDV;

            string cLivre = cCarteira + cConvenio + cModalidade + cCodCedente + cNossoNumero + cParcela;

            blt.AgenciaConta = cConvenio + "/" + cCodCedente;
            blt.NossoNumeroExibicao = cNossoNumero.Substring(0, cNossoNumero.Length - 1) + "-" + cDV;

            return cLivre;

        }

        public static string NossoNumero(ref string cConvenio, ref string cCodCedente, ref string cNossoNumero)
        {
            cConvenio = CobUtil.Right(cConvenio, 4);            //Código da Cooperativa
            cCodCedente = CobUtil.Right(cCodCedente, 7);        //Código do Cliente
            cNossoNumero = CobUtil.Right(cNossoNumero, 7);      //Número do Título
            return CalculaDigCoob(cConvenio + "000" + cCodCedente + cNossoNumero).ToString();
        }

        public static int CalculaDigCoob(string cValor)
        {
            int nContador, nV, nP, nD, nTotal, nResto;
            string cPeso = "319731973197319731973";
            //string cTotal="";
            nTotal = 0;
            for (nContador = 0; nContador < cValor.Length; nContador++)
            {
                nV = Int32.Parse(cValor.Substring(nContador, 1));
                nP = Int32.Parse(cPeso.Substring(nContador, 1));
                nD = (nV * nP);
                //cTotal+= nD + " ";
                nTotal += nD;
            }
            nResto = nTotal % 11;
            if (nResto == 0 || nResto == 1)
                return 0;
            else
                return 11 - nResto;
        }
    }

}
