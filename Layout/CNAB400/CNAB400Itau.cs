using System;
using Impactro.Cobranca;
using System.Runtime.InteropServices;

// Baseado no Bradesco Cobrança 07 (26/08/09)
// Inserido rotina de retorno 03/09/2012
namespace Impactro.Layout
{
    /// <summary>
    /// Gera o CNAB400 de Remessa de acordo com os padrões do Itaú
    /// Para facilitar o suporte e padronizar recursos e funcionalidades cada banco tera sua proprie classe e rotinas de geração
    /// </summary>
    [ComVisible(false)]
    public class CNAB400Itau : CNAB400<CNAB400Header1Itau, CNAB400Remessa1Itau, CNAB400ArquivoTrailer>
    {

        /// <summary>
        /// Gera o aquivo baseado nas coleção de todas informações passada até o momento
        /// </summary>
        /// <returns></returns>
        public override string Remessa()
        {

            string[] cAGDig = Cedente.Agencia.Split('-'); // Apenas para proteção e padronização pois ITAU Não tem controle de digito
            string[] cCCDig = Cedente.Conta.Split('-');

            regArqHeader[CNAB400Header1Itau.Agencia] = cAGDig[0];
            regArqHeader[CNAB400Header1Itau.Conta] = cCCDig[0];
            regArqHeader[CNAB400Header1Itau.DAC] = cCCDig[1];
            regArqHeader[CNAB400Header1Itau.Empresa] = Cedente.Cedente;
            regArqHeader[CNAB400Header1Itau.Geracao] = DataHoje;

            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // o sequencial do header é sempre 1 (FIXO)
            Add(regArqHeader);

            // Proximo item
            SequencialRegistro = 2;

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB400Remessa1Itau> regBoleto;

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                regBoleto = new Reg<CNAB400Remessa1Itau>();

                regBoleto[CNAB400Remessa1Itau.Carteira] = Cedente.Carteira;
                if (Cedente.Carteira == "150")
                    regBoleto[CNAB400Remessa1Itau.CarteiraTipo] = "U";
                else if (Cedente.Carteira == "191")
                    regBoleto[CNAB400Remessa1Itau.CarteiraTipo] = "1";
                else if (Cedente.Carteira == "147")
                    regBoleto[CNAB400Remessa1Itau.CarteiraTipo] = "E";
                else
                    regBoleto[CNAB400Remessa1Itau.CarteiraTipo] = "I";

                regBoleto[CNAB400Remessa1Itau.TipoDOC] = Cedente.Tipo;
                regBoleto[CNAB400Remessa1Itau.DOC] = Cedente.DocumentoNumeros;
                regBoleto[CNAB400Remessa1Itau.Agencia] = cAGDig[0];
                regBoleto[CNAB400Remessa1Itau.Conta] = cCCDig[0];
                regBoleto[CNAB400Remessa1Itau.DAC] = cCCDig[1];

                if (boleto.BoletoID > 0)
                    regBoleto[CNAB400Remessa1Itau.UsoEmpresa] = boleto.BoletoID;

                regBoleto[CNAB400Remessa1Itau.NossoNumero] = boleto.NossoNumero;

                regBoleto[CNAB400Remessa1Itau.Ocorrencia] = (int)boleto.Ocorrencia; // 01-Remessa 
                regBoleto[CNAB400Remessa1Itau.NumeroDocumento] = boleto.NumeroDocumento; // X(10)
                regBoleto[CNAB400Remessa1Itau.Vencimento] = boleto.DataVencimento;
                regBoleto[CNAB400Remessa1Itau.ValorDocumento] = boleto.ValorDocumento;
                regBoleto[CNAB400Remessa1Itau.Especie] = (int)boleto.Especie; // 01-Duplicata Mercantil
                regBoleto[CNAB400Remessa1Itau.Aceite] = boleto.Aceite; // N padrão
                regBoleto[CNAB400Remessa1Itau.Emissao] = boleto.DataDocumento;
                regBoleto[CNAB400Remessa1Itau.Instrucao1] = boleto.Instrucao1;
                regBoleto[CNAB400Remessa1Itau.Instrucao2] = boleto.Instrucao2;
                regBoleto[CNAB400Remessa1Itau.Juros] = boleto.ValorMora;
                regBoleto[CNAB400Remessa1Itau.DescontoData] = boleto.DataDesconto;
                regBoleto[CNAB400Remessa1Itau.DescontoValor] = boleto.ValorDesconto;
                regBoleto[CNAB400Remessa1Itau.Sacado_Tipo] = sacado.Tipo;
                regBoleto[CNAB400Remessa1Itau.Sacado_Inscricao] = sacado.DocumentoNumeros;
                regBoleto[CNAB400Remessa1Itau.Nome] = sacado.Sacado;
                regBoleto[CNAB400Remessa1Itau.Endereco] = sacado.Endereco;
                regBoleto[CNAB400Remessa1Itau.CEP] = sacado.CepNumeros;
                regBoleto[CNAB400Remessa1Itau.Bairro] = sacado.Bairro;
                regBoleto[CNAB400Remessa1Itau.Cidade] = sacado.Cidade;
                regBoleto[CNAB400Remessa1Itau.Estado] = sacado.UF;
                regBoleto[CNAB400Remessa1Itau.Sequencia] = SequencialRegistro++;

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
            Layout retorno = new Layout(typeof(CNAB400Retorno1Itau));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            retorno.ForEach<CNAB400Retorno1Itau>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    DataDocumento = (DateTime)reg[CNAB400Retorno1Itau.OcorrenciaData],
                    NossoNumero = (string)reg[CNAB400Retorno1Itau.NossoNumero],
                    NumeroDocumento = (string)reg[CNAB400Retorno1Itau.NumeroDocumento],
                    ValorDocumento = (double)reg[CNAB400Retorno1Itau.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB400Retorno1Itau.Vencimento],
                    DataPagamento = (DateTime)reg[CNAB400Retorno1Itau.DataPagamento],
                    ValorPago = (double)reg[CNAB400Retorno1Itau.ValorPago] + (double)reg[CNAB400Retorno1Itau.ValorDespesas],
                    ValorTarifa = (double)reg[CNAB400Retorno1Itau.ValorDespesas],
                    SetOcorrenciaRetorno = (int)reg[CNAB400Retorno1Itau.Ocorrencia] // Tradução direta: Supondo 100% de compatibilidade
                }, reg.OriginalLine)
            );
            return retorno;
        }
    }

    #region "Estruturas de Remessa"

    [RegLayout(@"^01", DateFormat6 = "ddMMyy")]
    public enum CNAB400Header1Itau
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
        /// AGÊNCIA MANTENEDORA DA CONTA 
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 27-30
        Agencia,

        /// <summary>
        /// COMPLEMENTO DE REGISTRO 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 31-32
        ZEROS,

        /// <summary>
        /// NÚMERO DA CONTA CORRENTE DA EMPRESA 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 33-37
        Conta,

        /// <summary>
        /// DÍGITO DE AUTO CONFERÊNCIA AG/CONTA EMPRESA 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 38-38
        DAC,

        /// <summary>
        /// COMPLEMENTO DO REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 8)] // 39-46
        BRANCOS1,

        /// <summary>
        /// NOME POR EXTENSO DA EMPRESA MÃE 
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 47-76
        Empresa,

        /// <summary>
        /// Nº DO BANCO NA CÂMARA DE COMPENSAÇÃO 
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "341")] // 77-79
        CodigoBanco,

        /// <summary>
        /// NOME POR EXTENSO DO BANCO COBRADOR 
        /// </summary>
        [RegFormat(RegType.PX, 15, Default = "BANCO ITAU SA")] // 80-94
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
    /// Estrutura de Remessa Itaú
    /// Página 11
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy", Upper = true)]
    public enum CNAB400Remessa1Itau
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
        [RegFormat(RegType.P9, 4)] // 18-21
        Agencia,

        /// <summary>
        /// COMPLEMENTO DE REGISTRO 
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "0")] // 22-23
        ZEROS,

        /// <summary>
        /// NÚMERO DA CONTA CORRENTE DA EMPRESA 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 24-28
        Conta,

        /// <summary>
        /// DÍGITO DE AUTO CONFERÊNCIA AG/CONTA EMPRESA 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 29-29
        DAC,

        /// <summary>
        /// COMPLEMENTO DE REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 4)] // 30-33
        BRANCOS1,

        /// <summary>
        /// CÓD.INSTRUÇÃO/ALEGAÇÃO A SER CANCELADA 
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 34-37
        Instrucao,

        /// <summary>
        /// IDENTIFICAÇÃO DO TÍTULO NA EMPRESA 
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 38-62
        UsoEmpresa,

        /// <summary>
        /// IDENTIFICAÇÃO DO TÍTULO NO BANCO 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 63-70
        NossoNumero,

        /// <summary>
        /// QUANTIDADE DE MOEDA VARIÁVEL 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 71-83
        MoedaVariavel,

        /// <summary>
        /// NÚMERO DA CARTEIRA NO BANCO 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 84-86
        Carteira,

        /// <summary>
        /// IDENTIFICAÇÃO DA OPERAÇÃO NO BANCO 
        /// </summary>
        [RegFormat(RegType.PX, 21)] // 87-107
        USOBANCO,

        /// <summary>
        /// CÓDIGO DA CARTEIRA 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 108-108
        CarteiraTipo,

        /// <summary>
        /// IDENTIFICAÇÃO DA OCORRÊNCIA 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 109-110
        Ocorrencia,

        /// <summary>
        /// Nº DO DOCUMENTO DE COBRANÇA (DUPL.,NP ETC.) 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 111-120
        NumeroDocumento,

        /// <summary>
        /// DATA DE VENCIMENTO DO TÍTULO 
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
        [RegFormat(RegType.P9, 3)] // 140-142
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
        [RegFormat(RegType.PV, 13)] // 161-173
        Juros,

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
        [RegFormat(RegType.PX, 30)] // 235-264
        Nome,

        /// <summary>
        /// COMPLEMENTO DE REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 265-274
        BRANCOS2,

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
        /// NOME DO SACADOR OU AVALISTA 
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 352-381
        Avalista,

        /// <summary>
        /// COMPLEMENTO DO REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 4)] // 382-385
        BRANCOS3,

        /// <summary>
        /// DATA DE MORA 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 386-391
        DataMora,

        /// <summary>
        /// QUANTIDADE DE DIAS 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 392-393
        Prazo,

        /// <summary>
        /// COMPLEMENTO DO REGISTRO 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 394-394
        BRANCOS4,

        /// <summary>
        /// Nº SEQÜENCIAL DO REGISTRO NO ARQUIVO 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395-400
        Sequencia
    }


    /// <summary>
    /// Estrutura de Retorno Itau
    /// Página 42
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy")]
    public enum CNAB400Retorno1Itau
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