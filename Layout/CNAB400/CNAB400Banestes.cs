using System;
using Impactro.Cobranca;
using System.Runtime.InteropServices;

// Baseado no Bradesco Cobrança 07 (26/08/09)
// Inserido rotina de retorno 03/09/2012
namespace Impactro.Layout
{
    /// <summary>
    /// Gera o CNAB400 de Remessa de acordo com os padrões do Bradesco
    /// Para facilitar o suporte e padronizar recursos e funcionalidades cada banco tera sua proprie classe e rotinas de geração
    /// </summary>
    [ComVisible(false)]
    public class CNAB400Banestes : CNAB400<CNAB400Header1Banestes, CNAB400Remessa1Banestes, CNAB400ArquivoTrailer>
    {

        /// <summary>
        /// Gera o aquivo baseado nas coleção de todas informações passada até o momento
        /// </summary>
        /// <returns></returns>
        public override string Remessa()
        {

            string[] cCCDig = Cedente.Conta.Split('-');

            regArqHeader[CNAB400Header1Banestes.CodCedente] = Cedente.CodCedente;
            regArqHeader[CNAB400Header1Banestes.Empresa] = Cedente.Cedente;
            regArqHeader[CNAB400Header1Banestes.Geracao] = DataHoje;

            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // o sequencial do header é sempre 1 (FIXO)
            Add(regArqHeader);

            // Proximo item
            SequencialRegistro = 2;

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB400Remessa1Banestes> regBoleto;

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                regBoleto = new Reg<CNAB400Remessa1Banestes>();

                regBoleto[CNAB400Remessa1Banestes.TipoDOC] = Cedente.Tipo;
                regBoleto[CNAB400Remessa1Banestes.DOC] = Cedente.DocumentoNumeros;
                regBoleto[CNAB400Remessa1Banestes.CodCedente] = Cedente.CodCedente;

                if (boleto.BoletoID > 0)
                    regBoleto[CNAB400Remessa1Banestes.UsoEmpresa] = boleto.BoletoID;

                string cDAC12 = Banco_Banestes.NossoNumero(ref boleto.NossoNumero);
                regBoleto[CNAB400Remessa1Banestes.NossoNumero] = boleto.NossoNumero + cDAC12;

                //if(boleto.ParcelaNumero>0 && boleto.ParcelaTotal>0)
                //{
                //    regBoleto[CNAB400Remessa1Banestes.Carne] = "A1";
                //    regBoleto[CNAB400Remessa1Banestes.CarneNumParcela] = boleto.ParcelaNumero;
                //    regBoleto[CNAB400Remessa1Banestes.CarneQtdParcela] = boleto.ParcelaTotal;
                //}
                regBoleto[CNAB400Remessa1Banestes.Avalista_Tipo] = sacado.AvalistaTipo;
                regBoleto[CNAB400Remessa1Banestes.Avalista_Inscricao] = sacado.AvalistaNumeros;
                regBoleto[CNAB400Remessa1Banestes.Vencimento] = boleto.DataVencimento;
                regBoleto[CNAB400Remessa1Banestes.ValorDocumento] = boleto.ValorDocumento;
                regBoleto[CNAB400Remessa1Banestes.Especie] = (int)boleto.Especie; // 01-Duplicata Mercantil
                regBoleto[CNAB400Remessa1Banestes.Aceite] = boleto.Aceite; // N padrão
                regBoleto[CNAB400Remessa1Banestes.Emissao] = boleto.DataDocumento;
                regBoleto[CNAB400Remessa1Banestes.Instrucao1] = boleto.Instrucao1;
                regBoleto[CNAB400Remessa1Banestes.Instrucao2] = boleto.Instrucao2;
               // regBoleto[CNAB400Remessa1Banestes.Juros] = boleto.ValorMora;
                regBoleto[CNAB400Remessa1Banestes.DescontoData] = boleto.DataDesconto;
                regBoleto[CNAB400Remessa1Banestes.DescontoValor] = boleto.ValorDesconto;
                regBoleto[CNAB400Remessa1Banestes.Sacado_Tipo] = sacado.Tipo;
                regBoleto[CNAB400Remessa1Banestes.Sacado_Inscricao] = sacado.DocumentoNumeros;
                regBoleto[CNAB400Remessa1Banestes.Nome] = sacado.Sacado;
                regBoleto[CNAB400Remessa1Banestes.Endereco] = sacado.Endereco;
                regBoleto[CNAB400Remessa1Banestes.CEP] = sacado.CepNumeros;
                regBoleto[CNAB400Remessa1Banestes.Bairro] = sacado.Bairro;
                regBoleto[CNAB400Remessa1Banestes.Cidade] = sacado.Cidade;
                regBoleto[CNAB400Remessa1Banestes.Estado] = sacado.UF;
                regBoleto[CNAB400Remessa1Banestes.Sequencia] = SequencialRegistro++;

                // adiciona o boleto convertido em registro
                AddBoleto(regBoleto, boleto);

                AddOpcionais(boleto);
            }

