using System;
using Impactro.Cobranca;
using System.Runtime.InteropServices;
using System.Data;

// Baseado no Bradesco Cobrança 07 (26/08/09)
// Inserido rotina de retorno 03/09/2012
namespace Impactro.Layout
{
    /// <summary>
    /// Gera o CNAB400 de Remessa de acordo com os padrões do Bradesco
    /// Para facilitar o suporte e padronizar recursos e funcionalidades cada banco tera sua proprie classe e rotinas de geração
    /// </summary>
    [ComVisible(false)]
    // ADicionado todos os tipos de registro na heranca ( Alexandre Savelli Bencz )
    public class CNAB400Bradesco : CNAB400<CNAB400Header1Bradesco, CNAB400Remessa1Bradesco, CNAB400ArquivoTrailer>
    {

        /// <summary>
        /// Cria uma construtora já configurando a classe base de layout
        /// </summary>
        public CNAB400Bradesco()
            : base()
        {
            AddTypes(typeof(CNAB400Remessa2Bradesco), typeof(CNAB400Remessa3Bradesco), typeof(CNAB400Remessa7Bradesco));
        }

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

            if (banco != Bancos.BRADESCO)
                throw new Exception("Esta classe é valida apenas para o Bradesco");

            //if (Cedente.CedenteCOD.Length != 20)
            //    throw new Exception("Erro nas definições do Código da Empresa ('" + Cedente.CedenteCOD + "') verifique os dados do Cedente: Banco, Agencia, Conta, Cod.Cedente");

            // Adicionado por Alexandre Savelli Bencz
            if (Cedente.CodCedente.Length != 20)
                Cedente.CodCedente = Cedente.CodCedente.PadLeft(20, '0');

            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // Proximo item
            SequencialRegistro = 1;

            regArqHeader[CNAB400Header1Bradesco.Empresa_Codigo] = Cedente.CedenteCOD;
            regArqHeader[CNAB400Header1Bradesco.Empresa_Nome] = Cedente.Cedente;
            regArqHeader[CNAB400Header1Bradesco.Data] = DataHoje;
            regArqHeader[CNAB400Header1Bradesco.Lote] = NumeroLote; // atualiza o lote
            regArqHeader[CNAB400Header1Bradesco.Sequencia] = SequencialRegistro++;

            // o sequencial do header é sempre 1 (FIXO)
            Add(regArqHeader);

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB400Remessa1Bradesco> regBoleto;
            Reg<CNAB400Remessa2Bradesco> regBoleto2; // Adicionado por Alexandre Savelli Bencz

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                regBoleto = new Reg<CNAB400Remessa1Bradesco>();

                // 21-37: Identificação da Empresa Cedente no Banco
                regBoleto[CNAB400Remessa1Bradesco.Carteira] = Cedente.Carteira;
                regBoleto[CNAB400Remessa1Bradesco.Agencia] = cAgDig[0];
                regBoleto[CNAB400Remessa1Bradesco.Conta] = cCCDig[0];
                regBoleto[CNAB400Remessa1Bradesco.ContaDAC] = cCCDig[1];

                if (boleto.BoletoID > 0) // 38-62
                    regBoleto[CNAB400Remessa1Bradesco.Identificacao] = boleto.BoletoID;

                regBoleto[CNAB400Remessa1Bradesco.MultaTipo] = boleto.PercentualMulta == 0 ? 0 : 2; // 66
                regBoleto[CNAB400Remessa1Bradesco.MultaPercentual] = boleto.PercentualMulta;        // 67

                // 71-82
                string cModalidade = Cedente.Modalidade;
                string cNossoNumero = boleto.NossoNumero; // Primeiro prepara e calcula o DV para ajustar o nosso numero de acordo com a modalidade e carteira
                regBoleto[CNAB400Remessa1Bradesco.NossoNumeroDig] = Banco_Bradesco.NossoNumero(Cedente.Carteira, ref cModalidade, ref cNossoNumero);
                regBoleto[CNAB400Remessa1Bradesco.NossoNumero] = cModalidade + cNossoNumero;

