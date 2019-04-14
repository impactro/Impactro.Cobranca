using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using Impactro.Layout;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Itau
    /// </summary>
    public abstract class Banco_Itau
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
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cCarteira, string cCodCedente, string cNossoNumero, string cNumeroDocumento)
        {
            string cDAC = NossoNumero(ref cAgenciaNumero, ref cContaNumero, ref cCarteira, ref cNossoNumero);

            if (CobUtil.GetInt(cCarteira) == 0)
                throw new Exception("Informe a Carteira");
            if (CobUtil.GetInt(cAgenciaNumero) == 0)
                throw new Exception("Informe a Agencia");
            if (CobUtil.GetInt(cContaNumero) == 0)
                throw new Exception("Informe a Conta");

            string cLivre;
            if (cCarteira == "107" || cCarteira == "122" || cCarteira == "142" || cCarteira == "143" || cCarteira == "196" || cCarteira == "198")
            {
                string cNumDoc = CobUtil.Right(cNumeroDocumento, 7);
                cCodCedente = CobUtil.Right(cCodCedente, 5);

                cLivre = cCarteira +
                    cNossoNumero +
                    cNumDoc +
                    cCodCedente;

                cLivre += CobUtil.Modulo10(cLivre) + "0";
            }
            else
            {
                cLivre = cCarteira +
                    cNossoNumero +
                    cDAC +
                    cAgenciaNumero +
                    cContaNumero +
                    CobUtil.Modulo10(cAgenciaNumero + cContaNumero) +
                    "000";
            }

            blt.NossoNumeroExibicao = cCarteira + "/" + cNossoNumero + "-" + cDAC;
            blt.AgenciaConta = cAgenciaNumero + "/" + blt.Conta;
                        
            return cLivre;

        }
        
        public static string NossoNumero(ref string cAgenciaNumero, ref string cContaNumero, ref string cCarteira, ref string cNossoNumero)
        {
            if (cContaNumero.Length > 5)
                throw new Exception("O Número da conta deve contar no maximo 5 digitos, sem o verificador: 12345-6");

            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cContaNumero = CobUtil.Right(cContaNumero, 5);
            cCarteira = CobUtil.Right(cCarteira, 3);
            cNossoNumero = CobUtil.Right(cNossoNumero.Split('-')[0], 8);

            if (cCarteira == "112" || cCarteira == "126" || cCarteira == "131" || cCarteira == "146" || cCarteira == "150" || cCarteira == "168")
                return CobUtil.Modulo10(cCarteira + cNossoNumero).ToString();
            else
                return CobUtil.Modulo10(cAgenciaNumero + cContaNumero + cCarteira + cNossoNumero).ToString();

        }
    }

}
