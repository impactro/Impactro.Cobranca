using Impactro.Cobranca;
using System;
using System.Runtime.InteropServices;

// Baseado no layout do Banco do Brasil 24/07/2015
namespace Impactro.Layout
{
    
    [ComVisible(false)]
    public class CNAB400Sicredi : CNAB400<CNAB400HeaderSicredi, CNAB400Remessa1Sicredi, CNAB400Trailer1Sicredi>
    {
        public override string Remessa()
        {

            string[] cBanco = Cedente.Banco.Split('-');
            Bancos banco = (Bancos)CobUtil.GetInt(cBanco[0]);

            if (banco != Bancos.SICREDI)
                throw new Exception("Esta classe é valida apenas para o Banco Sicredi");

            // Proximo item
            SequencialRegistro = 1;

            regArqHeader[CNAB400HeaderSicredi.CodCedente] = Cedente.CodCedente;
            regArqHeader[CNAB400HeaderSicredi.CNPJ] = Cedente.DocumentoNumeros;
            regArqHeader[CNAB400HeaderSicredi.Data] = DataHoje;
            regArqHeader[CNAB400HeaderSicredi.NumeroRemessa] = NumeroLote;
            regArqHeader[CNAB400HeaderSicredi.Sequencia] = SequencialRegistro++;
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
            Reg<CNAB400Remessa1Sicredi> regBoleto;

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                regBoleto = new Reg<CNAB400Remessa1Sicredi>();

                string cNossoNumero = boleto.NossoNumero;
                Banco_Sicredi.MontaNossoNumero(ref cNossoNumero, ref cAgenciaNumero, ref cModalidade, ref cCodCedente);

                regBoleto[CNAB400Remessa1Sicredi.NossoNumero] = cNossoNumero;
                regBoleto[CNAB400Remessa1Sicredi.NumeroDocumento] = boleto.NumeroDocumento;
                regBoleto[CNAB400Remessa1Sicredi.DataVencimento] = boleto.DataVencimento;
                regBoleto[CNAB400Remessa1Sicredi.ValorDocumento] = boleto.ValorDocumento;
                regBoleto[CNAB400Remessa1Sicredi.Especie] = EspecieSicred(boleto.Especie);
                regBoleto[CNAB400Remessa1Sicredi.Aceite] = boleto.Aceite == "A" ? "S" : "N";
                regBoleto[CNAB400Remessa1Sicredi.Data] = boleto.DataDocumento;
                regBoleto[CNAB400Remessa1Sicredi.DataEmissao] = boleto.DataDocumento;
                if(boleto.ParcelaTotal>0)
                {
                    regBoleto[CNAB400Remessa1Sicredi.TipoImpressao] = "B";
                    regBoleto[CNAB400Remessa1Sicredi.ParcelaNumero] = boleto.ParcelaNumero;
                    regBoleto[CNAB400Remessa1Sicredi.ParcelaTotal] = boleto.ParcelaTotal;
                }
                regBoleto[CNAB400Remessa1Sicredi.Instrucao] = boleto.Instrucao1;
                regBoleto[CNAB400Remessa1Sicredi.Protesto] = boleto.DiasProtesto >6 ? "06":"00";
                regBoleto[CNAB400Remessa1Sicredi.DiasProtesto] = boleto.DiasProtesto;
                regBoleto[CNAB400Remessa1Sicredi.PercentualMora] = boleto.PercentualMora;
                regBoleto[CNAB400Remessa1Sicredi.DataDesconto] = boleto.DataDesconto;
                regBoleto[CNAB400Remessa1Sicredi.ValorDesconto] = boleto.ValorDesconto;
                regBoleto[CNAB400Remessa1Sicredi.SacadoTipo] = sacado.Tipo;
                regBoleto[CNAB400Remessa1Sicredi.SacadoDocumento] = sacado.DocumentoNumeros;
                regBoleto[CNAB400Remessa1Sicredi.Sacado] = sacado.Sacado;
                regBoleto[CNAB400Remessa1Sicredi.Endereco] = sacado.Endereco;
                regBoleto[CNAB400Remessa1Sicredi.Cooperativa] = "00000";
                regBoleto[CNAB400Remessa1Sicredi.CEP] = sacado.CepNumeros;
                regBoleto[CNAB400Remessa1Sicredi.Sequencia] = SequencialRegistro++;

                // adiciona o boleto convertido em registro
                AddBoleto(regBoleto, boleto);

                AddOpcionais(boleto);
            }

