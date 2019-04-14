using Impactro.Cobranca;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

// Baseado no layout do Sicredi 05/09/2015
namespace Impactro.Layout
{
    
    [ComVisible(false)]
    public class CNAB400UniCred : CNAB400<CNAB400HeaderUniCred, CNAB400Remessa1UniCred, CNAB400Trailer1UniCred>
    {
        public override string Remessa()
        {

            string[] cBanco = Cedente.Banco.Split('-');
            Bancos banco = (Bancos)CobUtil.GetInt(cBanco[0]);

            if (banco != Bancos.UniCred)
                throw new Exception("Esta classe é valida apenas para o Banco UniCred");

            // Proximo item
            SequencialRegistro = 1;

            regArqHeader[CNAB400HeaderUniCred.Conta] = Cedente.CodCedente;
            regArqHeader[CNAB400HeaderUniCred.Empresa] = Cedente.Cedente;
            regArqHeader[CNAB400HeaderUniCred.Data] = DataHoje;
            regArqHeader[CNAB400HeaderUniCred.SequenciaArquivo] = NumeroLote;
            regArqHeader[CNAB400HeaderUniCred.Sequencia] = SequencialRegistro++;
            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // o sequencial do header é sempre 1 (FIXO)
            Add(regArqHeader);

            string cAgenciaNumero = Cedente.Agencia.Split('-')[0];
            string cModalidade = Cedente.Modalidade;
            string cCodCedente = Cedente.CodCedente;
            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB400Remessa1UniCred> regBoleto;

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                regBoleto = new Reg<CNAB400Remessa1UniCred>();

                string cNossoNumero = boleto.NossoNumero;
                //Banco_UniCred.MontaNossoNumero(ref cNossoNumero, ref cAgenciaNumero, ref cModalidade, ref cCodCedente, boleto.DataVencimento);

                regBoleto[CNAB400Remessa1UniCred.NumeroDocumento] = boleto.NumeroDocumento;
                regBoleto[CNAB400Remessa1UniCred.NossoNumero] = cNossoNumero;
                regBoleto[CNAB400Remessa1UniCred.DataVencimento] = boleto.DataVencimento;
                regBoleto[CNAB400Remessa1UniCred.ValorDocumento] = boleto.ValorDocumento;
                regBoleto[CNAB400Remessa1UniCred.DataEmissao] = boleto.DataDocumento;

                regBoleto[CNAB400Remessa1UniCred.Instrucao1] = boleto.Instrucao1;
                regBoleto[CNAB400Remessa1UniCred.DataDesconto] = boleto.DataDesconto;
                regBoleto[CNAB400Remessa1UniCred.ValorDesconto] = boleto.ValorDesconto;
                regBoleto[CNAB400Remessa1UniCred.SacadoTipo] = sacado.Tipo;
                regBoleto[CNAB400Remessa1UniCred.SacadoDocumento] = sacado.DocumentoNumeros;
                regBoleto[CNAB400Remessa1UniCred.Sacado] = sacado.Sacado;
                regBoleto[CNAB400Remessa1UniCred.Endereco] = sacado.Endereco;
                regBoleto[CNAB400Remessa1UniCred.CEP] = sacado.CepNumeros;
                regBoleto[CNAB400Remessa1UniCred.Bairro] = sacado.Bairro;
                regBoleto[CNAB400Remessa1UniCred.Cidade] = sacado.Cidade;
                regBoleto[CNAB400Remessa1UniCred.UF] = sacado.UF;
                regBoleto[CNAB400Remessa1UniCred.Sequencia] = SequencialRegistro++;

                // adiciona o boleto convertido em registro
                AddBoleto(regBoleto, boleto);

                AddOpcionais(boleto);
            }

            regArqTrailer[CNAB400Trailer1UniCred.Sequencia] = SequencialRegistro;
            Add(regArqTrailer);

            // Gera o Texto de saida da forma padrão
            return this.Conteudo;
        }

