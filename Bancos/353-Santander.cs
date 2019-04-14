using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco Santander (banco 353)
    /// </summary>
    public abstract class Banco_Santander
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
        public static string CampoLivre(Boleto blt, string cCodCedente, string cCarteira, string cNossoNumero, string cModalidade)
        {
            cCodCedente = CobUtil.Right(cCodCedente, 7);    // Formata e valida o código do Cedente com 7 digitos
            if (CobUtil.GetInt(cCodCedente) == 0)
                throw new Exception("Informe o Código de Cedente");
            // Valida as carteiras disponíveis
            else if (!(cCarteira == "101" || cCarteira == "102" || cCarteira == "201"))
                throw new Exception("Carteira invalida");
            else if (cModalidade.Length > 1)
                throw new Exception("Modalidade inválida, em geral deve ser '0'(Zero), ou o valor do IOF para seguradoras");

            cNossoNumero = CobUtil.Right(cNossoNumero, 12); // Monta o Nosso Número 12 Digitos 
            // (não importa se tiver 7 digitos no padrão CNAB400, pois zeros a frente não interfere na geração do digito do modulo 11)
            string cDig = CobUtil.Modulo11Especial(cNossoNumero, 9).ToString(); // Calcula o digito verificador
            string cLivre = "9" +
                    cCodCedente +
                    cNossoNumero +
                    cDig +
                    (cModalidade == "" ? "0" : cModalidade) + // IOF – Seguradoras (Se 7% informar 7, Limitado a 9%) Demais clientes usar 0 (zero)
                    cCarteira;
            if (cCarteira == "101") // 101-Cobrança Simples Rápida COM Registro
                blt.CarteiraExibicao = "COB SIMPLES RCR";
            else if (cCarteira == "102") // 102-Cobrança simples SEM Registro
                blt.CarteiraExibicao = "COB SIMPLES CSR"; 
            else if (cCarteira == "201") // 201-Penhor
                blt.CarteiraExibicao = "COB PENHOR RCR";
            blt.NossoNumeroExibicao = cNossoNumero + "-" + cDig;
            blt.AgenciaConta = blt.Agencia + " / " + cCodCedente;

            return cLivre;
        }
    }

}