                regBoleto[CNAB400Remessa1Bradesco.Ocorrencia] = (int)boleto.Ocorrencia;         // 109 - 01-Remessa 
                regBoleto[CNAB400Remessa1Bradesco.NumeroDocumento] = boleto.NumeroDocumento;    // 111
                regBoleto[CNAB400Remessa1Bradesco.Vencimento] = boleto.DataVencimento;          // 121
                regBoleto[CNAB400Remessa1Bradesco.ValorDocumento] = boleto.ValorDocumento;      // 127
                regBoleto[CNAB400Remessa1Bradesco.Especie] = (int)boleto.Especie;               // 148 - 01 - Duplicata Mercantil
                regBoleto[CNAB400Remessa1Bradesco.Aceite] = boleto.Aceite;                      // 150 - 'N' padrão
                regBoleto[CNAB400Remessa1Bradesco.Emissao] = boleto.DataDocumento;              // 151
                regBoleto[CNAB400Remessa1Bradesco.Instrucao1] = boleto.Instrucao1;              // 157
                regBoleto[CNAB400Remessa1Bradesco.Instrucao2] = boleto.Instrucao2;              // 159
                regBoleto[CNAB400Remessa1Bradesco.Juros] = boleto.ValorMora;                    // 161
                if (boleto.ValorDesconto > 0)
                {
                    regBoleto[CNAB400Remessa1Bradesco.DescontoData] = boleto.DataDesconto;      // 174
                    regBoleto[CNAB400Remessa1Bradesco.DescontoValor] = boleto.ValorDesconto;    // 180
                }
                regBoleto[CNAB400Remessa1Bradesco.Abatimento] = boleto.ValorOutras < 0 ? -boleto.ValorOutras : 0; // 206
                regBoleto[CNAB400Remessa1Bradesco.SacadoTipo] = sacado.Tipo;                    // 219
                regBoleto[CNAB400Remessa1Bradesco.SacadoInscricao] = sacado.DocumentoNumeros;   // 221
                regBoleto[CNAB400Remessa1Bradesco.Nome] = sacado.Sacado;                        // 235
                regBoleto[CNAB400Remessa1Bradesco.Endereco] = sacado.Endereco;                  // 275
                regBoleto[CNAB400Remessa1Bradesco.CEP] = sacado.CepNumeros;                     // 332
                regBoleto[CNAB400Remessa1Bradesco.Avalista] = sacado.Avalista;                  // 335
                regBoleto[CNAB400Remessa1Bradesco.Sequencia] = SequencialRegistro++;

                // adiciona o boleto convertido em registro
                AddBoleto(regBoleto, boleto);

                // Em breve por, padrão esse registro opcional será definido externamente

