using System;
using Impactro.Cobranca;
using System.Runtime.InteropServices;
using System.Text;

// 30/11/2013
// histórico em: http://www.fabioferreira.eng.br/arquivo-remessa-BRB.aspx
namespace Impactro.Layout
{
    /// <summary>
    /// Trata os arquivos CNAB400 do BRB 
    /// (baseado nos fontes do BRB)
    /// </summary>
    [ComVisible(false)]
    public class CNAB400BRB : CNAB400<CNAB400BRBHeader, CNAB400BRBRemessa1, CNAB400BRBTrailer>
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
            string[] cCCDig = Cedente.Conta.Split('-');

            Bancos banco = (Bancos)CobUtil.GetInt(cBanco[0]);
            if (banco != Bancos.BRB)
                throw new Exception("Esta classe é valida apenas para o BRB");

            regArqHeader[CNAB400BRBHeader.Agencia] = cAgDig[0];
            regArqHeader[CNAB400BRBHeader.Conta] = cCCDig[0];
            regArqHeader[CNAB400BRBHeader.DataHora] = DataHoje;

            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // o sequencial do header é sempre 1 (FIXO)
            Add(regArqHeader);

            // Proximo item
            SequencialRegistro = 2;

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB400BRBRemessa1> regBoleto;

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
                regBoleto = new Reg<CNAB400BRBRemessa1>();

                regBoleto[CNAB400BRBRemessa1.Agencia] = cAgDig[0];
                regBoleto[CNAB400BRBRemessa1.Conta] = cCCDig[0];
                regBoleto[CNAB400BRBRemessa1.Sacado_Inscricao] = sacado.DocumentoNumeros;
                regBoleto[CNAB400BRBRemessa1.Nome] = sacado.Sacado;
                regBoleto[CNAB400BRBRemessa1.Endereco] = sacado.Endereco;
                regBoleto[CNAB400BRBRemessa1.Cidade] = sacado.Cidade;
                regBoleto[CNAB400BRBRemessa1.UF] = sacado.UF;
                regBoleto[CNAB400BRBRemessa1.CEP] = sacado.CepNumeros;
                regBoleto[CNAB400BRBRemessa1.Sacado_Tipo] = sacado.Tipo;
                regBoleto[CNAB400BRBRemessa1.SeuNumero] = boleto.BoletoID;
                regBoleto[CNAB400BRBRemessa1.NossoNumero] = boleto.NossoNumero;
                regBoleto[CNAB400BRBRemessa1.Emissao] = boleto.DataDesconto;

                if (boleto.Especie == Especies.DM)
                    regBoleto[CNAB400BRBRemessa1.Tipo_Documento] = 21;
                else if (boleto.Especie == Especies.NP)
                    regBoleto[CNAB400BRBRemessa1.Tipo_Documento] = 22;
                else if (boleto.Especie == Especies.RC)
                    regBoleto[CNAB400BRBRemessa1.Tipo_Documento] = 25;
                else
                    regBoleto[CNAB400BRBRemessa1.Tipo_Documento] = 39;

                regBoleto[CNAB400BRBRemessa1.DataVencimento] = boleto.DataVencimento;
                regBoleto[CNAB400BRBRemessa1.Valor] = boleto.ValorDocumento;

                regBoleto[CNAB400BRBRemessa1.Instrucao1] = boleto.Instrucao1;
                regBoleto[CNAB400BRBRemessa1.Instrucao2] = boleto.Instrucao2;

                SequencialRegistro++;

                // adiciona o boleto convertido em registro
                AddBoleto(regBoleto, boleto);