        /// <summary>
        /// Processa as informações baseado no conteudo de um arquivo CNAB
        /// </summary>
        /// <param name="cData">Conteudo do arquivo</param>
        public override Layout Retorno(string cData)
        {
            throw new NotImplementedException();
        }
    }

     #region "Estruturas de Remessa"

     /// <summary>
     /// Header Geral do Arquivo 
     /// </summary>
     [RegLayout(@"^0", DateFormat6 = "ddMMyy", Upper = true)]
     public enum CNAB400HeaderUniCred
     {
         /// <summary>
         /// Tipo de Registro
         /// </summary>
         [RegFormat(RegType.P9, 1, Default = "0")] // 1
         Controle_Registro,

         /// <summary>
         /// tipo de operação - remessa
         /// </summary>
         [RegFormat(RegType.P9, 1, Default = "1")] // 2
         Operacao_Codigo,

         /// <summary>
         /// Identificação por extenso do movimento
         /// </summary>
         [RegFormat(RegType.PX, 7, Default = "REMESSA")] // 3
         Operacao_Literal,

         /// <summary>
         /// identificação do tipo de serviço
         /// </summary>
         [RegFormat(RegType.P9, 2, Default = "01")] // 10
         Servico_Codigo,

         /// <summary>
         /// identificação do tipo de serviço
         /// </summary>
         [RegFormat(RegType.PX, 15, Default = "COBRANCA")] // 12
         Servico_Leteral,

         /// <summary>
         /// Número da Conta Corrente
         /// </summary>
         [RegFormat(RegType.P9, 20)] // 27
         Conta,

         /// <summary>
         /// Nome da empresa
         /// </summary>
         [RegFormat(RegType.P9, 30)] // 47
         Empresa,

         /// <summary>
         /// Nº do banco na câmara de compensação (antigo 091)
         /// </summary>
         [RegFormat(RegType.P9, 3, Default= "136")] // 77
         Banco_Codigo,

         /// <summary>
         /// Nome do Banco por Extenso
         /// </summary>
         [RegFormat(RegType.PX, 15, Default = "UniCred")] // 80
         Banco_Nome,

         /// <summary>
         /// Data da Gravação do Arquivo
         /// </summary>
         [RegFormat(RegType.PD, 6)] //  95
         Data,

         [RegFormat(RegType.PX, 7)] // 101
         Branco1,

         /// <summary>
         /// Código do Parâmetro de Movimento da UNICRED
         /// </summary>
         [RegFormat(RegType.PX, 3)] // 108
         CodMovimento,

         [RegFormat(RegType.P9, 7)] // 111 
         SequenciaArquivo,

         /// <summary>
         /// Versão do sistema
         /// </summary>
         [RegFormat(RegType.PX, 277)] // 118
         Branco2,

         /// <summary>
         /// Número seqüencial do registro no arquivo
         /// </summary>
         [RegFormat(RegType.P9, 6)] // 395
         Sequencia

     }

     /// <summary>
     /// Estrutura de Remessa Bradesco
     /// Página 11
     /// </summary>
     [RegLayout(@"^1", DateFormat6 = "ddMMyy", Upper = true)]
     public enum CNAB400Remessa1UniCred
     {

        /// <summary>
        /// Identificação do Registro 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 1-1
        Registro,

        /// <summary>
        /// Agência do Beneficiário na UNICRED 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 2-6
        Agencia,

        /// <summary>
        /// Dígito da Agência 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 7-7
        DigitoAgencia,

        /// <summary>
        /// Conta Corrente 
        /// </summary>
        [RegFormat(RegType.P9, 12)] // 8-19
        Conta,

        /// <summary>
        /// Dígito da Conta Corrente 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 20-20
        ContaDigito,

        /// <summary>
        /// Zero 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 21-21
        Zero1,

        /// <summary>
        /// Código da Carteira 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 22-24
        Carteira,

        /// <summary>
        /// Número do Contrato 
        /// </summary>
        [RegFormat(RegType.P9, 13)] // 25-37
        Contrato,

        /// <summary>
        /// Nº Controle do Participante (Uso da empresa) 
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 38-62
        Controle,

        /// <summary>
        /// Código do Banco na Câmara de Compensação 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 63-65
        Banco,

        /// <summary>
        /// Zeros 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 66-67
        Zeros2,

        /// <summary>
        /// Identificação do Título no Banco (Nosso número no correspondente) 
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 68-82
        NossoNumero,

        /// <summary>
        /// Desconto Bonificação por dia 
        /// </summary>
        [RegFormat(RegType.PV, 10)] // 83-92
        Desconto,

        /// <summary>
        /// Filler 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 93-93
        Filler1,

        /// <summary>
        /// Brancos 
        /// </summary>
        [RegFormat(RegType.PX, 12)] // 94-105
        Filler2,

        /// <summary>
        /// Zeros 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 106-106
        Filler3,

        /// <summary>
        /// Branco 
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 107-108
        Branco1,

        /// <summary>
        /// Identificação ocorrência 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 109-110
        Ocorrencia,

        /// <summary>
        /// Nº do Documento (Seu número) 
        /// </summary>
        [RegFormat(RegType.P9, 10)] // 111-120
        NumeroDocumento,

        /// <summary>
        /// Data do Vencimento do Título 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 121-126
        DataVencimento,

        /// <summary>
        /// Valor do Título 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 127-139
        ValorDocumento,

        /// <summary>
        /// Filler 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 140-142
        Filler4,

        /// <summary>
        /// Agência Depositária 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 143-147
        AgenciaDep,

        /// <summary>
        /// Filler 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 148-149
        Filler5,

        /// <summary>
        /// Filler 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 150-150
        Filler6,

        /// <summary>
        /// Data da emissão do Título 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 151-156
        DataEmissao,

        /// <summary>
        /// 1ª instrução 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 157-158
        Instrucao1,

        /// <summary>
        /// 2ª instrução 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 159-160
        Instrucao2,

        /// <summary>
        /// Valor a ser cobrado por dia de atraso 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 161-173
        ValorMora,

        /// <summary>
        /// Data Limite P/Concessão de Desconto 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 174-179
        DataDesconto,

        /// <summary>
        /// Valor do Desconto 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 180-192
        ValorDesconto,

        /// <summary>
        /// Nosso Número na UNICRED 
        /// </summary>
        [RegFormat(RegType.P9, 11)] // 193-203
        NossoNumero2,

        /// <summary>
        /// Zeros 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 204-205
        Zeros4,

        /// <summary>
        /// Valor do Abatimento a ser concedido 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 206-218
        ValorAbatimento,

        /// <summary>
        /// Identificação do Tipo de Inscrição do Pagador 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 219-220
        SacadoTipo,

        /// <summary>
        /// Nº Inscrição do Pagador 
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 221-234
        SacadoDocumento,

        /// <summary>
        /// Nome do Pagador 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 235-274
        Sacado,

        /// <summary>
        /// Endereço Completo 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 275-314
        Endereco,

        /// <summary>
        /// Bairro do Pagador 
        /// </summary>
        [RegFormat(RegType.PX, 12)] // 315-326
        Bairro,

        /// <summary>
        /// CEP do Pagador 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 327-334
        CEP,

        /// <summary>
        /// Cidade do Pagador 
        /// </summary>
        [RegFormat(RegType.PX, 20)] // 335-354
        Cidade,

        /// <summary>
        /// Estado do Pagador 
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 355-356
        UF,

        /// <summary>
        /// Pagador/Avalista 
        /// </summary>
        [RegFormat(RegType.PX, 38)] // 357-394
        Avalista,

        /// <summary>
        /// Nº Sequencial do Registro 006 Nº Sequencial do Registro 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395-400
        Sequencia

    }

    public enum CNAB400Trailer1UniCred
    {
         /// <summary>
         /// Identificação do registro trailer
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="9")] // 1-1
         Identificador,

         [RegFormat(RegType.PX, 393)]
         Branco,

         /// <summary>
         /// Número seqüencial do registro no arquivo
         /// </summary>
         [RegFormat(RegType.P9, 6)] // 395
         Sequencia
    }
    

     #endregion
}
