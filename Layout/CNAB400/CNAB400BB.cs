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
    public class CNAB400BB : CNAB400<CNAB400Header1BB, CNAB400Remessa1BB, CNAB400ArquivoTrailer>
    {

        /// <summary>
        /// Gera o aquivo baseado nas coleção de todas informações passada até o momento
        /// </summary>
        /// <returns></returns>
        public override string Remessa()
        {

            string[] cAgDig = Cedente.Agencia.Split('-');
            string[] cCCDig = Cedente.Conta.Split('-');
            string[] cBanco = Cedente.Banco.Split('-');

            Bancos banco = (Bancos)CobUtil.GetInt(cBanco[0]);

            if (banco != Bancos.BANCO_DO_BRASIL)
                throw new Exception("Esta classe é valida apenas para o Banco do Brasil");
            else if(cAgDig.Length!=2)
                throw new Exception("Informe a agência e digito no formato 9999-9");
            else if (cCCDig.Length != 2)
                throw new Exception("Informe a conta e digito no formato 99999999-9");

            // Proximo item
            SequencialRegistro = 1;

            regArqHeader[CNAB400Header1BB.Agencia] = cAgDig[0];
            regArqHeader[CNAB400Header1BB.AgenciaDV] = cAgDig[1];
            regArqHeader[CNAB400Header1BB.ContaCorrente] = cCCDig[0];
            regArqHeader[CNAB400Header1BB.ContaCorrenteDV] = cCCDig[1];
            regArqHeader[CNAB400Header1BB.Cedente] = Cedente.Cedente;
            regArqHeader[CNAB400Header1BB.Data] = DataHoje;
            regArqHeader[CNAB400Header1BB.Remessa] = NumeroLote;
            regArqHeader[CNAB400Header1BB.ConvenioLider] = Cedente.Convenio;
            regArqHeader[CNAB400Header1BB.Sequencia] = SequencialRegistro++;
            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // atualiza o lote
            //regArqHeader[CNAB400Header1Bradesco.Lote] = NumeroLote;

            // o sequencial do header é sempre 1 (FIXO)
            Add(regArqHeader);

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB400Remessa1BB> regBoleto;

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                regBoleto = new Reg<CNAB400Remessa1BB>();

                regBoleto[CNAB400Remessa1BB.CedenteTipo] = Cedente.Tipo;
                regBoleto[CNAB400Remessa1BB.Documento] = Cedente.DocumentoNumeros;
                regBoleto[CNAB400Remessa1BB.Agencia] = cAgDig[0];
                regBoleto[CNAB400Remessa1BB.AgenciaDV] = cAgDig[1];
                regBoleto[CNAB400Remessa1BB.ContaCorrente] = cCCDig[0];
                regBoleto[CNAB400Remessa1BB.ContaCorrenteDV] = cCCDig[1];
                regBoleto[CNAB400Remessa1BB.Convenio] = Cedente.Convenio;
                regBoleto[CNAB400Remessa1BB.EmpresaCodigo] = sacado.SacadoCodigo;
                regBoleto[CNAB400Remessa1BB.NossoNumero] = Banco_do_Brasil.NossoNumero(Cedente.Convenio, Cedente.Modalidade, Cedente.Carteira, boleto.NossoNumero);
                regBoleto[CNAB400Remessa1BB.Indicativo] = " "; // TODO: Fazer indicativo (informações adicionais de sacador/avalista)
                regBoleto[CNAB400Remessa1BB.Modalidade] = Cedente.Modalidade;
                regBoleto[CNAB400Remessa1BB.Carteira] = Cedente.Carteira;
                regBoleto[CNAB400Remessa1BB.Comando] = boleto.Comando;
                regBoleto[CNAB400Remessa1BB.NumeroDocumento] = boleto.NumeroDocumento;
                regBoleto[CNAB400Remessa1BB.DataVencimento] = boleto.DataVencimento;
                regBoleto[CNAB400Remessa1BB.ValorDocumento] = boleto.ValorDocumento;
                regBoleto[CNAB400Remessa1BB.Especie] = (int)boleto.Especie;
                regBoleto[CNAB400Remessa1BB.Aceite] = boleto.Aceite;
                regBoleto[CNAB400Remessa1BB.DataDocumento] = boleto.DataDocumento;
                regBoleto[CNAB400Remessa1BB.Instrucao1] = boleto.Instrucao1;
                regBoleto[CNAB400Remessa1BB.Instrucao2] = boleto.Instrucao2;
                regBoleto[CNAB400Remessa1BB.JurosValor] = boleto.ValorMora;
                regBoleto[CNAB400Remessa1BB.DataDesconto] = boleto.DataDesconto;
                regBoleto[CNAB400Remessa1BB.ValorDesconto] = boleto.ValorDesconto;
                regBoleto[CNAB400Remessa1BB.SacadoTipo] = sacado.Tipo;
                regBoleto[CNAB400Remessa1BB.SacadoDocumento] = sacado.DocumentoNumeros;
                regBoleto[CNAB400Remessa1BB.Sacado] = sacado.Sacado;
                regBoleto[CNAB400Remessa1BB.Endereco] = sacado.Endereco;
                regBoleto[CNAB400Remessa1BB.Bairro] = sacado.Bairro;
                regBoleto[CNAB400Remessa1BB.CEP] = sacado.CepNumeros;
                regBoleto[CNAB400Remessa1BB.Cidade] = sacado.Cidade;
                regBoleto[CNAB400Remessa1BB.UF] = sacado.UF;
                regBoleto[CNAB400Remessa1BB.IndicativoValor] = ""; // TODO: valor de acordo com o indicativo
                if (boleto.DiasProtesto > 0)
                    regBoleto[CNAB400Remessa1BB.DiasProtesto] = boleto.DiasProtesto;

                regBoleto[CNAB400Remessa1BB.Sequencia] = SequencialRegistro++;

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
            Layout retorno = new Layout(typeof(CNAB400Retorno7BB));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            retorno.ForEach<CNAB400Retorno7BB>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    NossoNumero = (string)reg[CNAB400Retorno7BB.NossoNumero],
                    NumeroDocumento = (string)reg[CNAB400Retorno7BB.NumeroDocumento],
                    ValorDocumento = (double)reg[CNAB400Retorno7BB.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB400Retorno7BB.DataVencimento],
                    DataPagamento = (DateTime)reg[CNAB400Retorno7BB.DataPagamento]
                }, reg.OriginalLine)
            );
            return retorno;
        }
    }

    #region "Estruturas de Remessa"

    /// <summary>
    /// Header Geral do Arquivo 
    /// </summary>
    [RegLayout(@"^0", DateFormat8 = "ddMMyy", Upper = true)]
    public enum CNAB400Header1BB
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
        Servico_Codido,

        /// <summary>
        /// identificação do tipo de serviço
        /// </summary>
        [RegFormat(RegType.PX, 15, Default = "COBRANCA")] // 12
        Servico_Leteral,

        /// <summary>
        /// Prefixo da Agência: Número da Agência onde está cadastrado oconvênio líder do cedente
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 27
        Agencia,

        /// <summary>
        /// Dígito Verificador - D.V. - do Prefixo da Agência
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 31
        AgenciaDV,

        /// <summary>
        /// Número da Conta Corrente: Número da conta onde está cadastrado o Convênio Líder do Cedente
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 32
        ContaCorrente,

        /// <summary>
        /// Dígito Verificador - D.V. – do Número da Conta Corrente do Cedente
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 40
        ContaCorrenteDV,

        [RegFormat(RegType.P9, 6)] // 41
        Zeros,

        /// <summary>
        /// Nome do Cedente
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 47
        Cedente,

        /// <summary>
        /// Nº do banco na câmara de compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default="001")] // 77
        Banco_Codigo,

        /// <summary>
        /// Nome do Banco por Extenso
        /// </summary>
        [RegFormat(RegType.PX, 15, Default = "BANCODOBRASIL")] // 80
        Banco_Nome,

        /// <summary>
        /// Data da Gravação do Arquivo
        /// </summary>
        [RegFormat(RegType.PD, 6)] //  95
        Data,

        /// <summary>
        /// Seqüencial da Remessa
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 101
        Remessa,

        [RegFormat(RegType.PX, 22)] // 108
        Brancos1,

        /// <summary>
        /// Número do Convênio Líder (numeração acima de 1.000.000 um milhão)"
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 130
        ConvenioLider,

        [RegFormat(RegType.PX, 258)] // 137 
        Brancos2,

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
    public enum CNAB400Remessa1BB
    {
        /// <summary>
        /// Identificação do Registro Detalhe: 7 (sete) 
        /// </summary>
        [RegFormat(RegType.P9, 1, Default="7")] // 1-1
        Identificador,

        /// <summary>
        /// Tipo de Inscrição do Cedente #obs: 22 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 2-3
        CedenteTipo,

        /// <summary>
        /// Número do CPF/CNPJ do Cedente 
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 4-17
        Documento,

        /// <summary>
        /// Prefixo da Agência #obs:02 
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 18-21
        Agencia,

        /// <summary>
        /// Dígito Verificador - D.V. - do Prefixo da Agência #obs:02 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 22-22
        AgenciaDV,

        /// <summary>
        /// Número da Conta Corrente do Cedente #obs:02 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 23-30
        ContaCorrente,

        /// <summary>
        /// Dígito Verificador - D.V. - do Número da Conta Corrente do Cedente #obs:02 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 31-31
        ContaCorrenteDV,

        /// <summary>
        /// Número do Convênio de Cobrança do Cedente #obs:02 
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 32-38
        Convenio,

        /// <summary>
        /// Código de Controle da Empresa #obs:23 
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 39-63
        EmpresaCodigo,

        /// <summary>
        /// Nosso-Número #obs:06 
        /// </summary>
        [RegFormat(RegType.P9, 17)] // 64-80
        NossoNumero,

        /// <summary>
        /// Número da Prestação: 00 (Zeros) 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 81-82
        Zeros1,

        /// <summary>
        /// Grupo de Valor: 00 (Zeros) 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 83-84
        Zeros2,

        /// <summary>
        /// Complemento do Registro: Brancos 
        /// </summary>
        [RegFormat(RegType.PX, 3)] // 85-87
        Brancos1,

        /// <summary>
        /// Indicativo de Mensagem ou Sacador/Avalista #obs:13 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 88-88
        Indicativo,

        /// <summary>
        /// Prefixo do Título: Brancos 
        /// </summary>
        [RegFormat(RegType.PX, 3)] // 89-91
        Bracos2,

        /// <summary>
        /// Variação da Carteira #obs:02 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 92-94
        Modalidade,

        /// <summary>
        /// Conta Caução: 0 (Zero) 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 95-95
        Caucao,

        /// <summary>
        /// Número do Borderô: 000000 (Zeros) 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 96-101
        Bordero,

        /// <summary>
        /// Tipo de Cobrança #obs:24 
        /// </summary>
        [RegFormat(RegType.PX, 5)] // 102-106
        CarteiraTipo,

        /// <summary>
        /// Carteira de Cobrança #obs:25 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 107-108
        Carteira,

        /// <summary>
        /// Comando #obs:20 
        /// </summary>
        [RegFormat(RegType.P9, 2, Default="1")] // 109-110
        Comando,

        /// <summary>
        /// Seu Número/Número do Título Atribuído pelo Cedente 05 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 111-120
        NumeroDocumento,

        /// <summary>
        /// Data de Vencimento #obs:08 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 121-126
        DataVencimento,

        /// <summary>
        /// Valor do Título #obs:19 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 127-139
        ValorDocumento,

        /// <summary>
        /// Número do Banco: 001 
        /// </summary>
        [RegFormat(RegType.P9, 3, Default ="001")] // 140-142
        NumeroBanco,

        /// <summary>
        /// Prefixo da Agência Cobradora: 0000 #obs:26 
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 143-146
        AgenciaCobradora,

        /// <summary>
        /// Dígito Verificador do Prefixo da Agência Cobradora: Brancos 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 147-147
        AgenciaCobradoraDV,

        /// <summary>
        /// Espécie de Titulo #obs:07 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 148-149
        Especie,

        /// <summary>
        /// Aceite do Título: #obs:27 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 150-150
        Aceite,

        /// <summary>
        /// Data de Emissão: Informe no formato DDMMAA #obs:28 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 151-156
        DataDocumento,

        /// <summary>
        /// Instrução Codificada #obs:09 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 157-158
        Instrucao1,

        /// <summary>
        /// Instrução Codificada #obs:09 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 159-160
        Instrucao2,

        /// <summary>
        /// Juros de Mora por Dia de Atraso #obs:10 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 161-173
        JurosValor,

        /// <summary>
        /// Data Limite para Concessão de Desconto/Data de Operação do BBVendor/Juros de Mora. #obs:11 
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 174-179
        DataDesconto,

        /// <summary>
        /// Valor do Desconto #obs:29 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 180-192
        ValorDesconto,

        /// <summary>
        /// Valor do IOF/Qtde Unidade Variável. #obs:30 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 193-205
        ValorIOF,

        /// <summary>
        /// Valor do Abatimento #obs:31 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 206-218
        ValorAbatimento,

        /// <summary>
        /// Tipo de Inscrição do Sacado #obs:32 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 219-220
        SacadoTipo,

        /// <summary>
        /// Número do CNPJ ou CPF do Sacado #obs:33 
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 221-234
        SacadoDocumento,

        /// <summary>
        /// Nome do Sacado 
        /// </summary>
        [RegFormat(RegType.PX, 37)] // 235-271
        Sacado,

        /// <summary>
        /// Complemento do Registro: Brancos 
        /// </summary>
        [RegFormat(RegType.PX, 3)] // 272-274
        Brancos3,

        /// <summary>
        /// Endereço do Sacado 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 275-314
        Endereco,

        /// <summary>
        /// Bairro do Sacado 
        /// </summary>
        [RegFormat(RegType.PX, 12)] // 315-326
        Bairro,

        /// <summary>
        /// CEP do Endereço do Sacado 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 327-334
        CEP,

        /// <summary>
        /// Cidade do Sacado 
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 335-349
        Cidade,

        /// <summary>
        /// UF da Cidade do Sacado 
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 350-351
        UF,

        /// <summary>
        /// (ver campo Indicativo) Observações/Mensagem ou Sacador/Avalista #obs:13 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 352-391
        IndicativoValor,

        /// <summary>
        /// Número de Dias Para Protesto #obs:34 
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 392-393
        DiasProtesto,

        /// <summary>
        /// Complemento do Registro: Brancos 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 394-394
        Brancos4,

        /// <summary>
        /// Seqüencial de Registro #obs:35 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395-400
        Sequencia,
    }


    #endregion
      
    /// <summary>
    /// Estrutura de Retorno BB: Layout de Arquivo Retorno para convênios na faixa numérica entre 1.000.000 a 9.999.999 (Convênios de 7 posições)
    /// banco_do_brasil_cnab400-retorno.pdf - Página 4
    /// </summary>
    [RegLayout(@"^7", DateFormat6 = "ddMMyy")]
    public enum CNAB400Retorno7BB
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 1
        Controle_Registro,

        [RegFormat(RegType.P9, 2)] // 2
        Zeros1,

        [RegFormat(RegType.P9, 14)] // 4
        Zeros2,

        /// <summary>
        /// Prefixo da Agência
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 18
        Agencia,

        [RegFormat(RegType.PX, 1)] // 22
        AgenciaDV,

        /// <summary>
        /// Número da Conta Corrente do Cedente
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 23
        Conta,

        [RegFormat(RegType.PX, 1)] // 31
        ContaDV,

        /// <summary>
        /// Número do Convênio de Cobrança do Cedente
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 32-38
        Convenio,

        /// <summary>
        /// Número de Controle do Participante
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 39-63
        Controle,

        /// <summary>
        /// Nosso-Número
        /// </summary>
        [RegFormat(RegType.P9, 17)] // 64-80
        NossoNumero,

        /// <summary>
        /// Tipo de cobrança
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 81
        TipoCobranca,

        /// <summary>
        /// Tipo de cobrança específico para comando 72 (alteração de tipo de cobrança de títulos das carteiras 11 e 17)
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 82
        TipoCobrancaAlterado,

        /// <summary>
        /// Dias para cálculo
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 83-86
        DiasParaCalculos,

        /// <summary>
        /// Natureza do recebimento
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 87-88
        NaturezaRecebimento,

        /// <summary>
        /// Prefixo do Título
        /// </summary>
        [RegFormat(RegType.PX, 3)] // 89-91
        PrefixoTitulo,

        /// <summary>
        /// Variação da Carteira
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 92-94
        VariacaoCarteira,

        /// <summary>
        /// Conta Caução
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 95
        ContaCaucao,

        /// <summary>
        /// Taxa para desconto
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 96-100
        TaxaDesconto,

        /// <summary>
        /// Taxa IOF
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 101-105
        TaxaIOF,

        [RegFormat(RegType.P9, 1)] // 106
        Branco,

        [RegFormat(RegType.P9, 2)] // 107-108
        Carteira,
