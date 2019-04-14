using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Banespa (modelo antigo de Fev/2001)
    /// Veja o Banco Santander! Banco 33
    /// </summary>
    public abstract class Banco_Banespa
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "7";

        /// <summary>
        /// define se deve ser usado a nova logica de geração no Santander em vez do antigo Banespa
        /// </summary>
        //public static bool useSantander = false;

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <param name="cCodCedente">Código do Cedente</param>
        /// <param name="cNossoNumero">Nosso Numero</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cCodCedente, string cNossoNumero )
        {
            if (cCodCedente.Length != 11)
                throw new Exception("O código do cedente deve ter 11 digitos");

            cCodCedente = CobUtil.Right(cCodCedente, 11);
            cNossoNumero = CobUtil.Right(cNossoNumero, 7);

            if (Int64.Parse(cCodCedente) == 0)
                throw new Exception("Informe o código do cedente");

            string cLivre = 
                cCodCedente + 
                cNossoNumero + 
                "00" + 
                "033";

            string cD1;
            string cD2;
            cD1 = CobUtil.Modulo10(cLivre).ToString();
            cD2 = CobUtil.Modulo11Padrao(cLivre + cD1, 7).ToString();
            if (cD2 == "1")
            {
                cD1 = "0";
            loopDigitoInc:
                cD2 = CobUtil.Modulo11Padrao(cLivre + cD1, 7).ToString();
                if (cD2 == "1")
                {
                    cD1 = string.Format("{0}", Int32.Parse(cD1) + 1);
                    goto loopDigitoInc;
                }
            }
            
            cLivre = cLivre + cD1+ cD2;

            string cAgenciaNumero = cCodCedente.Substring(0, 3);

            blt.NossoNumeroExibicao = cAgenciaNumero + " " + cNossoNumero + "-" + DVBanespa(cAgenciaNumero + cNossoNumero);
            //blt.NossoNumeroExibicao = cAgenciaNumero + " " + cNossoNumero + "-" + CobUtil.Modulo11Especial(cAgenciaNumero + cNossoNumero, 9);
            blt.AgenciaConta = cCodCedente.Substring(0, 3) + " " + cCodCedente.Substring(3, 2) + " " + cCodCedente.Substring(5, 5) + " " + cCodCedente.Substring(10, 1);

            return cLivre;
        }

        
        /// <summary>
        /// Rotina para Calculo do digito do Nosso numero da Nossa Caixa
        /// </summary>
        /// <param name="cValor">Valor a calcular</param>
        /// <returns>Dígito</returns>
        private static int DVBanespa(string cValor)
        {

            int nContador, nV, nP;
            string cPeso = "7319731973";
            int nTotal = 0;

            for (nContador = cValor.Length - 1; nContador >= 0; nContador--)
            {
                nV = int.Parse(cValor.Substring(nContador, 1));
                nP = int.Parse(cPeso.Substring(nContador, 1));
                nTotal += ((nV * nP) % 10); // apenas unidade
            }

            int nDig = nTotal % 10; // apenas unidade

            if (nDig == 0)
                return 0;
            else
                return 10 - nDig;

        }
        
    }

    /*
    /// <summary>
    /// rotinas para o Banco Santander (banco 33)
    /// </summary>
    public abstract class Banco_SantanderBanespa
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "7";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <param name="cCodCedente">Código do Cedente</param>
        /// <param name="cNossoNumero">Nosso Numero</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cCodCedente, string cNossoNumero, string cCarteira, string cModalidade)
        {
        
            cCodCedente = CobUtil.Right(cCodCedente, 7);
            cNossoNumero = CobUtil.Right(cNossoNumero, 13);
            cModalidade = CobUtil.Right(cModalidade, 1); // Campo: IOS – Seguradoras (Se 7% informar 7. Limitado a 9%) 
            cCarteira = CobUtil.Right(cCarteira, 3);

            string cLivre = "9" +
                cCodCedente + 
                cNossoNumero + 
                cModalidade +
                cCarteira;

            blt.NossoNumeroExibicao = cNossoNumero;
            blt.AgenciaConta = blt.Agencia + "/" + cCodCedente;
            blt.CarteiraExibicao = "C/REG";

            return cLivre;
        }
        
    }
*/

}