            regArqTrailer[CNAB400Trailer1Sicredi.Conta] = CobUtil.GetInt(Cedente.Conta.Split('-')[0]);
            regArqTrailer[CNAB400Trailer1Sicredi.Sequencia] = SequencialRegistro;
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
            Layout retorno = new Layout(typeof(CNAB400Retorno1Sicredi));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            retorno.ForEach<CNAB400Retorno1Sicredi>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    DataDocumento = (DateTime)reg[CNAB400Retorno1Sicredi.OcorrenciaData],
                    NossoNumero = (string)reg[CNAB400Retorno1Sicredi.NossoNumero],
                    NumeroDocumento = (string)reg[CNAB400Retorno1Sicredi.NumeroDocumento],
                    ValorDocumento = (double)reg[CNAB400Retorno1Sicredi.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB400Retorno1Sicredi.Vencimento],
                    DataPagamento = (DateTime)reg[CNAB400Retorno1Sicredi.OcorrenciaData],
                    ValorPago = (double)reg[CNAB400Retorno1Sicredi.ValorPago],
                    Especie = SicredEspecie((string)reg[CNAB400Retorno1Sicredi.Especie])
                }, reg.OriginalLine)
            );
            return retorno;
        }

        public static string EspecieSicred(Especies especie)
        {
            switch (especie)
            {
                case Especies.DM:
                    return "A";
                case Especies.NP:
                    return "C";
                case Especies.NS:
                    return "E";
                case Especies.RC:
                    return "G";
                case Especies.LC:
                    return "H";
                case Especies.ND:
                    return "I";
                case Especies.DS:
                    return "J";

                default:
                    return "K";
            }
        }

        public static Especies SicredEspecie(string especie)
        {
            switch (especie)
            {
                case "A":
                    return Especies.DM;
                case "C":
                    return Especies.NP;
                case "E":
                    return Especies.NS;
                case "G":
                    return Especies.RC;
                case "H":
                    return Especies.LC;
                case "I":
                    return Especies.ND;
                case "J":
                    return Especies.DS;

                default:
                    return Especies.DM;
            }
        }
    }

     #region "Estruturas de Remessa"

     /// <summary>
     /// Header Geral do Arquivo 
     /// </summary>
     [RegLayout(@"^0", DateFormat8 = "yyyyMMdd", Upper = true)]
     public enum CNAB400HeaderSicredi
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
         /// Código do Cedente
         /// </summary>
         [RegFormat(RegType.P9, 5)] // 27
         CodCedente,

         /// <summary>
         /// CIC/CGC do Cedente (CNPJ)
         /// </summary>
         [RegFormat(RegType.P9, 14)] // 32
         CNPJ,

         /// <summary>
         /// Número da Conta Corrente: Número da conta onde está cadastrado o Convênio Líder do Cedente
         /// </summary>
         [RegFormat(RegType.PX, 31)] // 46
         Filler1,

         /// <summary>
         /// Nº do banco na câmara de compensação
         /// </summary>
         [RegFormat(RegType.P9, 3, Default="748")] // 77
         Banco_Codigo,

         /// <summary>
         /// Nome do Banco por Extenso
         /// </summary>
         [RegFormat(RegType.PX, 15, Default = "SICREDI")] // 80
         Banco_Nome,

         /// <summary>
         /// Data da Gravação do Arquivo
         /// </summary>
         [RegFormat(RegType.PD, 8)] //  95
         Data,

         /// <summary>
         /// Seqüencial da Remessa
         /// </summary>
         [RegFormat(RegType.PX, 8)] // 103
         Filler2,

         /// <summary>
         /// Numero da Remessa
         /// </summary>
         [RegFormat(RegType.P9, 7)] // 111
         NumeroRemessa,

         [RegFormat(RegType.PX, 273)] // 118 
         Filler3,

         /// <summary>
         /// Versão do sistema
         /// </summary>
         [RegFormat(RegType.PX, 4, Default="2.00")] // 391
         Versao,

         /// <summary>
         /// Número seqüencial do registro no arquivo
         /// </summary>
         [RegFormat(RegType.P9, 6)] // 395
         Sequencia

     }

     /// <summary>
     /// Estrutura de Remessa Sicredi
     /// Página 11
     /// </summary>
     [RegLayout(@"^1", DateFormat6 = "ddMMyy", Upper = true)]
     public enum CNAB400Remessa1Sicredi
     {
         /// <summary>
         /// Identificação do registro detalhe de estar “1” 
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="1")] // 1-1
         Identificador,

         /// <summary>
         /// Tipo de cobrança (“A” - Sicredi Com Registro) 
         /// </summary>
         [RegFormat(RegType.PX, 1, Default = "A")] // 2-2
         TipoCobranca,

         /// <summary>
         /// Tipo de carteira (“A” – Simples) 
         /// </summary>
         [RegFormat(RegType.PX, 1, Default = "A")] // 3-3
         TipoCarteira,

         /// <summary>
         /// Tipo de Impressão
         /// “A” – Normal, 
         /// “B” – Carnê
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="A")] // 4-4
         TipoImpressao,

         [RegFormat(RegType.PX, 12)] // 5-16
         Filler1,

         /// <summary>
         /// Tipo de moeda (“A” – Real) 
         /// </summary>
         [RegFormat(RegType.PX, 1, Default = "A")] // 17-17
         TipoMoeda,

         /// <summary>
         /// Tipo de desconto (“A” – Valor, “B” – Percentual) 
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="A")] // 18-18
         TipoDesconto,

         /// <summary>
         /// Tipo de juros (“A” – Valor, “B” – Percentual) 
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="A")] // 19-19
         TipoJuros,

         [RegFormat(RegType.PX, 28)] // 20-47
         Filler2,

         /// <summary>
         /// Nosso número Sicredi - Se a impressão for pela Sicredi (A) é possível deixar em branco (sem preenchimento - gerado automaticamente pelo Banco) ou informar Nosso Número devidamente preenchido. 
         /// </summary>
         [RegFormat(RegType.P9, 9)] // 48-56
         NossoNumero,

         [RegFormat(RegType.PX, 6)] // 57-62
         Filler3,

         /// <summary>
         /// Data da Instrução - O Formato da data de instrução do arquivo deve estar no padrão: AAAAMMDD 
         /// </summary>
         [RegFormat(RegType.PD, 8)] // 63-70
         Data,

         /// <summary>
         /// Campo alterado, quando instrução “31”, Campo deve estar vazio (sem preenchimento), só utilizar quando 109-110 for = 31. 
         /// Usar as seguintes opções: 
         /// A – Desconto; 
         /// B - Juros por dia; 
         /// C - Desconto por dia de antecipação; 
         /// D - Data limite para concessão de desconto; 
         /// E - Cancelamento de protesto automático; 
         /// F - Carteira de cobrança - não disponível. 
         /// </summary>
         [RegFormat(RegType.PX, 1)] // 71-71
         Alteracao,

         /// <summary>
         /// Postagem do título:
         /// “S” - Para postar o título diretamente ao pagador, 
         /// “N” - Não postar e remeter o título para o beneficiário
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="N")] // 72-72
         TipoPostagem,

         [RegFormat(RegType.PX, 1)] // 73-73
         Filler4,

         /// <summary>
         /// Emissão do boleto:
         /// “A” – Impressão é feita pelo Sicredi
         /// “B” – Impressão é feita pelo Beneficiário
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="B")] // 74-74
         Emissao,

         /// <summary>
         /// Número da parcela do carnê (Quando o tipo de impressão for “B – Carnê” (posição 004)) 
         /// </summary>
         [RegFormat(RegType.P9, 2)] // 75-76
         ParcelaNumero,

         /// <summary>
         /// Número total de parcelas do carnê (Quando o tipo de impressão for “B – Carnê” (posição 004)) 
         /// </summary>
         [RegFormat(RegType.P9, 2)] // 77-78
         ParcelaTotal,

         [RegFormat(RegType.PX, 4)] // 79-82
         Filler5,

         /// <summary>
         /// Valor de desconto por dia de antecipação 
         /// </summary>
         [RegFormat(RegType.PV, 10)] // 83-92
         ValorDesconto,

         /// <summary>
         /// % multa por pagamento em atraso (Alinhado à direita com zeros à esquerda, sem separador decimal ou preencher com zeros.) 
         /// </summary>
         [RegFormat(RegType.PV, 4)] // 93-96
         MultaPercentual,

         [RegFormat(RegType.PX, 12)] // 97-108
         Filler6,

         /// <summary>
         /// Instrução - Este campo só permite usar os seguintes códigos: 
         /// 01 - Cadastro de título; 
         /// 02 - Pedido de baixa; 
         /// 04 - Concessão de abatimento; 
         /// 05 - Cancelamento de abatimento concedido; 
         /// 06 - Alteração de vencimento; 
         /// 09 - Pedido de protesto; 
         /// 18 - Sustar protesto e baixar título; 
         /// 19 - Sustar protesto e manter em carteira; 
         /// 31 - Alteração de outros dados. 
         /// </summary>
         [RegFormat(RegType.P9, 2)] // 109-110
         Instrucao,

         /// <summary>
         /// Seu número - Este campo nunca pode se repetir (Diferente de branco) - normalmente usado neste campo o número da nota fiscal gerada para o pagador. 
         /// </summary>
         [RegFormat(RegType.P9, 10)] // 111-120
         NumeroDocumento,

         /// <summary>
         /// Data de vencimento - A data de vencimento deve ser sete dias MAIOR que o campo 151 a 156 “Data de emissão”. Formato: DDMMAA 
         /// </summary>
         [RegFormat(RegType.PD, 6)] // 121-126
         DataVencimento,

         /// <summary>
         /// Valor do título (Alinhado à direita e zeros à esquerda) 
         /// </summary>
         [RegFormat(RegType.PV, 13)] // 127-139
         ValorDocumento,

         [RegFormat(RegType.PX, 9)] // 140-148
         Filler7,

         /// <summary>
         /// Espécie de documento - Este campo só permite usar os seguintes códigos: 
         /// A - Duplicata Mercantil por Indicação; 
         /// B - Duplicata Rural; 
         /// C - Nota Promissória; 
         /// D - Nota Promissória Rural; 
         /// E - Nota de Seguros; 
         /// G – Recibo; 
         /// H - Letra de Câmbio; 
         /// I - Nota de Débito; 
         /// J - Duplicata de Serviço por Indicação; 
         /// K – Outros. O – Boleto Proposta 
         /// (Obs.: Se título possuir protesto automático, favor utilizar o código A, pois esta é uma espécie de documento que permite utilizar o protesto automático sem a utilização de um Sacador Avalista.) 
         /// </summary>
         [RegFormat(RegType.PX, 1)] // 149-149
         Especie,

         /// <summary>
         /// Aceite do título (“S” – sim, “N” – não) 
         /// </summary>
         [RegFormat(RegType.PX, 1)] // 150-150
         Aceite,

         /// <summary>
         /// Data de emissão - A data de emissão deve ser sete dias MENOR que o campo 121 a 126 “Data de vencimento”. Formato: DDMMAA 
         /// </summary>
         [RegFormat(RegType.PD, 6)] // 151-156
         DataEmissao,

         /// <summary>
         /// Instrução de protesto automático 
         /// “00” - Não protestar automaticamente, 
         /// “06” - Protestar automaticamente
         /// </summary>
         [RegFormat(RegType.P9, 2)] // 157-158
         Protesto,

         /// <summary>
         /// Número de dias p/protesto automático - Campo numérico - mínimo 03 (três) dias, 
         /// quando preenchido com 3 ou 4 dias o sistema comandará protesto em dias úteis após o vencimento. 
         /// Quando preenchido acima de 4 dias, o sistema comandará protesto em dias corridos após o vencimento. 
         /// </summary>
         [RegFormat(RegType.P9, 2)] // 159-160
         DiasProtesto,

         /// <summary>
         /// Valor/% de juros por dia de atraso - Preencher com valor (alinhados à direita com zeros à esquerda) ou preencher com zeros. 
         /// </summary>
         [RegFormat(RegType.PV, 13)] // 161-173
         PercentualMora,

         /// <summary>
         /// Data limite p/concessão de desconto - Informar data no padrão: DDMMAA ou preencher com zeros. 
         /// </summary>
         [RegFormat(RegType.PD, 6)] // 174-179
         DataDesconto,

         /// <summary>
         /// Valor/% do desconto - Informar valor do desconto (alinhado à direita e zeros à esquerda) ou preencher com zeros. 
         /// </summary>
         [RegFormat(RegType.PV, 13)] // 180-192
         PercentualDesconto,

         [RegFormat(RegType.P9, 13)] // 193-205
         Filler,

         /// <summary>
         /// Valor do abatimento - Informar valor do abatimento (alinhado à direita e zeros à esquerda) ou preencher com zeros. 
         /// </summary>
         [RegFormat(RegType.PV, 13)] // 206-218
         ValorAbatimento,

         /// <summary>
         /// Tipo de pessoa do pagador: PF ou PJ (“1” - Pessoa Física, “2” - Pessoa Jurídica) 
         /// </summary>
         [RegFormat(RegType.P9, 1)] // 219-219
         SacadoTipo,

         [RegFormat(RegType.P9, 1)] // 220-220
         Filler8,

         /// <summary>
         /// CPF/CNPJ do Pagador - Alinhado à direita e zeros à esquerda; (Obs: No momento dos testes para homologação estes dados devem ser enviados com informações válidas.) 
         /// </summary>
         [RegFormat(RegType.P9, 14)] // 221-234
         SacadoDocumento,

         /// <summary>
         /// Nome do pagador - Neste campo informar o nome do pagador sem acentuação ou caracteres especiais. 
         /// </summary>
         [RegFormat(RegType.PX, 40)] // 235-274
         Sacado,

         /// <summary>
         /// Endereço do pagador - Neste campo informar o endereço do pagador sem acentuação ou caracteres especiais. 
         /// </summary>
         [RegFormat(RegType.PX, 40)] // 275-314
         Endereco,

         /// <summary>
         /// Código do pagador na cooperativa beneficiário - Se pagador novo, o campo deve conter zeros. Para pagador já cadastrado, enviar o código enviado no primeiro arquivo de retorno ou sempre zeros quando o sistema do beneficiário não utiliza esse campo – campo alfanumérico; 
         /// </summary>
         [RegFormat(RegType.PX, 5)] // 315-319
         Cooperativa,

         [RegFormat(RegType.P9, 6)] // 320-325
         Filler9,

         [RegFormat(RegType.PX, 1)] // 326-326
         Filler10,

         /// <summary>
         /// CEP do pagador - Obrigatório ser um CEP Válido 
         /// </summary>
         [RegFormat(RegType.P9, 8)] // 327-334
         CEP,

         /// <summary>
         /// Código do Pagador junto ao cliente - Campo numérico (zeros quando inexistente) - Obs.: Para validações de arquivos deixar este campo com zeros, após a homologação pode ser usado o código do cliente, conforme informação do campo. 
         /// </summary>
         [RegFormat(RegType.P9, 5)] // 335-339
         CodigoPagador,

         /// <summary>
         /// CPF/CNPJ do Sacador Avalista - Alinhado à direita e zeros à esquerda. Deixar em branco caso não exista Sacador Avalista. O Sacador Avalista deve ser diferente do beneficiário e pagador. 
         /// </summary>
         [RegFormat(RegType.P9, 14)] // 340-353
         SacadorDocumento,

         /// <summary>
         /// Nome do Sacador Avalista - Deixar em brancos quando inexistente. Caso utilize usar sem acentuação ou caracteres especiais. 
         /// </summary>
         [RegFormat(RegType.PX, 41)] // 354-394
         Sacador,

         /// <summary>
         /// Número sequencial do registro - Neste campo sempre informar 000002 para primeiro registro de cobrança. Alinhado à direita e zeros à esquerda; 
         /// </summary>
         [RegFormat(RegType.P9, 6)] // 395-400
         Sequencia,
     }

    public enum CNAB400Trailer1Sicredi
    {
         /// <summary>
         /// Identificação do registro trailer
         /// </summary>
         [RegFormat(RegType.PX, 1, Default="9")] // 1-1
         Identificador,

         /// <summary>
         /// Identificação do arquivo remessa
         /// </summary>
         [RegFormat(RegType.PX, 1, Default = "1")] // 2-2
         Remessa,

         /// <summary>
         /// Número do Sicredi
         /// </summary>
         [RegFormat(RegType.P9, 3, Default="748")] // 3-5
         Banco,

         /// <summary>
         /// Código do beneficiário: Conta Corrente sem o DV ou conta beneficiário.
         /// </summary>
         [RegFormat(RegType.P9, 5)] // 6-10
         Conta,

         [RegFormat(RegType.PX, 384)]
         Filler,

         /// <summary>
         /// Número seqüencial do registro no arquivo
         /// </summary>
         [RegFormat(RegType.P9, 6)] // 395
         Sequencia
    }

    #endregion

    /// <summary>
    /// Estrutura de Retorno Sicredi
    /// Página 42
    /// </summary>
    [RegLayout(@"^1", DateFormat6 = "ddMMyy", DateFormat8 ="yyyyMMdd")]
    public enum CNAB400Retorno1Sicredi
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 1
        Controle_Registro,

        [RegFormat(RegType.PX, 12)] // 12-13
        Filler1,

        [RegFormat(RegType.PX, 1)] // 14
        Tipo_Cobranca,

        /// <summary>
        /// Código do pagador na cooperativa do beneficiário
        /// </summary>
        [RegFormat(RegType.PX, 5)] // 15-19
        Pagador_cooperativa,

        /// <summary>
        /// Código do pagador junto ao associado
        /// </summary>
        [RegFormat(RegType.PX, 5)] // 20-24
        Pagador_Associado,

        /// <summary>
        /// DDA: 1 – Boleto enviado a CIP/DDA, 2 – Boleto normal
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 25
        DDA,

        [RegFormat(RegType.PX, 22)] // 26-47
        Filler2,

        /// <summary>
        /// Identificação do Título no Banco
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 48-62
        NossoNumero,

        [RegFormat(RegType.PX, 46)] // 63-108
        Filler3,

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
        [RegFormat(RegType.PX, 10)] // 117-126
        NumeroDocumento,

        [RegFormat(RegType.PX, 20)] // 127-146
        Filler4,

        /// <summary>
        /// Data Vencimento do Título
        /// </summary>
        [RegFormat(RegType.PD, 6)] // 147
        Vencimento,

        /// <summary>
        /// Valor do Título
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 153-165
        ValorDocumento,

        [RegFormat(RegType.P9, 9)] // 166-174
        Filler5,

        /// <summary>
        /// Espécie do Título
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 175
        Especie,

        /// <summary>
        /// Valor Despesa
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 176-188
        ValorDespesas,

        /// <summary>
        /// Valor outras despesas
        /// </summary>
        [RegFormat(RegType.PV, 13)] // 189-201
        CustasProtesto,

        [RegFormat(RegType.PX, 26)] // 202-227
        Filler6,

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

        [RegFormat(RegType.PX, 2)] // 293-294
        Filler7,

        [RegFormat(RegType.PX, 1)] // 295
        Aeite,

        [RegFormat(RegType.PX, 23)] // 296-318
        Filler8,

        /// <summary>
        /// Motivo da ocorrencia
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 319-328
        MotivoOcorrencia,

        /// <summary>
        /// MEIO PELO QUAL O TÍTULO FOI LIQUIDADO
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 329-336
        DataCredito,

        [RegFormat(RegType.PX, 58)] // 337-394
        Filler9,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 395
        Sequencia
    }
}