            regArqTrailer[CNAB400ArquivoTrailer.Sequencia] = SequencialRegistro;
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
            Layout retorno = new Layout(typeof(CNAB400Retorno1Banestes));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            retorno.ForEach<CNAB400Retorno1Banestes>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    DataDocumento = (DateTime)reg[CNAB400Retorno1Banestes.OcorrenciaData],
                    NossoNumero = (string)reg[CNAB400Retorno1Banestes.NossoNumero],
                    NumeroDocumento = (string)reg[CNAB400Retorno1Banestes.NumeroDocumento],
                    ValorDocumento = (double)reg[CNAB400Retorno1Banestes.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB400Retorno1Banestes.Vencimento],
                    DataPagamento = (DateTime)reg[CNAB400Retorno1Banestes.DataPagamento],
                    ValorPago = (double)reg[CNAB400Retorno1Banestes.ValorPago]
                }, reg.OriginalLine)
            );
            return retorno;
        }
    }

    #region "Estruturas de Remessa"

    [RegLayout(@"^01", DateFormat6 = "ddMMyy")]
    public enum CNAB400Header1Banestes
    {
        /// <summary>
        /// IDENTIFICAÇÃO DO REGISTRO HEADER 
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "0")] // 1-1
        Tipo,

        /// <summary>
        /// TIPO DE OPERAÇÃO - REMESSA 
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 2-2
        Operacao,

        /// <summary>
        /// IDENTIFICAÇÃO POR EXTENSO DO MOVIMENTO 
        /// </summary>
        [RegFormat(RegType.PX, 7, Default = "REMESSA")] // 3-9
        Remessa,

        /// <summary>
        /// IDENTIFICAÇÃO DO TIPO DE SERVIÇO 
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "1")] // 10-11
        CodServ,

        /// <summary>
        /// IDENTIFICAÇÃO POR EXTENSO DO TIPO DE SERVIÇO 
        /// </summary>
        [RegFormat(RegType.PX, 15, Default = "COBRANCA")] // 12-26
        Cobranca,

        /// <summary>
        /// Identificação da Empresa no Banestes
        /// </summary>
        [RegFormat(RegType.P9, 11)] // 27-37
        CodCedente,

        /// <summary>
        /// COMPLEMENTO DE REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 9)] // 38-46
        BRANCOS1,

        /// <summary>
        /// NOME POR EXTENSO DA EMPRESA MÃE 
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 47-76
        Empresa,

        /// <summary>
        /// Nº DO BANCO NA CÂMARA DE COMPENSAÇÃO 
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "021")] // 77-79
        CodigoBanco,

        /// <summary>
        /// NOME POR EXTENSO DO BANCO COBRADOR 
        /// </summary>
        [RegFormat(RegType.PX, 15, Default = "BANESTES")] // 80-94
        NomeBanco,

        /// <summary>
        /// DATA DE GERAÇÃO DO ARQUIVO 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 95-100
        Geracao,

        /// <summary>
        /// COMPLEMENTO DO REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 294)] // 101-394
        BRANCOS2,

        /// <summary>
        /// NÚMERO SEQÜENCIAL DO REGISTRO NO ARQUIVO 
        /// </summary>
        [RegFormat(RegType.P9, 6, Default = "1")] // 395-400
        Sequencia

    }

    /// <summary>
    /// Estrutura de Remessa Bradesco
    /// Página 11
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy", Upper = true)]
    public enum CNAB400Remessa1Banestes
    {
        /// <summary>
        /// IDENTIFICAÇÃO DO REGISTRO TRANSAÇÃO 
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 1-1
        Tipo,

        /// <summary>
        /// TIPO DE INSCRIÇÃO DA EMPRESA 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 2-3
        TipoDOC,

        /// <summary>
        /// Nº DE INSCRIÇÃO DA EMPRESA (CPF/CNPJ) 
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 4-17
        DOC,

        /// <summary>
        /// AGÊNCIA MANTENEDORA DA CONTA 
        /// </summary>
        [RegFormat(RegType.P9, 11)] // 18-28
        CodCedente,

        /// <summary>
        /// COMPLEMENTO DE REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 9)] // 29-37
        Brancos1,

        /// <summary>
        /// IDENTIFICAÇÃO DO TÍTULO NA EMPRESA 
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 38-62
        UsoEmpresa,

        /// <summary>
        /// IDENTIFICAÇÃO DO TÍTULO NO BANCO 
        /// </summary>
        [RegFormat(RegType.P9, 10)] // 63-72
        NossoNumero,

        /// <summary>
        /// Se for =1 siguinifica que o valor da multa foi especificado
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 73-73
        CodigoMulta,

        /// <summary>
        /// Valor da Multa
        /// </summary>
        [RegFormat(RegType.PV, 9)] // 74-82
        ValorMulta,

        /// <summary>
        /// Tipo de carne
        /// </summary>
        [RegFormat(RegType.PX, 6)] // 83-88
        Carne,

        /// <summary>
        /// Número da parcela atual
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 89-90
        CarneNumParcela,

        /// <summary>
        /// Número total de parcelas
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 91-92
        CarneQtdParcela,

        /// <summary>
        /// Tipo do Avalista: 1-CPF, 2-CNPJ
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 93-93
        Avalista_Tipo,

        /// <summary>
        /// Documento Avalista
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 94-107
        Avalista_Inscricao,

        /// <summary>
        /// 1: Cobrança Simles, 2: Cobrança Calcionada
        /// </summary>
        [RegFormat(RegType.P9, 1, Default ="1")] // 108-108
        Carteira,

        /// <summary>
        /// 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 109-110
        Ocorrencia,

        /// <summary>
        /// Duplicada, Fatura
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 111-120
        NumeroFatura,

        /// <summary>
        /// Vencimento
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 121-126
        Vencimento,

        /// <summary>
        /// VALOR NOMINAL DO TÍTULO 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 127-139
        ValorDocumento,

        /// <summary>
        /// Nº DO BANCO NA CÂMARA DE COMPENSAÇÃO 
        /// </summary>
        [RegFormat(RegType.P9, 3, Default ="021")] // 140-142
        Banco,

        /// <summary>
        /// AGÊNCIA ONDE O TÍTULO SERÁ COBRADO 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 143-147
        AgenciaCob,

        /// <summary>
        /// ESPÉCIE DO TÍTULO 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 148-149
        Especie,

        /// <summary>
        /// IDENTIFICAÇÃO DE TÍTULO ACEITO OU NÃO ACEITO 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 150-150
        Aceite,

        /// <summary>
        /// DATA DA EMISSÃO DO TÍTULO 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 151-156
        Emissao,

        /// <summary>
        /// 1ª INSTRUÇÃO DE COBRANÇA 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 157-158
        Instrucao1,

        /// <summary>
        /// 2ª INSTRUÇÃO DE COBRANÇA 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 159-160
        Instrucao2,

        /// <summary>
        /// VALOR DE MORA POR DIA DE ATRASO 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 161-173
        CodigoMora,

        /// <summary>
        /// VALOR DE MORA POR DIA DE ATRASO 
        /// </summary>
        [RegFormat(RegType.PV, 12)] // 161-173
        ValorMora,

        /// <summary>
        /// DATA LIMITE PARA CONCESSÃO DE DESCONTO 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 174-179
        DescontoData,

        /// <summary>
        /// VALOR DO DESCONTO A SER CONCEDIDO 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 180-192
        DescontoValor,

        /// <summary>
        /// VALOR DO I.O.F. RECOLHIDO P/ NOTAS SEGURO 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 193-205
        IOF,

        /// <summary>
        /// VALOR DO ABATIMENTO A SER CONCEDIDO 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 206-218
        Abatimento,

        /// <summary>
        /// IDENTIFICAÇÃO DO TIPO DE INSCRIÇÃO/PAGADOR 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 219-220
        Sacado_Tipo,

        /// <summary>
        /// Nº DE INSCRIÇÃO DO PAGADOR (CPF/CNPJ) 
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 221-234
        Sacado_Inscricao,

        /// <summary>
        /// NOME DO PAGADOR 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 235-274
        Nome,
        
        /// <summary>
        /// NÚMERO E COMPLEMENTO DO PAGADOR 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 275-314
        Endereco,

        /// <summary>
        /// BAIRRO DO PAGADOR 
        /// </summary>
        [RegFormat(RegType.PX, 12)] // 315-326
        Bairro,

        /// <summary>
        /// CEP DO PAGADOR 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 327-334
        CEP,

        /// <summary>
        /// CIDADE DO PAGADOR 
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 335-349
        Cidade,

        /// <summary>
        /// UF DO PAGADOR 
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 350-351
        Estado,

        /// <summary>
        /// Mensagem a ser impressa no campo instrução
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 352-391
        Mensagem,

        /// <summary>
        /// COMPLEMENTO DO REGISTRO 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 392-393
        Filler,

        /// <summary>
        /// COMPLEMENTO DO REGISTRO 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 394-394
        Moeda,

        /// <summary>
        /// Nº SEQÜENCIAL DO REGISTRO NO ARQUIVO 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395-400
        Sequencia
    }


    /// <summary>
    /// Estrutura de Retorno Bradesco
    /// Página 42
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy")]
    public enum CNAB400Retorno1Banestes
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 1
        Controle_Registro,

        /// <summary>
        /// Tipo de Inscrição Empresa: 01-CPF, 02-CNPJ, 03-PIS/PASEP, 98-Não tem, 99-Outros
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 2
        Inscricao_Tipo,

        /// <summary>
        /// Nº Inscrição da Empresa: CNPJ/CPF Número Filial Controle
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 4
        Inscricao_Numero,

        /// <summary>
        /// AGÊNCIA MANTENEDORA DA CONTA
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 18
        Agencia,

        [RegFormat(RegType.PX, 2)] // 22
        Zeros1,

        /// <summary>
        /// NÚMERO DA CONTA CORRENTE DA EMPRESA
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 24
        Conta,

        /// <summary>
        /// DÍGITO DE AUTO CONFERÊNCIA AG/CONTA EMPRESA
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 29
        DAC,

        [RegFormat(RegType.PX, 8)] // 30
        Brancos1,

        /// <summary>
        /// IDENTIFICAÇÃO DO TÍTULO NA EMPRESA
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 38
        Identificacao,

        /// <summary>
        /// Identificação do Título no Banco
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 63
        NossoNumero1,

        [RegFormat(RegType.PX, 12)] // 71
        Brancos2,

        /// <summary>
        /// NUMERO DA CARTEIRA
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 83
        Carteira1,

        /// <summary>
        /// IDENTIFICAÇÃO DO TÍTULO NO BANCO
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 86
        NossoNumero2,

        /// <summary>
        /// Identificação do Título no Banco
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 94
        DACNossoNumero,

        [RegFormat(RegType.PX, 13)] // 95
        Brancos3,

        // <summary>
        /// CÓDIGO DA CARTEIRA
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 108
        Carteira2,

        /// <summary>
        /// Identificação de Ocorrência
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 109
        Ocorrencia,

        /// <summary>
        /// Data Ocorrência no Banco
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 111
        OcorrenciaData,

        /// <summary>
        /// Identificação do Título no Banco 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 117
        NumeroDocumento,

        /// <summary>
        /// Identificação do Título no Banco
        /// </summary>
        [RegFormat(RegType.PX, 8)] // 127
        NossoNumero,

        [RegFormat(RegType.PX, 12)] // 135
        Brancos4,

        /// <summary>
        /// Data Vencimento do Título
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 147
        Vencimento,

        /// <summary>
        /// Valor do Título
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 153
        ValorDocumento,

        /// <summary>
        /// Banco Cobrador
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 166
        CobradorBanco,

        /// <summary>
        /// Código da Agência do Banco Cobrador
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 169
        CobradorAgencia,

        /// <summary>
        /// DAC Agencia cobradora
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 173
        AgenciaDAC,

        /// <summary>
        /// Espécie do Título
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 174
        Especie,

        /// <summary>
        /// Valor Despesa
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 176
        ValorDespesas,

        /// <summary>
        /// Valor outras despesas
        /// </summary>
        [RegFormat(RegType.PX, 26)] // 189
        Brancos5,

        /// <summary>
        /// IOF Devido
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 215
        IOF,

        /// <summary>
        /// Abatimento Concedido sobre o Título
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 228
        Abatimento,

        /// <summary>
        /// Desconto Concedido
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 241
        Descontos,

        /// <summary>
        /// Valor Pago
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 254
        ValorPago,

        /// <summary>
        /// Juros de Mora
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 267
        JurosMora,

        /// <summary>
        /// Outros Créditos
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 280
        CreditoOutros,

        /// <summary>
        /// INDICADOR DE BOLETO DDA
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 293
        DDA,

        [RegFormat(RegType.PX, 2)] // 294
        Brancos6,

        /// <summary>
        /// Data do Crédito
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 296
        DataPagamento,

        /// <summary>
        /// CÓDIGO DA INSTRUÇÃO CANCELADA
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 302
        InstrucaoCancelada,

        [RegFormat(RegType.PX, 6)] // 306
        Brancos7,

        [RegFormat(RegType.P9, 13)] // 312
        Zeros2,

        /// <summary>
        /// Nome do pagador
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 325
        NomePagador,

        [RegFormat(RegType.PX, 23)] // 355
        Brancos8,

        /// <summary>
        /// REGISTROS REJEITADOS OU ALEGAÇÃO DO PAGADOR OU REGISTRO DE MENSAGEM INFORMATIVA
        /// </summary>
        [RegFormat(RegType.PX, 8)] // 378
        Erro,

        [RegFormat(RegType.PX, 7)] // 386
        Brancos9,

        /// <summary>
        /// MEIO PELO QUAL O TÍTULO FOI LIQUIDADO
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 393
        CodigoLiquidacao,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia

    }

    #endregion

}