/* 
03 - Comando recusado (Motivo indicado na posição 087/088)
05 - Liquidado sem registro (carteira 17-tipo4)
06 - Liquidação Normal
07 - Liquidação por Conta/Parcial
08 - Liquidação por Saldo
09 - Baixa de Titulo
10 - Baixa Solicitada
11 - Títulos em Ser (constara somente do arquivo de existência de cobrança, fornecido mediante solicitação do cliente)
12 - Abatimento Concedido
13 - Abatimento Cancelado
14 - Alteração de Vencimento do título
15 - Liquidação em Cartório
16 - Confirmação de alteração de juros de mora
19 - Confirmação de recebimento de instruções para protesto
20 - Débito em Conta
21 - Alteração do Nome do Sacado
22 - Alteração do Endereço do Sacado
23 - Indicação de encaminhamento a cartório
24 - Sustar Protesto
25 - Dispensar Juros de mora
26 - Alteração do número do título dado pelo Cedente (Seu número) – 10 e 15 posições
28 - Manutenção de titulo vencido
31 - Conceder desconto
32 - Não conceder desconto
33 - Retificar desconto
34 - Alterar data para desconto
35 - Cobrar Multa
36 - Dispensar Multa
37 - Dispensar Indexador
38 - Dispensar prazo limite para recebimento
39 - Alterar prazo limite para recebimento
41 - Alteração do número do controle do participante (25 posições)
42 - Alteração do número do documento do sacado (CNPJ/CPF)
44 - Título pago com cheque devolvido
46 - Título pago com cheque, aguardando compensação
72 - Alteração de tipo de cobrança (específico para títulos das carteiras 11 e 17)
73 - Confirmação de Instrução de Parâmetro de Pagamento Parcial
96 - Despesas de Protesto
97 - Despesas de Sustação de Protesto
98 - Débito de Custas Antecipadas
*/
        [RegFormat(RegType.P9, 2)] // 109-110
        Comando,

        [RegFormat(RegType.PD, 6)] // 111-116
        DataPagamento,

        [RegFormat(RegType.PX, 10)] // 117-126
        NumeroDocumento,

        [RegFormat(RegType.PX, 20)] // 127-146
        Brancos1,

        [RegFormat(RegType.PD, 6)] // 147-152
        DataVencimento,

        [RegFormat(RegType.PV, 13)] // 153-165
        ValorDocumento,

        [RegFormat(RegType.P9, 3)] // 166-168
        BancoRecebedor,

        [RegFormat(RegType.P9, 4)] // 169-172
        AgenciaRecebedora,

        [RegFormat(RegType.PX, 1)] // 173
        AgenciaRecebedoraDV,

