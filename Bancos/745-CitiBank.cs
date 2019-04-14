using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{

    /// <summary>
    /// Rotinas para o Banco Citibank
    /// </summary>
    public abstract class Banco_CitiBank
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "5";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cCodCedente, string cModalidade, string cNossoNumero)
        {
            // De acrodo com a documentação (pg 5) segue o calculo do digito do nosso numero
            cNossoNumero = CobUtil.Right(cNossoNumero, 11); // Força ter 11 digitos
            cModalidade = CobUtil.Right(cModalidade, 3); // Portfólio, 3 últimos dígitos do campo de identificação da empresa
            cCodCedente = CobUtil.Right(cCodCedente, 9); // 'Conta COSMOS (somente numeros, sem o indice - 1 digito) 0/123456/789

            if (CobUtil.GetInt(cCodCedente) == 0)
                throw new Exception("Informe o Código de Cedente");
            if (CobUtil.GetInt(cModalidade) == 0)
                throw new Exception("Informe a Modalidade");

            string cDV  = CobUtil.Modulo11Padrao(cNossoNumero, 9).ToString(); // Calcula o digito verificador
            blt.NossoNumeroExibicao = blt.NossoNumero + "." + cDV; // formata o numero com o digito na tela
            cNossoNumero += cDV; //acrescenta o digito no boleto

            //De acordo com a documentação (pg 9) os 25 caracteres do campo livre são
            //TAM - Descrição
            //  1 - Código do Produto 3 - Cobrança com registro / sem registro
            //  3 - Portfólio, 3 últimos dígitos do campo de identificação da empresa no CITIBANK (Posição 44 a 46 do arquivo retorno)
            //  6 - Base da conta COSMOS (pg 13, veja abaixo)
            //  2 - Seqüência da conta COSMOS (pg 13, veja abaixo)
            //  1 - Dígito Conta COSMOS (pg 13, veja abaixo)
            // 12 - Nosso Número 
            //----
            // 25 - Total (campo livre)

            //De acordo com a documentação (pg 13) temos a configuração da CONTA COSMOS
            //Ex.: 0/ 123456/ 789 = Conta Cosmos
            //     0 Índice
            //123456 Base (Posição 24 a 29)
            //    78 Seqüência (Posição 30 a 31)
            //     9 Dígito Verificador (Posição 32)

            //Parametros:
            //O código da conta COSMOS ficará no campo 'CodCedente' somento os numeros 123456789
            //O código do portfolio ficará no campo 'Modalidade'

            string cLivre = "3" +
                cModalidade +
                cCodCedente +
                cNossoNumero;

            return cLivre;
        }
    }
}
