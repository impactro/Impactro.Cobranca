using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco do Brasil
    /// </summary>
    public abstract class Banco_do_Brasil
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "9";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Instancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cConvenio, string cModalidade, string cCarteira, string cNossoNumero)
        {
            string cLivre;

            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cContaNumero = CobUtil.Right(cContaNumero, 8);
            cModalidade = CobUtil.Right(cModalidade, 2);
            cCarteira = CobUtil.Right(cCarteira, 2);

            if (CobUtil.GetInt(cConvenio) == 0)
                throw new Exception("Informe o código do convenio");
            if (CobUtil.GetInt(cModalidade) == 0)
                throw new Exception("Informe o a modalidade");
            if (CobUtil.GetInt(cCarteira) == 0)
                throw new Exception("Informe o a carteira");

            if (cConvenio.Length == 7)
            {
                // CÓDIGO DE BARRAS PARA EMISSÃO DE BLOQUETOS NAS CARTEIRAS 17 E 18,
                // EXCLUSIVO PARA CONVÊNIOS COM NUMERAÇÃO SUPERIOR À 1.000.000 (UM MILHÃO).
                if (cNossoNumero.Length == 17)
                {
                    cLivre = "000000" + // o convenio faz parte do nosso numero
                        cNossoNumero +
                        cCarteira;
                }
                else
                {
                    cNossoNumero = CobUtil.Right(cNossoNumero, 10);
                    cLivre = "000000" +
                        cConvenio +
                        cNossoNumero +
                        cCarteira;
                }
            }
            else if (cConvenio.Length == 6)
            {
                if (cCarteira == "16" || cCarteira == "18")
                {
                    if (cModalidade == "21")
                    {
                        cNossoNumero = CobUtil.Right(cNossoNumero, 17);
                        cLivre = cConvenio + // 6
                                 cNossoNumero + // 17
                                 cModalidade; // 2
                    }
                    else // Tipos de Convênio 2, 3, 4 ou 5. (COD CEDENTE=CONVENIO NO BB)
                    {
                        cNossoNumero = cConvenio + CobUtil.Right(cNossoNumero, 5); // 6 + 5
                        cLivre = cNossoNumero + // 11
                                 cAgenciaNumero + // 4
                                 cContaNumero + // 8
                                 cCarteira; // 2
                    }
                }
                else
                    throw new Exception("Carteira invalida");

                blt.CarteiraExibicao = cCarteira + "/0" + cModalidade;
            }
            else if (cConvenio.Length == 4)
            {
                cNossoNumero = cConvenio + CobUtil.Right(cNossoNumero, 7); // 4 + 7 => 11 
                cLivre =
                    cNossoNumero +      // 11 => 11
                    cAgenciaNumero +    // 11 + 4 => 15
                    cContaNumero +      // 15 + 8 => 23
                    cCarteira;          // 23 + 2 => 25 OK
            }
            else
                throw new Exception("Numero de Convenio Inválido (deveria ter 4, 6 ou 7 digitos)");

            if (cNossoNumero.Length == 17)
                blt.NossoNumeroExibicao = cNossoNumero;
            else
            {
                // Alterado aki por Georgenes
                string cDAC = NossoNumeroDV(cConvenio + cNossoNumero);
                // string cDAC = CobUtil.Modulo11Especial(cNossoNumero, 9).ToString();
                blt.NossoNumeroExibicao = cConvenio + cNossoNumero + "-" + cDAC;
            }

            return cLivre;
        }

        public static string NossoNumeroDV(string cNossoNumero)
        {
            int nResto;
            int nTotalNumero = CobUtil.Modulo11Total(cNossoNumero, 9);
            nResto = (10 * nTotalNumero) % 11;
            string cDAC;
            if (nResto == 10)
                cDAC = "X";
            else
                cDAC = nResto.ToString();

            return cDAC;
        }

        public static string NossoNumero(string cConvenio, string cModalidade, string cCarteira, string cNossoNumero)
        {

            if (CobUtil.GetInt(cConvenio) == 0)
                throw new Exception("Informe o código do convenio");

            if (cConvenio.Length == 7)
            {
                // CÓDIGO DE BARRAS PARA EMISSÃO DE BLOQUETOS NAS CARTEIRAS 17 E 18,
                // EXCLUSIVO PARA CONVÊNIOS COM NUMERAÇÃO SUPERIOR À 1.000.000 (UM MILHÃO).
                if (cNossoNumero.Length < 17)
                    cNossoNumero = CobUtil.Right(cNossoNumero, 10); // => 10 + convenio = 17
            }
            else if (cConvenio.Length == 6)
            {
                cModalidade = CobUtil.Right(cModalidade, 2);
                cCarteira = CobUtil.Right(cCarteira, 2);

                if (CobUtil.GetInt(cModalidade) == 0)
                    throw new Exception("Informe o a modalidade");
                if (CobUtil.GetInt(cCarteira) == 0)
                    throw new Exception("Informe o a carteira");

                if (cCarteira == "16" || cCarteira == "18")
                {
                    if (cModalidade == "21")
                        cNossoNumero = CobUtil.Right(cNossoNumero, 17);
                    else // Tipos de Convênio 2, 3, 4 ou 5. (COD CEDENTE=CONVENIO NO BB)
                        cNossoNumero = cConvenio + CobUtil.Right(cNossoNumero, 5); // 6 + 5 => 11 + convenio = 17
                }
                else
                    throw new Exception("Carteira invalida");
            }
            else if (cConvenio.Length == 4)
                cNossoNumero = cConvenio + CobUtil.Right(cNossoNumero, 7); // 4 + 7 => 11 + convenio = 15 (maz com zeros a frente não há problemas)
            else
                throw new Exception("Numero de Convenio Inválido (deveria ter 4, 6 ou 7 digitos)");

            if (cNossoNumero.Length < 17)
                cNossoNumero = cConvenio + cNossoNumero;

            return cNossoNumero;
        }
    }
}