                // Adicionado por Alexandre Savelli Bencz
                // Gera o registro tipo 2 na remessa
                if (!string.IsNullOrEmpty(boleto.Instrucoes) ||
                    boleto.ValorDesconto2 != 0 ||
                    boleto.ValorDesconto3 != 0)
                {
                    var mensagens = new string[4] { "", "", "", "" };

                    var msgs = boleto.Instrucoes.Replace("\r", "").Replace("<br/>", "\n").Replace("<br>", "\n").Split('\n');
                    for (int i = 0; i < msgs.Length; i++)
                        mensagens[i] = msgs[i].Trim();

                    regBoleto2 = new Reg<CNAB400Remessa2Bradesco>();

                    regBoleto2[CNAB400Remessa2Bradesco.Mensagem1] = mensagens[0];
                    regBoleto2[CNAB400Remessa2Bradesco.Mensagem2] = mensagens[1];
                    regBoleto2[CNAB400Remessa2Bradesco.Mensagem3] = mensagens[2];
                    regBoleto2[CNAB400Remessa2Bradesco.Mensagem4] = mensagens[3];
                    regBoleto2[CNAB400Remessa2Bradesco.DataLimiteDesconto2] = boleto.DataLimiteDesconto2;
                    regBoleto2[CNAB400Remessa2Bradesco.ValorDesconto2] = boleto.ValorDesconto2;
                    regBoleto2[CNAB400Remessa2Bradesco.DataLimiteDesconto3] = boleto.DataLimiteDesconto3;
                    regBoleto2[CNAB400Remessa2Bradesco.ValorDesconto3] = boleto.ValorDesconto3;

                    regBoleto2[CNAB400Remessa2Bradesco.Carteira] = Cedente.Carteira;
                    regBoleto2[CNAB400Remessa2Bradesco.Agencia] = cAgDig[0];
                    regBoleto2[CNAB400Remessa2Bradesco.Conta] = cCCDig[0];
                    regBoleto2[CNAB400Remessa2Bradesco.ContaDAC] = cCCDig[1];

                    regBoleto2[CNAB400Remessa2Bradesco.NossoNumero] = cModalidade + cNossoNumero;
                    regBoleto2[CNAB400Remessa2Bradesco.NossoNumeroDig] = Banco_Bradesco.NossoNumero(Cedente.Carteira, ref cModalidade, ref cNossoNumero);

                    regBoleto2[CNAB400Remessa2Bradesco.Sequencia] = SequencialRegistro++;

                    AddBoleto(regBoleto2, boleto);
                }

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
            Layout retorno = new Layout(typeof(CNAB400Retorno1Bradesco));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            retorno.ForEach<CNAB400Retorno1Bradesco>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    DataDocumento = (DateTime)reg[CNAB400Retorno1Bradesco.OcorrenciaData],
                    NossoNumero = (string)reg[CNAB400Retorno1Bradesco.NossoNumero],
                    NumeroDocumento = (string)reg[CNAB400Retorno1Bradesco.NumeroDocumento],
                    ValorDocumento = (double)reg[CNAB400Retorno1Bradesco.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB400Retorno1Bradesco.Vencimento],
                    DataPagamento = (DateTime)reg[CNAB400Retorno1Bradesco.DataPagamento],
                    ValorPago = (double)reg[CNAB400Retorno1Bradesco.ValorPago]
                }, reg.OriginalLine)
            );
            return retorno;
        }
    }

    #region "Estruturas de Remessa"

    /// <summary>
    /// Header Geral do Arquivo 
    /// </summary>
    [RegLayout(@"^0", DateFormat6 = "ddMMyy", Upper = true)]
    public enum CNAB400Header1Bradesco
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
        /// identificação do tipo de serviço
        /// </summary>
        [RegFormat(RegType.PX, 20)] // 27
        Empresa_Codigo,

        /// <summary>
        /// Nome por extenso da "empresa mãe"
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 47
        Empresa_Nome,

        /// <summary>
        /// Nº do banco na câmara de compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default="237")] // 77
        Banco_Codigo,

        /// <summary>
        /// Nome do Banco por Extenso
        /// </summary>
        [RegFormat(RegType.PX, 15, Default = "BRADESCO")] // 80
        Banco_Nome,

        /// <summary>
        /// Data da Gravação do Arquivo
        /// </summary>
        [RegFormat(RegType.PD, 6)] //  95
        Data,

        /// <summary>
        /// Espaços em branco
        /// </summary>
        [RegFormat(RegType.PX, 8)] // 101
        Brancos1,

        /// <summary>
        /// Identificação do sistema
        /// </summary>
        [RegFormat(RegType.PX, 2, Default = "MX")] // 109 
        Identificacao,

        /// <summary>
        /// Nº Seqüencial de Remessa
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 111
        Lote,

        /// <summary>
        /// Espaços em branco
        /// </summary>
        [RegFormat(RegType.PX, 277)] 
        Brancos2,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)]
        Sequencia

    }

    /// <summary>
    /// Estrutura de Remessa Bradesco
    /// Página 11
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy", Upper = true)]
    public enum CNAB400Remessa1Bradesco
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 1
        Controle_Registro,

        /// <summary>
        /// Agencia para débito automatico (opcional) Código da Agência do Sacado Exclusivo para Débito em Conta
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 2
        DebitoAutomatico_Agencia,

        /// <summary>
        /// Digito da Agencia para débito automatico (opcional) 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 7
        DebitoAutomatico_AgenciaDigito,

        /// <summary>
        /// Razão da Conta Corrente (opcional) Razão da Conta do Sacado 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 8
        DebitoAutomatico_Razao,

        /// <summary>
        /// Conta Corrente (opcional) Número da Conta do Sacado
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 13
        DebitoAutomatico_Conta,

        /// <summary>
        /// Digito da Conta Corrente (opcional) 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 20
        DebitoAutomatico_ContaDigito,

        /// <summary>
        /// código da carteira
        /// </summary>
        [RegFormat(RegType.P9, 1, Dump = "Identificação da Empresa Cedente no Banco (posição 21 a 37)")] // 21
        Zero,

        /// <summary>
        /// código da carteira
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 22
        Carteira,

        /// <summary>
        /// código da Agência Cedente, sem o dígito
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 25
        Agencia,

        /// <summary>
        /// Complemento do registro
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 30
        Conta,

        /// <summary>
        /// dígito da Conta
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 37
        ContaDAC,

        /// <summary>
        /// identificação do título na empresa
        /// </summary>
        [RegFormat(RegType.P9, 25, Dump = "BoletoID")] // 38 (usado como numerico)
        Identificacao,

        /// <summary>
        /// Código do Banco a ser debitado na Câmara de Compensação.
        /// Deverá ser informado 237, caso o cliente Cedente tenha optado pelo débito automático em Conta do Sacado.
        /// Para Títulos em que não deve ser aplicado o débito automático, este campo deverá ser preenchido com Zeros.
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 63
        Banco,

        /// <summary>
        /// Campo de Multa Se = 2 considerar percentual de multa. Se = 0, sem multa
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 66
        MultaTipo,

        /// <summary>
        /// Percentual de multa a ser considerado
        /// </summary>
        [RegFormat(RegType.PV, 4)] // 67
        MultaPercentual,

        /// <summary>
        /// Identificação do Título no Banco - Número Bancário para Cobrança Com e Sem Registro
        /// </summary>
        [RegFormat(RegType.P9, 11)] // 71
        NossoNumero,

        /// <summary>
        /// Digito de Auto Conferencia do Número Bancário.
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 82
        NossoNumeroDig,

        /// <summary>
        /// Desconto Bonificação por dia
        /// </summary>
        [RegFormat(RegType.PV, 10)] // 83
        BonificacaoValor,

        /// <summary>
        /// Condição para Emissão do Boleto de Cobrança
        /// (Cliente emite e o Banco somente processa o registro)
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "2")] // 93 
        Condicao,

        /// <summary>
        /// Ident. se emite Boleto para Débito Automático
        /// (quando diferente de “N” e os dados do débito estiverem incorretos, registra na cobrança e emite Boleto de cobrança. Nesta condição, não ocorrerá o agendamento do debito.)
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "S")] // 94 
        DebitoAutomatico,

        /// <summary>
        /// Espaços em Branco
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 95
        Brancos1,

        /// <summary>
        /// Indicador Rateio Crédito (opcional)
        /// (Somente deverá ser preenchido com a Letra “R”, se a Empresa contratou o serviço e rateio de crédito, caso não, informar Branco)
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 105
        Rateio,

        /// <summary>
        /// Endereçamento para Aviso do Débito Automático em Conta Corrente (opcional)
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 106
        DebitoAutomatico_Aviso,

        /// <summary>
        /// Agência onde o título será cobrado (será preenchido pelo banco no retorno)
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 107
        Brancos2,

        /// <summary>
        /// Identificação da ocorrência
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 109
        Ocorrencia,

        /// <summary>
        /// Nº do documento de cobrança (dupl.,np etc.)
        /// </summary>
        [RegFormat(RegType.P9, 10)] // 111 (na documentação o tipo está como caracter, mas será usado como numerico)
        NumeroDocumento,

        /// <summary>
        /// Data de Vencimento do Título
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 121
        Vencimento,

        /// <summary>
        /// Valor nominal do título
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 127
        ValorDocumento,

        /// <summary>
        /// Nº do banco na câmara de compensação (zeros)
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 140
        BancoCobrança,

        /// <summary>
        /// Agência onde o título será cobrado (será preenchido pelo banco no retorno)
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 143
        Agencia_Operadora,

        /// <summary>
        /// Espécie do título
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "01")] // 148
        Especie,

        /// <summary>
        /// Identificação de título aceito ou não aceito (A-Aceite, N-Não Aceite)
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "N")] // 150
        Aceite,

        /// <summary>
        /// Data da emissão do título
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 151
        Emissao,

        /// <summary>
        /// 1ª Instrução de cobrança
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 157
        Instrucao1,

        /// <summary>
        /// 2ª Instrução de cobrança
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 159
        Instrucao2,

        /// <summary>
        /// Valor de mora por dia de atraso
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 161
        Juros,

        /// <summary>
        /// Data limite para concessão de desconto
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 174
        DescontoData,

        /// <summary>
        /// Valor do desconto a ser concedido
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 180
        DescontoValor,

        /// <summary>
        /// Valor do i.o.f. recolhido p/ notas seguro 
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 193
        IOF,

        /// <summary>
        /// Valor do abatimento a ser concedido
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 206
        Abatimento,

        /// <summary>
        /// Identificação do tipo de inscrição/sacado (1-CPG, 2-CNPJ)
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 219
        SacadoTipo,

        /// <summary>
        /// Nº de inscrição do sacado (cpf/cnpj)
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 221
        SacadoInscricao,

        /// <summary>
        /// Nome do sacado
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 235
        Nome,

        /// <summary>
        /// Rua, número e complemento do sacado
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 275
        Endereco,

        /// <summary>
        /// Bairro do sacado
        /// </summary>
        [RegFormat(RegType.PX, 12)] // 315
        Mensagem1,

        /// <summary>
        /// CEP do sacado
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 332
        CEP,

        /// <summary>
        /// Nome do sacador ou avalista
        /// </summary>
        [RegFormat(RegType.PX, 60)] // 335
        Avalista,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia
    }

    /// <summary>
    /// Estrutura de Remessa Bradesco ( Adicionado por Alexandre Savelli Bencz )
    /// Página 11
    /// </summary>
    [RegLayout(@"^2", DateFormat6 = "ddMMyy", Upper = true)]
    public enum CNAB400Remessa2Bradesco
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "2")] // 1
        Controle_Registro,

        /// <summary>
        /// Mensagem ao pagador 1
        /// </summary>
        [RegFormat(RegType.PX, 80)] // 2
        Mensagem1,

        /// <summary>
        /// Mensagem ao pagador 2
        /// </summary>
        [RegFormat(RegType.PX, 80)] // 82
        Mensagem2,

        /// <summary>
        /// Mensagem ao pagador 3
        /// </summary>
        [RegFormat(RegType.PX, 80)] // 162
        Mensagem3,

        /// <summary>
        /// Mensagem ao pagador 4
        /// </summary>
        [RegFormat(RegType.PX, 80)] // 242
        Mensagem4,

        /// <summary>
        /// Data limite para concessão de Desconto 2
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 322
        DataLimiteDesconto2,

        /// <summary>
        /// Valor do desconto 2
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 328
        ValorDesconto2,

        /// <summary>
        /// Data limite para concessão de Desconto 3
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 341
        DataLimiteDesconto3,

        /// <summary>
        /// Valor do desconto 3
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 347
        ValorDesconto3,

        /// <summary>
        /// Reserva
        /// </summary>
        [RegFormat(RegType.PX, 7)] // 360
        Reserva,

        /// <summary>
        /// código da carteira
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 367
        Carteira,

        /// <summary>
        /// código da Agência Cedente, sem o dígito
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 370
        Agencia,

        /// <summary>
        /// Complemento do registro
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 375
        Conta,

        /// <summary>
        /// dígito da Conta
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 382
        ContaDAC,

        /// <summary>
        /// Identificação do Título no Banco - Número Bancário para Cobrança Com e Sem Registro
        /// </summary>
        [RegFormat(RegType.P9, 11)] // 383
        NossoNumero,

        /// <summary>
        /// Digito de Auto Conferencia do Número Bancário.
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 397
        NossoNumeroDig,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia
    }

    /// <summary>
    /// Rateio de credito ( Adicionado por Alexandre Savelli Bencz )
    /// </summary>
    [RegLayout(@"^3", DateFormat8 = "ddMMyy", Upper = true)]
    public enum CNAB400Remessa3Bradesco
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 1
        Controle_Registro,

        /// <summary>
        /// código da carteira
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 2
        Carteira,

        /// <summary>
        /// código da Agência Cedente, sem o dígito
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 5
        Agencia,

        /// <summary>
        /// Complemento do registro
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 10
        Conta,

        /// <summary>
        /// dígito da Conta
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 17
        ContaDAC,

        /// <summary>
        /// Identificação do Título no Banco - Número Bancário para Cobrança Com e Sem Registro com DV
        /// </summary>
        [RegFormat(RegType.P9, 12)] // 18
        NossoNumero,

        /// <summary>
        /// Código para calculo do rateio
        /// 1 - Valor Cobrado
        /// 2 - Valor do registro
        /// 3 - Rateio pelo menor valor
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 30
        Codigo_Calculo_Rateio,

        /// <summary>
        /// Tipo do valor informado
        /// 1 - percentual
        /// 2 - valor
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 31
        Tipo_Valor,

        /// <summary>
        /// Brancos
        /// </summary>
        [RegFormat(RegType.PX, 12)] // 32
        Brancos1,

        //-----------------------------------------------------------------------------------------------
        // Beneficiario 1
        /// <summary>
        /// Codigo do banco para credito do 1º beneficiario
        /// Padrão = 237 ( para o Bradesco )
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "237")] // 44
        Codigo_Para_Credito_Beneficiario_1,

        /// <summary>
        /// Codigo da agencia para o credito do 1º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 47
        Codigo_Agencia_Beneficiario_1,

        /// <summary>
        /// Digito da agencia para o credito do 1º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 52
        Digito_Agencia_Beneficiario_1,

        /// <summary>
        /// Numero da conta para o credito do 1º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 12)] // 53
        Numero_Conta_Beneficiario_1,

        /// <summary>
        /// Digito da conta para o credito do 1º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 65
        Digito_Conta_Beneficiatio_1,

        /// <summary>
        /// Valor ou percentual para rateio
        /// - Valor do rateio, de acordo com o tipo ( percentual ou valor definido na pos. 31)
        /// - Se percentual, informar com 3 casas decimais
        /// TODO - PERCENTUAL COM 3 CASAS DECIMAIS ( apenas para porcentagem )
        /// </summary>
        [RegFormat(RegType.PV, 15)] //66
        Valor_Para_Rateio_1,

        /// <summary>
        /// Nome do primeiro beneficiario
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 81
        Nome_Beneficiario_1,

        /// <summary>
        /// Brancos
        /// </summary>
        [RegFormat(RegType.PX, 31)] // 121
        Brancos2,

        /// <summary>
        /// Identificador da parcela do 1º beneficiario
        /// </summary>
        [RegFormat(RegType.PX, 6)] // 152
        Parcela_Beneficiario_1,

        /// <summary>
        /// Quantidade de dias para credito do beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 158
        Floating_Beneficiario_1,

        //-----------------------------------------------------------------------------------------------
        // Beneficiario 2
        /// <summary>
        /// Codigo do banco para credito do 2º beneficiario
        /// Padrão = 237 ( para o Bradesco )
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "237")] // 161
        Codigo_Para_Credito_Beneficiario_2,

        /// <summary>
        /// Codigo da agencia para o credito do 2º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 164
        Codigo_Agencia_Beneficiario_2,

        /// <summary>
        /// Digito da agencia para o credito do 2º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 169
        Digito_Agencia_Beneficiario_2,

        /// <summary>
        /// Numero da conta para o credito do 2º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 12)] // 170
        Numero_Conta_Beneficiario_2,

        /// <summary>
        /// Digito da conta para o credito do 2º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 182
        Digito_Conta_Beneficiatio_2,

        /// <summary>
        /// Valor ou percentual para rateio
        /// - Valor do rateio, de acordo com o tipo ( percentual ou valor definido na pos. 31)
        /// - Se percentual, informar com 3 casas decimais
        /// TODO - PERCENTUAL COM 3 CASAS DECIMAIS ( apenas para porcentagem )
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 183
        Valor_Para_Rateio_2,

        /// <summary>
        /// Nome do primeiro beneficiario
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 198
        Nome_Beneficiario_2,

        /// <summary>
        /// Brancos
        /// </summary>
        [RegFormat(RegType.PX, 31)] // 238
        Brancos3,

        /// <summary>
        /// Identificador da parcela do 2º beneficiario
        /// </summary>
        [RegFormat(RegType.PX, 6)] // 269
        Parcela_Beneficiario_2,

        /// <summary>
        /// Quantidade de dias para credito do beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 275
        Floating_Beneficiario_2,
        //-----------------------------------------------------------------------------------------------
        // Beneficiario 3
        /// <summary>
        /// Codigo do banco para credito do 3º beneficiario
        /// Padrão = 237 ( para o Bradesco )
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "237")] // 278
        Codigo_Para_Credito_Beneficiario_3,

        /// <summary>
        /// Codigo da agencia para o credito do 3º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 281
        Codigo_Agencia_Beneficiario_3,

        /// <summary>
        /// Digito da agencia para o credito do 3º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 286
        Digito_Agencia_Beneficiario_3,

        /// <summary>
        /// Numero da conta para o credito do 3º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 12)] // 287
        Numero_Conta_Beneficiario_3,

        /// <summary>
        /// Digito da conta para o credito do 3º beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 299
        Digito_Conta_Beneficiatio_3,

        /// <summary>
        /// Valor ou percentual para rateio
        /// - Valor do rateio, de acordo com o tipo ( percentual ou valor definido na pos. 31)
        /// - Se percentual, informar com 3 casas decimais
        /// TODO - PERCENTUAL COM 3 CASAS DECIMAIS ( apenas para porcentagem )
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 300
        Valor_Para_Rateio_3,

        /// <summary>
        /// Nome do primeiro beneficiario
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 315
        Nome_Beneficiario_3,

        /// <summary>
        /// Brancos
        /// </summary>
        [RegFormat(RegType.PX, 31)] // 355
        Brancos4,

        /// <summary>
        /// Identificador da parcela do 3º beneficiario
        /// </summary>
        [RegFormat(RegType.PX, 6)] // 386
        Parcela_Beneficiario_3,

        /// <summary>
        /// Quantidade de dias para credito do beneficiario
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 392
        Floating_Beneficiario_3,

        //-----------------------------------------------------------------------------------------------

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia
    }

    /// <summary>
    /// Sacador avalista ( Adicionado por Alexandre Savelli Bencz )
    /// </summary>
    [RegLayout(@"^7", DateFormat8 = "ddMMyy", Upper = true)]
    public enum CNAB400Remessa7Bradesco
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "7")] // 1
        Controle_Registro,

        /// <summary>
        /// Endereco sacador/avalista
        /// </summary>
        [RegFormat(RegType.PX, 45)] // 2
        Endereco_Sacador_Avalista,

        /// <summary>
        /// CEP do sacado
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 45 & 52
        CEP,

        /// <summary>
        /// Cidade
        /// </summary>
        [RegFormat(RegType.PX, 20)] // 55
        Cidade,

        /// <summary>
        /// UF
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 75
        UF,

        /// <summary>
        /// Brancos
        /// </summary>
        [RegFormat(RegType.PX, 290)] // 77
        Brancos,

        /// <summary>
        /// código da carteira
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 367
        Carteira,

        /// <summary>
        /// código da Agência Cedente, sem o dígito
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 370
        Agencia,

        /// <summary>
        /// Complemento do registro
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 375
        Conta,

        /// <summary>
        /// dígito da Conta
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 382
        ContaDAC,

        /// <summary>
        /// Identificação do Título no Banco - Número Bancário
        /// </summary>
        [RegFormat(RegType.P9, 11)] // 383
        NossoNumero,

        /// <summary>
        /// Digito de Auto Conferencia do Número Bancário.
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 394
        NossoNumeroDig,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia
    }
#endregion

    #region Retorno
    /// <summary>
    /// Estrutura de Retorno Bradesco
    /// Página 42
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy")]
    public enum CNAB400Retorno1Bradesco
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

    #endregion

}