/*
00 – informado nos registros com comando 97-Despesas de Sustação de Protesto
nas posições 109/110 desde que o titulo não conste mais da existência
01 –duplicata mercantil
02 – nota promissória
03 – nota de seguro
05 – recibo
08 – letra de câmbio
09 – warrant
10 – cheque
12 – duplicata de serviço
13 – nota de débito
15 – apólice de seguro
25 – dívida ativa da União
26 – dívida ativa de Estado
27 – dívida ativa de Municípi
*/
        [RegFormat(RegType.P9, 2)] // 174-175
        Especie,

        [RegFormat(RegType.P9, 6)] // 176-181
        DataCredito,

        [RegFormat(RegType.PV, 7)] // 182-188
        ValorTarifa,

        [RegFormat(RegType.PV, 13)] // 189-201
        Despesas,

        [RegFormat(RegType.PV, 13)] // 202-214
        Juros,

        [RegFormat(RegType.PV, 13)] // 215-227
        IOF,

        [RegFormat(RegType.PV, 13)] // 228-240
        Abatimento,

        [RegFormat(RegType.PV, 13)] // 241-253
        Desconto,

        [RegFormat(RegType.PV, 13)] // 254-266
        Recebido,

        [RegFormat(RegType.PV, 13)] // 267-279
        Mora,

        [RegFormat(RegType.PV, 13)] // 280-292
        OutrosRecebimentos,

        [RegFormat(RegType.PV, 13)] // 293-305
        AbatimentoNaoAproveitado,

        [RegFormat(RegType.PV, 13)] // 306-318
        ValorPago,
