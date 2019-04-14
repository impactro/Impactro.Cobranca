using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o HSBC
    /// </summary>
    public abstract class Banco_HSBC
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "9";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cCodCedente, string cModalidade, string cCarteira, string cNossoNumero)
        {
            // Verificar CNR ou CNR Facil

            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cContaNumero = CobUtil.Right(cContaNumero, 5);
            cCodCedente = CobUtil.Right(cCodCedente, 7);

            if (CobUtil.GetInt(cCodCedente) == 0)
                throw new Exception("Informe o Código de Cedente");

            string cTotal = "0";
            string cDataJuliana = "0000";
            string cLivre; 
            DateTime dtVenc = blt.DataVencimento;

            //Utilizar o identificador 5 sempre que a data de vencimento estiver em branco e sem fator de vencimento. 
            if (dtVenc == new DateTime(2001, 1, 1))
                cModalidade = "5";

            if (cCarteira == "01") //Sem Registro
            {
                cNossoNumero = CobUtil.Right(cNossoNumero, 13);
                cNossoNumero = cNossoNumero + CobUtil.Modulo11Especial(cNossoNumero, 9).ToString();

                // Identificador 4: vincula vencimento, código do cedente e código do documento
                if (cModalidade == "4")
                {
                    // Monta a Data Juliana do Venciento
                    string cDia = CobUtil.Right(dtVenc.Day.ToString(), 2);
                    string cMes = CobUtil.Right(dtVenc.Month.ToString(), 2);
                    string cAno = CobUtil.Right(dtVenc.Year.ToString(), 2);

                    cDataJuliana = cDia + cMes + cAno;

                    // Efetua a soma Nosso numero (+Fim 4) + Cedente + Vencimento
                    cNossoNumero = cNossoNumero + "4";
                    cTotal = CobUtil.Soma(cDataJuliana, cCodCedente);
                    cTotal = CobUtil.Soma(cNossoNumero, cTotal);
                    cNossoNumero = cNossoNumero + ModuloHSBC(cTotal);

                    DateTime dStart = new DateTime(dtVenc.Year, 1, 1);
                    int nDias = (int)((TimeSpan)dtVenc.Subtract(dStart)).TotalDays + 1;
                    cDataJuliana = CobUtil.Right(nDias.ToString(), 3) + CobUtil.Right(dtVenc.Year.ToString(), 1);

                    blt.CarteiraExibicao = "CNR";
                    blt.AgenciaConta = cCodCedente;
                    blt.Especie = Especies.DM;
                    blt.LocalPagamento = "PAGAR PREFERENCIALMENTE EM AGENCIA HSBC";
                }
                else if (cModalidade == "5")
                {
                    // Identificador 5: vincula código do cedente e código do documento.
                    // Efetua a soma Nosso Bumero (+Fim 5) + Cedente 

                    cNossoNumero = cNossoNumero + "5";
                    cTotal = CobUtil.Soma(cNossoNumero, cCodCedente);
                    cNossoNumero = cNossoNumero + ModuloHSBC(cTotal);
                    cDataJuliana = "0000";
                }

                cLivre = CobUtil.Right(cCodCedente, 7) +
                         CobUtil.Right(blt.NossoNumero, 13) + 
                         cDataJuliana + "2";
            }
            else // Cartrira 00 
            {
                cNossoNumero = CobUtil.Right(cNossoNumero, 11);
                cCodCedente = CobUtil.Right(cCodCedente, 11);
                cCarteira = CobUtil.Right(cCarteira, 2);
                cModalidade = CobUtil.Right(cModalidade, 1);

                cLivre = cNossoNumero + cCodCedente + cCarteira + cModalidade;
            }

            //if (cModalidade != "5")
                blt.NossoNumeroExibicao =
                    cNossoNumero.Substring(0, cNossoNumero.Length - 3) + " " +
                    cNossoNumero.Substring(cNossoNumero.Length - 3);
            //else
            //    blt.NossoNumeroExibicao = cNossoNumero;

            return cLivre;
        }


        /// <summary>
        /// Calcula um Modulo especial apenas para o banco HSBC
        /// </summary>
        /// <param name="cTexto">Sequencia de digitos a calcular</param>
        /// <returns>Modulo (resto)</returns>
        public static int ModuloHSBC(string cTexto)
        {
            int nContador, nNumero, nTotal, nMultiplicador, nResultado;
            string cCaracter;

            nTotal = 0;
            nMultiplicador = 9;

            for (nContador = cTexto.Length - 1; nContador >= 0; nContador--)
            {
                cCaracter = cTexto.Substring(nContador, 1);
                nNumero = Int32.Parse(cCaracter) * nMultiplicador;
                nTotal += nNumero;
                nMultiplicador -= 1;

                if (nMultiplicador < 2)
                    nMultiplicador = 9;
            }

            nResultado = nTotal % 11;

            if (nResultado == 10 || nResultado == 0)
                return 0;
            else
                return nResultado;

        }
    }

}