                AddOpcionais(boleto);
            }

            // Atualiza o Header com o numero total de registros
            regArqHeader[CNAB400BRBHeader.Sequencia] = SequencialRegistro - 1;

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
            Layout retorno = new Layout(typeof(CNAB400BRBRetorno1));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            retorno.ForEach<CNAB400BRBRetorno1>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    DataDocumento = (DateTime)reg[CNAB400BRBRetorno1.OcorrenciaData],
                    NossoNumero = (string)reg[CNAB400BRBRetorno1.NossoNumero],
                    NumeroDocumento = (string)reg[CNAB400BRBRetorno1.NumeroDocumento],
                    ValorDocumento = (double)reg[CNAB400BRBRetorno1.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB400BRBRetorno1.Vencimento],
                    DataPagamento = (DateTime)reg[CNAB400BRBRetorno1.DataPagamento],
                    ValorPago = (double)reg[CNAB400BRBRetorno1.ValorPago]
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
    public enum CNAB400BRBHeader
    {
        [RegFormat(RegType.PX, 3, Default = "DCB")] // 1
        Literal,

        [RegFormat(RegType.P9, 3, Default = "001")] // 2
        Versao,

        [RegFormat(RegType.PX, 3, Default = "075")] // 3
        Arquivo,

        [RegFormat(RegType.P9, 3)] // 10
        Agencia,

        [RegFormat(RegType.P9, 7)] // 13
        Conta,

        [RegFormat(RegType.PD, 14)] // 20
        DataHora,

        [RegFormat(RegType.P9, 6)]
        Sequencia
    }

    /// <summary>
    /// Estrutura de Remessa Bradesco
    /// Página 11
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy", Upper = true)]
    public enum CNAB400BRBRemessa1
    {
        [RegFormat(RegType.P9, 2, Default = "01")] // 1
        Identificacao_Registro,

        [RegFormat(RegType.P9, 3)] // 3
        Agencia,

        [RegFormat(RegType.P9, 7)] // 6
        Conta,

        [RegFormat(RegType.PX, 14)] // 13
        Sacado_Inscricao,

        [RegFormat(RegType.PX, 35)] // 27
        Nome,

        [RegFormat(RegType.PX, 35)] // 62
        Endereco,

        [RegFormat(RegType.PX, 15)] // 97
        Cidade,

        [RegFormat(RegType.PX, 2)] // 112
        UF,

        [RegFormat(RegType.PX, 8)] // 114
        CEP,

        [RegFormat(RegType.P9, 1)] // 122
        Sacado_Tipo,

        [RegFormat(RegType.P9, 13)] // 123
        SeuNumero,

        /// <summary>
        /// 1 - Sem Registro;
        /// 2 - Com registro, impresso pelo cliente
        /// 3 - Com registro impresso pelo BRB
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "2")] // 136
        Categoria_Cobranca,

        [RegFormat(RegType.PD, 8)] // 137
        Emissao,

        [RegFormat(RegType.P9, 2)] // 145
        Tipo_Documento,

        [RegFormat(RegType.P9, 1)] // 147
        Natureza,

        [RegFormat(RegType.P9, 1, Default = "1")] // 148
        Condicao,

        /// <summary>
        /// 02 - Real
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "02")] // 149
        Moeda,

        [RegFormat(RegType.P9, 3, Default = "070")] // 151
        Banco,

        [RegFormat(RegType.P9, 4)] // 154
        Agencia_Cobradora,

        [RegFormat(RegType.P9, 30)] // 158
        Praca_Cobranca,

        [RegFormat(RegType.PD, 8)] // 188
        DataVencimento,

        [RegFormat(RegType.PV, 14)] // 196
        Valor,

        [RegFormat(RegType.P9, 12)] // 210
        NossoNumero,

        [RegFormat(RegType.P9, 2)] // 222
        Tipo_Juros,

        [RegFormat(RegType.PV, 14)] // 224
        Valor_Juros,

        [RegFormat(RegType.PV, 14)] // 238
        ValorAbatimento,

        [RegFormat(RegType.P9, 2)] // 252
        Codigo_Desconto,

        [RegFormat(RegType.PD, 8)] // 254
        Data_Desconto,

        [RegFormat(RegType.PV, 14)] // 262
        Valor_Desconto,

        [RegFormat(RegType.P9, 2)] // 276
        Instrucao1,

        [RegFormat(RegType.P9, 2)] // 278
        Instrucao1_Prazo,

        [RegFormat(RegType.P9, 2)] // 280
        Instrucao2,

        [RegFormat(RegType.P9, 2)] // 282
        Instrucao2_Prazo,

        [RegFormat(RegType.P9, 5)] // 284
        Taxa,

        [RegFormat(RegType.PX, 40)] // 289
        Emitente,

        [RegFormat(RegType.PX, 40)] // 329
        Mensagem,

        [RegFormat(RegType.PX, 32)] // 369
        Branco,
    }

    /// <summary>
    /// Estrutura de Retorno BRB
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy")]
    public enum CNAB400BRBRetorno1
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
    [RegLayout(@"", Upper = true)]
    public enum CNAB400BRBTrailer
    {
    }

    #endregion
}