/*
0-sem lançamento
1-débito
2-crédito
*/
        [RegFormat(RegType.P9, 1)] // 319
        IndicativoDebitoCredito,
/*
0-para todos os tipos de cobrança, exceto cobrança descontada nos comandos de liquidação ou baixa de título
Para tipo de cobrança descontada, nos comandos de liquidação ou baixa de título:
1-sem ajuste de valor/ajuste de valor a débito
2-ajuste de valor a crédito
*/
        [RegFormat(RegType.P9, 1)] // 320
        IndicativoValor,

        [RegFormat(RegType.P9, 12)] // 321-332
        Valorajuste,

        [RegFormat(RegType.PX, 60)] // 333-392
        Brancos2,

        [RegFormat(RegType.P9, 2)] // 393-394
        Canal,

        [RegFormat(RegType.P9, 6)] // 101-105
        Sequencia
    }


    /*
    /// <summary>
    /// Estrutura de Retorno Bradesco
    /// Página 42
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy")]
    public enum CNAB400Retorno1BB
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

        [RegFormat(RegType.PX, 3)] // 18
        Brancos1,

        /// <summary>
        /// Identificação da Empresa Cedente no Banco: Zero + Carteira Agência + Conta Corrente
        /// </summary>
        [RegFormat(RegType.PX, 17)] // 21
        Identificacao,

        /// <summary>
        /// Nº Controle do Participante - Uso da Empresa
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 38
        Documento,

        [RegFormat(RegType.PX, 8)] // 63
        Brancos2,

        /// <summary>
        /// Identificação do Título no Banco - Número Bancário para Cobrança Com e Sem Registro - Geração
        /// </summary>
        [RegFormat(RegType.P9, 12)] // 71
        NossoNumeroGerado,

        [RegFormat(RegType.PX, 22)] // 83
        Brancos3,

        /// <summary>
        /// Indicador de Rateio Crédito - “R”
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 105
        Rateio,

        [RegFormat(RegType.P9, 2)] // 106
        Brancos4,

        /// <summary>
        /// Carteira
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 108
        Carteira1,

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
        [RegFormat(RegType.PX, 20)] // 127
        NossoNumero,

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
        [RegFormat(RegType.P9, 5)] // 169
        CobradorAgencia,

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
        [RegFormat(RegType.PV, 13)] // 189
        ValorOutros,

        /// <summary>
        /// Juros Operação em Atraso
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 202
        Juros,

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

        [RegFormat(RegType.PX, 2)] // 293
        Brancos5,

        /// <summary>
        /// Motivo do Código de Ocorrência 25 (Confirmação de Instrução de Protesto Falimentar e do Código de Ocorrência 19 Confirmação de Instrução de Protesto)
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 295
        Motivo,

        /// <summary>
        /// Data do Crédito
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 296
        DataPagamento,

        /// <summary>
        /// Origem Pagamento
        /// </summary>
        [RegFormat(RegType.PX, 3)] // 302
        OrigemPagamento,

        [RegFormat(RegType.PX, 10)] // 305
        Brancos6,

        /// <summary>
        /// Quando cheque Bradesco informe 0237
        /// </summary>
        [RegFormat(RegType.PX, 4)] // 315
        Cheque,

        /// <summary>
        /// Motivos das Rejeições para os Códigos de Ocorrência da Posição 109 a 110
        /// </summary>
        [RegFormat(RegType.P9, 10)] // 319
        MotivoRegeicao,

        [RegFormat(RegType.PX, 40)] // 329
        Brancos7,

        /// <summary>
        /// Número do Cartório
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 369
        Cartorio,

        /// <summary>
        /// Número do Protocolo
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 371
        Protocolo,

        [RegFormat(RegType.PX, 14)] // 381
        Brancos8,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia

    }
    */
}
