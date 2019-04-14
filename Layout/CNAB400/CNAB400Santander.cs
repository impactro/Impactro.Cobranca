using System;
using Impactro.Cobranca;
using System.Runtime.InteropServices;
using System.Text;

// 30/11/2013
// histórico em: http://www.fabioferreira.eng.br/arquivo-remessa-santander.aspx
namespace Impactro.Layout
{
    /// <summary>
    /// Trata os arquivos CNAB400 do santander
    /// </summary>
    [ComVisible(false)]
    public class CNAB400Santander : CNAB400<CNAB400SantanderHeader, CNAB400SantanderRemessa1, CNAB400SantanderTrailer>
    {
        /// <summary>
        /// Gera o aquivo baseado nas coleção de todas informações passada até o momento
        /// </summary>
        /// <returns></returns>
        public override string Remessa()
        {
            // Existe algum BUG na executação de template e reflection com ActiveX que  dentro do doreach de conteudo da classe e layout acaba por não gerar o arquivo final.
            // Então, cada linha já será gerada imediatamente em uma String Builder, liberando assim a necessidade de memorizar os itens que iriam conter no arquivo.

            string[] cBanco = Cedente.Banco.Split('-');
            string[] cAgDig = Cedente.Agencia.Split('-');
            string cContaDigitos = CobUtil.SoNumeros(Cedente.Conta);

            Bancos banco = (Bancos)CobUtil.GetInt(cBanco[0]);
            if (!(banco == Bancos.SANTANDER || banco == Bancos.BANESPA_SANTANDER))
                throw new Exception("Esta classe é valida apenas para o Santander");
            else if (cContaDigitos.Length != 10)
                throw new Exception("Erro na Conta não tem 10 dígitos");
            else if (Cedente.CodCedente.Length != 7)
                throw new Exception("Erro no CodCedente não tem 7 dígitos");
            else if (Cedente.CedenteCOD.Length != 20)
                throw new Exception("Erro no CedenteCOD não tem 20 dígitos");
            else if (Cedente.Convenio.Length != 25)
                throw new Exception("Erro no Convenio não tem 25 dígitos");

            regArqHeader[CNAB400SantanderHeader.Empresa_Codigo] = Cedente.CedenteCOD;
            regArqHeader[CNAB400SantanderHeader.Empresa_Nome] = Cedente.Cedente;
            regArqHeader[CNAB400SantanderHeader.Banco_Codigo] = (int)banco;
            regArqHeader[CNAB400SantanderHeader.Data] = DataHoje;

            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // Inicia o contador Sequencial
            regArqHeader[CNAB400SantanderHeader.Sequencia] = SequencialRegistro = 1;

            // o sequencial do header é sempre 1 (FIXO)
            Add(regArqHeader);

            // Proximo item
            SequencialRegistro = 2;

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB400SantanderRemessa1> regBoleto;

            double nValor = 0;
#if TEST_LOG
            Log += "Remessa " + Boletos.Count + "!\r\n";
#endif
            foreach (string n in Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;
                nValor += boleto.ValorDocumento;
#if TEST_LOG
                Log += string.Format("{0} {1} {2:C}", n, boleto.NossoNumero, boleto.ValorDocumento );
#endif
                regBoleto = new Reg<CNAB400SantanderRemessa1>();

                regBoleto[CNAB400SantanderRemessa1.CedenteTipo] = Cedente.Tipo;
                regBoleto[CNAB400SantanderRemessa1.CedenteCNPJ] = Cedente.DocumentoNumeros;
                regBoleto[CNAB400SantanderRemessa1.CedenteCOD] = Cedente.CedenteCOD;
                regBoleto[CNAB400SantanderRemessa1.CedenteControle] = Cedente.Convenio;
                regBoleto[CNAB400SantanderRemessa1.Banco] = 33;
                regBoleto[CNAB400SantanderRemessa1.CarteiraTipo] = Cedente.CarteiraTipo;
                regBoleto[CNAB400SantanderRemessa1.Agencia] = Cedente.CarteiraTipo == "5" ? cAgDig[0] : "0";


                regBoleto[CNAB400SantanderRemessa1.NossoNumero] = boleto.NossoNumero;
#if TEST_LOG
                Log += string.Format(" ? {0} ? {1}\r\n", regBoleto[CNAB400SantanderRemessa1.NossoNumero], boleto.NossoNumero);
#endif
                regBoleto[CNAB400SantanderRemessa1.NossoNumeroDig] = CobUtil.Modulo11Especial(boleto.NossoNumero, 9);
                regBoleto[CNAB400SantanderRemessa1.MultaTipo] = boleto.PercentualMulta == 0 ? 0 : 4;
                regBoleto[CNAB400SantanderRemessa1.MultaPercentual] = boleto.PercentualMulta;
                regBoleto[CNAB400SantanderRemessa1.Ocorrencia] = (int)boleto.Ocorrencia;
                regBoleto[CNAB400SantanderRemessa1.SeuNumero] = boleto.BoletoID;
                regBoleto[CNAB400SantanderRemessa1.DataVencimento] = boleto.DataVencimento;
                regBoleto[CNAB400SantanderRemessa1.Valor] = boleto.ValorDocumento;
                regBoleto[CNAB400SantanderRemessa1.Especie] = (int)boleto.Especie;
                regBoleto[CNAB400SantanderRemessa1.Aceite] = boleto.Aceite;
                regBoleto[CNAB400SantanderRemessa1.DataDocumento] = boleto.DataDocumento;

                int nInstrucao1 = boleto.Instrucao1;
                int nInstrucao2 = boleto.Instrucao2;

                if (nInstrucao1 == 0 && nInstrucao2 > 0)
                {
                    nInstrucao1 = nInstrucao2;
                    nInstrucao2 = 0;
                }

                // Prioriza protesto na instrução 1
                if (nInstrucao1 > 0)
                {
                    regBoleto[CNAB400SantanderRemessa1.Instrucao1] = nInstrucao1;
                    if (nInstrucao1 == 6)
                    {
                        if (boleto.DiasProtesto == 0)
                            throw new Exception("Não é possivel dar instrução de protesto sem 'DiasProtesto'");

                        regBoleto[CNAB400SantanderRemessa1.DiasProtesto] = boleto.DiasProtesto;
                    }
                }

                // Trata a mora na segunda instrução (Se tem Mora maior de 1 centavo adiciona a mora)
                if (boleto.ValorMora > 0.01)
                    regBoleto[CNAB400SantanderRemessa1.Mora] = boleto.ValorMora;
                else // Caso contrario não cobra
                {
                    if (nInstrucao1 == 0)
                        regBoleto[CNAB400SantanderRemessa1.Instrucao1] = 8;
                    else
                        regBoleto[CNAB400SantanderRemessa1.Instrucao2] = 8;
                }

                // Mas se a instrução 2 algo diferente adicona também, talvez sobrescrevendo...
                if (nInstrucao2 != 0)
                    regBoleto[CNAB400SantanderRemessa1.Instrucao2] = nInstrucao2;

                regBoleto[CNAB400SantanderRemessa1.DataDesconto] = boleto.DataDesconto;
                regBoleto[CNAB400SantanderRemessa1.ValorIOF] = boleto.ValorIOF;
                regBoleto[CNAB400SantanderRemessa1.ValorDesconto] = boleto.ValorDesconto;
                regBoleto[CNAB400SantanderRemessa1.ValorAbatimento] = boleto.ValorOutras < 0 ? -boleto.ValorOutras : 0;
                regBoleto[CNAB400SantanderRemessa1.Sacado_Tipo] = sacado.Tipo;
                regBoleto[CNAB400SantanderRemessa1.Sacado_Inscricao] = sacado.DocumentoNumeros;
                regBoleto[CNAB400SantanderRemessa1.Nome] = sacado.Sacado;
                regBoleto[CNAB400SantanderRemessa1.Endereco] = sacado.Endereco;
                regBoleto[CNAB400SantanderRemessa1.Bairro] = sacado.Bairro;
                regBoleto[CNAB400SantanderRemessa1.Cidade] = sacado.Cidade;
                regBoleto[CNAB400SantanderRemessa1.UF] = sacado.UF;
                regBoleto[CNAB400SantanderRemessa1.CEP] = sacado.CepNumeros;
                regBoleto[CNAB400SantanderRemessa1.Avalista] = sacado.Avalista;

                regBoleto[CNAB400SantanderRemessa1.Complemento] = cContaDigitos.Substring(8, 2);
                regBoleto[CNAB400SantanderRemessa1.Sequencia] = SequencialRegistro++;

                // adiciona o boleto convertido em registro
                AddBoleto(regBoleto, boleto);

                AddOpcionais(boleto);
            }

            regArqTrailer[CNAB400SantanderTrailer.Quantidade] = Boletos.NossoNumeros.Count;
            //regArqTrailer[CNAB400SantanderTrailer.Quantidade] = Boletos.NossoNumeros.Count + 2; // adicionando o proprio header e trailer
            regArqTrailer[CNAB400SantanderTrailer.Valor] = nValor;
            regArqTrailer[CNAB400SantanderTrailer.Sequencia] = SequencialRegistro;

            Add(regArqTrailer);

            //ShowDumpLine = true;
            // Gera o Texto de saida da forma padrão
            return this.Conteudo;
        }

        /// <summary>
        /// Processa as informações baseado no conteudo de um arquivo CNAB
        /// </summary>
        /// <param name="cData">Conteudo do arquivo</param>
        public override Layout Retorno(string cData)
        {
            Layout retorno = new Layout(typeof(CNAB400SantanderRetorno1));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            retorno.ForEach<CNAB400SantanderRetorno1>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    DataDocumento = (DateTime)reg[CNAB400SantanderRetorno1.OcorrenciaData],
                    NossoNumero = (string)reg[CNAB400SantanderRetorno1.NossoNumero],
                    NumeroDocumento = (string)reg[CNAB400SantanderRetorno1.NumeroDocumento],
                    ValorDocumento = (double)reg[CNAB400SantanderRetorno1.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB400SantanderRetorno1.Vencimento],
                    DataPagamento = (DateTime)reg[CNAB400SantanderRetorno1.DataPagamento],
                    ValorPago = (double)reg[CNAB400SantanderRetorno1.ValorPago]
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
    public enum CNAB400SantanderHeader
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
        [RegFormat(RegType.P9, 3)] // 77
        Banco_Codigo,

        /// <summary>
        /// Nome do Banco por Extenso
        /// </summary>
        [RegFormat(RegType.PX, 15, Default="SANTANDER")] // 80
        Banco_Nome,

        /// <summary>
        /// Data da Gravação do Arquivo
        /// </summary>
        [RegFormat(RegType.PD, 6)] //  95
        Data,

        /// <summary>
        /// Zeros
        /// </summary>
        [RegFormat(RegType.P9, 16)] // 101
        Zeros1,

        /// <summary>
        /// Espaços em branco
        /// </summary>
        [RegFormat(RegType.PX, 275)] // 117 (no proprio arquivo de documentação este valor está errado)
        Brancos1,

        /// <summary>
        /// Espaços em branco
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 392
        Versao,

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
    public enum CNAB400SantanderRemessa1
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 1
        Controle_Registro,

        /// <summary>
        /// Tipo de inscrição do cedente: 01 = CPF 02 = CGC(CNPJ)
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 2
        CedenteTipo,

        /// <summary>
        /// CGC ou CPF do cedente
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 4
        CedenteCNPJ,

        /// <summary>
        /// Código de Transmissão (nota 1)
        /// </summary>
        [RegFormat(RegType.P9, 20)] // 18
        CedenteCOD,

        /// <summary>
        /// Número de controle do participante, para controle por parte do cedente
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 38
        CedenteControle,

        /// <summary>
        /// Identificação do Título no Banco - Número Bancário para Cobrança + DV
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 63
        NossoNumero,

        [RegFormat(RegType.P9, 1)] // 70
        NossoNumeroDig,

        [RegFormat(RegType.PD, 6)] // 71
        DataSegundoDesconto,

        [RegFormat(RegType.PX, 1)] // 77
        Brancos1,

        [RegFormat(RegType.P9, 1)] // 78
        MultaTipo,

        [RegFormat(RegType.PV, 4)] // 79
        MultaPercentual,

        [RegFormat(RegType.P9, 2)] // 83
        MoedaValor,

        [RegFormat(RegType.PV, 13)] // 85
        MoedaValorOutro,

        [RegFormat(RegType.PX, 4)] // 98
        Brancos2,

        [RegFormat(RegType.PD, 6)] // 102
        DataMulta,

        /// <summary>
        /// 1 = ELETRÔNICA COM REGISTRO
        /// 3 = CAUCIONADA ELETRÔNICA
        /// 4 = COBRANÇA SEM REGISTRO
        /// 5 = RÁPIDA COM REGISTRO (BLOQUETE EMITIDO PELO CLIENTE)
        /// 6 = CAUCIONADA RAPIDA
        /// 7 = DESCONTADA ELETRÔNICA
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 108
        CarteiraTipo,

        [RegFormat(RegType.P9, 2)] // 109
        Ocorrencia,

        [RegFormat(RegType.P9, 10)] // 111
        SeuNumero,

        [RegFormat(RegType.PD, 6)] // 121
        DataVencimento,

        [RegFormat(RegType.PV, 13)] // 127
        Valor,

        [RegFormat(RegType.P9, 3)] // 140
        Banco,

        [RegFormat(RegType.P9, 5)] // 143
        Agencia,

        [RegFormat(RegType.P9, 2)] // 148
        Especie,

        [RegFormat(RegType.PX, 1)] // 150
        Aceite,

        [RegFormat(RegType.PD, 6)] // 151
        DataDocumento,

        /// <summary>
        /// 00 = NÃO HÁ INSTRUÇÕES
        /// 02 = BAIXAR APÓS QUINZE DIAS DO VENCIMENTO
        /// 03 = BAIXAR APÓS 30 DIAS DO VENCIMENTO
        /// 04 = NÃO BAIXAR
        /// 06 = PROTESTAR (VIDE POSIÇÃO392/393)
        /// 07 = NÃO PROTESTAR
        /// 08 = NÃO COBRAR JUROS DE MORA
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 157
        Instrucao1,

        [RegFormat(RegType.P9, 2)] // 159
        Instrucao2,

        [RegFormat(RegType.PV, 13)] // 161
        Mora,
        
        [RegFormat(RegType.PD, 6)] // 174
        DataDesconto,
        
        [RegFormat(RegType.PV, 13)] // 180
        ValorDesconto,

        [RegFormat(RegType.PV, 13)] // 193
        ValorIOF,

        [RegFormat(RegType.PV, 13)] // 206
        ValorAbatimento,

        /// <summary>
        /// Identificação do tipo de inscrição/sacado (1-CPG, 2-CNPJ)
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 219
        Sacado_Tipo,

        /// <summary>
        /// Nº de inscrição do sacado (cpf/cnpj)
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 221
        Sacado_Inscricao,

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
        Bairro,

        /// <summary>
        /// CEP do sacado
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 332
        CEP,

        /// <summary>
        /// Nome do sacador ou avalista
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 335
        Cidade,

        [RegFormat(RegType.PX, 2)] // 350
        UF,

        [RegFormat(RegType.PX, 30)] // 352
        Avalista,
        
        [RegFormat(RegType.PX, 1)] // 382
        Brancos3,
        
        [RegFormat(RegType.PX, 1, Default="I")] // 383
        Identificador,

        [RegFormat(RegType.P9, 2)] // 384
        Complemento,

        [RegFormat(RegType.PX, 6)] // 386
        Brancos4,

        [RegFormat(RegType.P9, 2)] // 392
        DiasProtesto,

        [RegFormat(RegType.PX, 1)] // 394
        Brancos5,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia
    }

    /// <summary>
    /// Estrutura de Retorno Santander
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy")]
    public enum CNAB400SantanderRetorno1
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
        CarteiraTipo,

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

    /// <summary>
    /// Trailer Geral do Arquivo CNAB400 (Tipo=9)
    /// </summary>
    [RegLayout(@"^9", Upper = true)]
    public enum CNAB400SantanderTrailer
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "9")]
        Controle_Registro,

        /// <summary>
        /// Quantidade total de linhas no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)]
        Quantidade,

        [RegFormat(RegType.PV, 13)]
        Valor,

        /// <summary>
        /// Espaços em branco
        /// </summary>
        [RegFormat(RegType.P9, 374)]
        Zeros,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)]
        Sequencia
    }
    
    #endregion
}
