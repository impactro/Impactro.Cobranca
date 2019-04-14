using System;
using System.Collections.Generic;
using System.Text;
using Impactro.Cobranca;

namespace Impactro.Layout
{
    /*
    /// <summary>
    /// Layout para SICREDI no padrão CNAB240
    /// Validado pelo site: www.sicredi.com.br -> Produtos e Serviços -> Validador de Arquivos
    /// </summary>
    [Obsolete("Muitas coisas da classe base mudou não sendo mais garantido a geração do arquivo")]
    public class CNAB240Sicredi : CNAB240<
        CNAB240HeaderArquivoSicred , CNAB240HeaderLoteSicredi,
        CNAB240SegmentoPSicredi, CNAB240SegmentoQSicredi,
        CNAB240TrailerLoteSicredi, CNAB240TrailerArquivoSicred>
    {
        /// <summary>
        /// Contrutora: Informe o Cedente
        /// </summary>
        /// <param name="cedente">Informações do CEDENTE</param>
        public CNAB240Sicredi()
            : base()
        {
            AddTypes(typeof(CNAB240SegmentoGSicredi));
        }

        
        public CNAB240Sicredi(CedenteInfo cedente)
            : this()
        {
        }
        
        /// <summary>
        /// Retorna no layout CNAB240
        /// </summary>
        public override string Remessa()
        {
            // Limpa os objetos de saida/entrada
            Data.Clear();
            Clear();

            // Conteudo final do arquivo
            string cOut = "";

            /*
            // Define o Header Geral do Arquivo
            regHeaderArquivo[CNAB240HeaderArquivoSicred.Controle_Lote] = regCobTrailer[CNAB240TrailerLoteSicredi.Controle_Lote] = 0; // fixo
            regHeaderArquivo[CNAB240HeaderArquivoSicred.Arquivo_Layout] = 81;
            regHeaderArquivo[CNAB240HeaderArquivoSicred.Arquivo_Densidade] = 1600;
            regHeaderArquivo[CNAB240HeaderArquivoSicred.Empresa_AgenciaDV] = regCobHeader[CNAB240HeaderLoteSicredi.Empresa_AgenciaDV] = "";
            regHeaderArquivo[CNAB240HeaderArquivoSicred.Empresa_ContaDV] = regCobHeader[CNAB240HeaderLoteSicredi.Empresa_ContaDV] = "";

            regCobHeader[CNAB240HeaderLoteSicredi.Servico_Operacao] = "R";
            regCobHeader[CNAB240HeaderLoteSicredi.Servico_Layout] = 40;

            cOut = regArqHeader.Line + "\r\n";
            Sequencial++; // Inicia em 1, aqui vai para 2! Assim deve ser incrementado sempre apos a inserção de uma nova linha
            if (ShowDumpLine)
                cOut += regArqHeader.Dump;

            // Define o Header da cobrança
            cOut += regCobHeader.Line + "\r\n";
            Sequencial++;
            if (ShowDumpLine)
                cOut += regCobHeader.Dump;

            // Cotadores para o Trailer
            double nTotal = 0;
            int nSequenciaLote = 1; // Sequencial exclusivo do lote

            foreach (string n in this.Boletos.NossoNumeros)
            {
                BoletoInfo boleto = Boletos[n];

                // Define as informações do segmento P
                Reg<CNAB240SegmentoPSicredi> regP = new Reg<CNAB240SegmentoPSicredi>();
                regP[CNAB240SegmentoPSicredi.Banco] = (int)Bancos.SICREDI;
                regP[CNAB240SegmentoPSicredi.Lote] = Lote;
                regP[CNAB240SegmentoPSicredi.Nregistro] = nSequenciaLote;
                regP[CNAB240SegmentoPSicredi.ContaAgencia] = regArqHeader[CNAB240ArquivoHeader.Empresa_Agencia];
                regP[CNAB240SegmentoPSicredi.ContaAgenciaDV] = regArqHeader[CNAB240ArquivoHeader.Empresa_AgenciaDV];
                regP[CNAB240SegmentoPSicredi.ContaNumero] = regArqHeader[CNAB240ArquivoHeader.Empresa_Conta];
                regP[CNAB240SegmentoPSicredi.ContaNumeroDV] = regArqHeader[CNAB240ArquivoHeader.Empresa_ContaDV];
                regP[CNAB240SegmentoPSicredi.ContaDV] = regArqHeader[CNAB240ArquivoHeader.Empresa_DV];
                regP[CNAB240SegmentoPSicredi.NossoNumero] = boleto.NossoNumero;
                regP[CNAB240SegmentoPSicredi.Carteira] = 1;
                regP[CNAB240SegmentoPSicredi.NumeroDocumento] = boleto.NumeroDocumento;
                regP[CNAB240SegmentoPSicredi.Vencimento] = boleto.DataVencimento;
                regP[CNAB240SegmentoPSicredi.Valor] = boleto.ValorDocumento;
                regP[CNAB240SegmentoPSicredi.DataEmissao] = boleto.DataDocumento;
                regP[CNAB240SegmentoPSicredi.Identificacao] = boleto.BoletoID;
                
                cOut += regP.Line + "\r\n";
                Sequencial++;
                nSequenciaLote++;
                if (ShowDumpLine)
                    cOut += regP.Dump;

                // Define as informações do segmento Q
                Reg<CNAB240SegmentoQSicredi> regQ = new Reg<CNAB240SegmentoQSicredi>();
                regQ[CNAB240SegmentoQSicredi.Banco] = (int)Bancos.SICREDI;
                regQ[CNAB240SegmentoQSicredi.Lote] = Lote;
                regQ[CNAB240SegmentoQSicredi.Nregistro] = nSequenciaLote;
                regQ[CNAB240SegmentoQSicredi.Inscricao_Tipo] = boleto.Sacado.Tipo;
                regQ[CNAB240SegmentoQSicredi.Inscricao_Numero] = boleto.Sacado.DocumentoNumeros;
                regQ[CNAB240SegmentoQSicredi.Nome] = boleto.Sacado.Sacado;
                regQ[CNAB240SegmentoQSicredi.Endereco] = boleto.Sacado.Endereco;
                regQ[CNAB240SegmentoQSicredi.Bairro] = boleto.Sacado.Bairro;
                regQ[CNAB240SegmentoQSicredi.CEP] = boleto.Sacado.Cep.Replace("-","");
                regQ[CNAB240SegmentoQSicredi.Cidade] = boleto.Sacado.Cidade;
                regQ[CNAB240SegmentoQSicredi.UF] = boleto.Sacado.UF;

                cOut += regQ.Line + "\r\n";
                Sequencial++;
                nSequenciaLote++;
                if (ShowDumpLine)
                    cOut += regQ.Dump;

                nTotal += boleto.ValorDocumento;
            }

            // Define o Trailer da Remessa de Cobrança
            regCobTrailer[CNAB240TrailerLoteSicredi.Total_Registros] = nSequenciaLote + 1; // para incluir a contagem da propria linha 
            regCobTrailer[CNAB240TrailerLoteSicredi.Total_Quantidade] = Count;
            regCobTrailer[CNAB240TrailerLoteSicredi.Total_Valor] = nTotal;
            cOut += regCobTrailer.Line + "\r\n";
            Sequencial++;
            if (ShowDumpLine)
                cOut += regCobTrailer.Dump;

            // Define o Trailer do Arquivo Geral
            regArqTrailer[CNAB240ArquivoTrailer.Total_Lotes] = 1;
            regArqTrailer[CNAB240ArquivoTrailer.Total_Registros] = Sequencial;
            cOut += regArqTrailer.Line + "\r\n";
            if (ShowDumpLine)
                cOut += regArqTrailer.Dump;
             * 
             * 
             *

            return cOut;
        }

        /// <summary>
        /// Processa o Retorno
        /// </summary>
        /// <param name="cData">TXT de entrada</param>
        public override Layout Retorno(string cData)
        {
            throw new NotImplementedException();
        } 
          
    }

    [RegLayout(@"^1", DateFormat8 = "ddMMyyyy", DateFormat14 = "ddMMyyyyHHmmss")]
    public enum CNAB240HeaderArquivoSicred
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "104")] // 1
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 4
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "0")] // 8
        Registro,

        #endregion

        /// <summary>
        /// Uso Exclusivo FEBRABAN / CNAB G004 
        /// </summary>
        [RegFormat(RegType.PX, 9)] // 9-17
        CNAB1,

        /// <summary>
        /// Tipo de Inscrição da Empresa G005 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 18-18
        InscricaoTipo,

        /// <summary>
        /// Número de Inscrição da Empresa G006 
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 19-32
        InscricaoNumero,

        /// <summary>
        /// Uso Exclusivo Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 20)] // 33-52
        UsoCaixa1,

        /// <summary>
        /// Código Agência Mantenedora da Conta G008 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 53-57
        Agencia,

        /// <summary>
        /// Dígito Verificador da Agência G009 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 58-58
        AgenciaDAC,

        /// <summary>
        /// Código do Convênio no Banco G007 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 59-64
        CodigoCedente,

        /// <summary>
        /// Uso Exclusivo Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 65-71
        UsoCaixa2,

        /// <summary>
        /// Uso Exclusivo Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 72-72
        UsoCaixa3,

        /// <summary>
        /// Nome da Empresa G013 
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 73-102
        EmpresaNome,

        /// <summary>
        /// Nome do Banco G014 
        /// </summary>
        [RegFormat(RegType.PX, 30, Default = "CAIXA ECONOMICA FEDERAL")] // 103-132
        BancoNome,

        /// <summary>
        /// Uso Exclusivo FEBRABAN / CNAB G004 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 133-142
        CNAB2,

        /// <summary>
        /// Código Código Remessa / Retorno G015 
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 143-143
        ArquivoCodigo,

        /// <summary>
        /// Data e Hora de Geração Data de Geração do Arquivo G016 G017 DDMMAAAAHHMMSS 
        /// </summary>
        [RegFormat(RegType.PD, 14)] // 144-157
        Data,

        /// <summary>
        /// Seqüência (NSA) Número Seqüencial do Arquivo G018 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 158-163
        NumeroLote,

        /// <summary>
        /// N. da Versão do Layout do Arquivo G019 
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "050")] // 164-166
        Layout,

        /// <summary>
        /// Densidade de Gravação do Arquivo G020 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 167-171
        Densidade,

        /// <summary>
        /// Para Uso Reservado do Banco G021 
        /// </summary>
        [RegFormat(RegType.PX, 20)] // 172-191
        ReservadoBanco,

        /// <summary>
        /// Para Uso Reservado da Empresa G022 
        /// </summary>
        [RegFormat(RegType.PX, 20)] // 192-211
        ReservadoEmpresa,

        /// <summary>
        /// Versão Aplicativo CAIXA C077 
        /// </summary>
        [RegFormat(RegType.PX, 4)] // 212-215
        VersaoAplicativo,

        /// <summary>
        /// Uso Exclusivo FEBRABAN / CNAB G004 
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 216-240
        CNAB3

    }
    

    /// <summary>
    /// Tipo: 1
    /// Revisado SICREDI
    /// </summary>
    public enum CNAB240HeaderLoteSicredi
    {

        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3)]
        Controle_Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4)]
        Controle_Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")]
        Controle_Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Tipo da Operação G028
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        Servico_Operacao,

        /// <summary>
        /// Tipo do Serviço
        /// </summary>
        [RegFormat(RegType.P9, 2)]
        Servico_Servico,

        /// <summary>
        /// SICREDI 01=>Cobrança
        /// Generico
        /// '01' = Crédito em Conta Corrente
        /// '02' = Cheque Pagamento / Administrativo
        /// '03' = DOC/TED (1) (2)
        /// '04' = Cartão Salário (somente para Tipo de Serviço = '30')
        /// '05' = Crédito em Conta Poupança
        /// '10' = OP à Disposição
        /// ‘11’ = Pagamento de Contas e Tributos com Código de Barras
        /// ‘16’ = Tributo - DARF Normal
        /// ‘17’ = Tributo - GPS (Guia da Previdência Social)
        /// ‘18’ = Tributo - DARF Simples
        /// ‘19’ = Tributo - IPTU – Prefeituras
        /// '20' = Pagamento com Autenticação
        /// ‘21’ = Tributo – DARJ
        /// ‘22’ = Tributo - GARE-SP ICMS
        /// ‘23’ = Tributo - GARE-SP DR
        /// ‘24’ = Tributo - GARE-SP ITCMD
        /// ‘25’ = Tributo - IPVA
        /// ‘26’ = Tributo - Licenciamento
        /// ‘27’ = Tributo – DPVAT
        /// '30' = Liquidação de Títulos do Próprio Banco
        /// '31' = Pagamento de Títulos de Outros Bancos
        /// '40' = Extrato de Conta Corrente
        /// '41' = TED – Outra Titularidade (1)
        /// '43' = TED – Mesma Titularidade (1)
        /// ‘44’ = TED para Transferência de Conta Investimento
        /// '50' = Débito em Conta Corrente
        /// '70' = Extrato para Gestão de Caixa
        /// ‘71’ = Depósito Judicial em Conta Corrente
        /// ‘72’ = Depósito Judicial em Poupança
        /// ‘73’ = Extrato de Conta Investimento
        /// </summary>
        [RegFormat(RegType.PX, 2)]
        Servico_Forma,

        /// <summary>
        /// Nº da Versão do Layout do Lote
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "030")]
        Servico_Layout,

        /// <summary>
        /// Uso Exclusivo da FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        CNAB1,

        #endregion

        #region "Empresa"

        /// <summary>
        /// Tipo de Inscrição da Empresa:
        /// 0 - Isento / Não Informado
        /// 1 - CPF
        /// 2 - CGC / CNPJ
        /// 3 - PIS / PASEP
        /// 9 - Outros
        /// </summary>
        [RegFormat(RegType.P9, 1)]
        Empresa_Tipo,

        /// <summary>
        /// Número de Inscrição da Empresa
        /// </summary>
        [RegFormat(RegType.P9, 15)]
        Empresa_Numero,

        /// <summary>
        /// Código do Convênio no Banco
        /// </summary>
        [RegFormat(RegType.PX, 20)]
        Empresa_Convenio,

        /// <summary>
        /// Agência Mantenedora da Conta
        /// </summary>
        [RegFormat(RegType.P9, 5)]
        Empresa_Agencia,

        /// <summary>
        /// Dígito Verificador da Agência
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        Empresa_AgenciaDV,

        /// <summary>
        /// Número da Conta Corrente
        /// </summary>
        [RegFormat(RegType.P9, 12)]
        Empresa_Conta,

        /// <summary>
        /// Dígito Verificador da Conta
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        Empresa_ContaDV,

        /// <summary>
        /// Dígito Verificador da Ag/Conta
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        Empresa_DV,

        /// <summary>
        /// Nome da Empresa
        /// </summary>
        [RegFormat(RegType.PX, 30)]
        Empresa_Nome,

        /// <summary>
        /// Mensagem 1
        /// </summary>
        [RegFormat(RegType.PX, 40)]
        Empresa_Informacao1,

        /// <summary>
        /// Mensagem 2
        /// </summary>
        [RegFormat(RegType.PX, 40)]
        Empresa_Informacao2,

        #endregion

        /// <summary>
        /// Número Remessa/Retorno
        /// </summary>
        [RegFormat(RegType.PX, 8)]
        Controle_Remessa,

        /// <summary>
        /// Data de Gravação Remessa/Retorno
        /// </summary>
        [RegFormat(RegType.PD, 8)]
        Controle_Data,

        /// <summary>
        /// Data do Crédito
        /// </summary>
        [RegFormat(RegType.PD, 8)]
        Data_Credito,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 33)]
        CNAB2
    }

    /// <summary>
    /// SegmentoG
    /// 7480247100002G 01000001012345678000112IMPACTRO INFORMATICA LTDA ME  2012091300000000034780000000000093333           012345           DM201209030000000000000000000000000000000000000000000000 X
    /// </summary>
    public enum CNAB240SegmentoGSicredi
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3)]
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4)]
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1)]
        Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Nº Sequencial do Registro no Lote
        /// </summary>
        [RegFormat(RegType.P9, 5)]
        Nregistro,

        /// <summary>
        /// Cód. Segmento do Registro Detalhe
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        Segmento,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        CNAB,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2)]
        CodMov,

        #endregion

        /// <summary>
        /// Código de Barras 
        /// *G063
        /// </summary>
        [RegFormat(RegType.P9, 5)]
        CodigoBarras,

        #region "Cedente"

        /// <summary>
        /// Tipo de Inscrição do Cedente
        /// </summary>
        [RegFormat(RegType.P9, 1)]
        Tipo,

        /// <summary>
        /// Número de Inscrição do Cedente
        /// </summary>
        [RegFormat(RegType.P9, 15)]
        Inscricao,

        /// <summary>
        /// Nome do Cedente
        /// </summary>
        [RegFormat(RegType.PX, 30)]
        Nome,

        #endregion

        /// <summary>
        /// Data de Vencimento do Título
        /// *C012
        /// </summary>
        [RegFormat(RegType.PD, 8)]
        Vencimento,

        /// <summary>
        /// Valor Nominal do Título
        /// *G070
        /// </summary>
        [RegFormat(RegType.PV, 15)]
        Valor,

        /// <summary>
        /// Quantidade da Moeda 
        /// *G041
        /// </summary>
        [RegFormat(RegType.P9, 10)]
        Qtde,

        /// <summary>
        /// Código da Moeda 
        /// *G065
        /// </summary>
        [RegFormat(RegType.P9, 2)]
        Moeda,

        /// <summary>
        /// Número do Documento de Cobrança 
        /// *C011
        /// </summary>
        [RegFormat(RegType.PX, 15)]
        Documento,

        /// <summary>
        /// Agência Encarregada da Cobrança 
        /// *C014
        /// </summary>
        [RegFormat(RegType.P9, 5)]
        Agencia,

        /// <summary>
        /// Verificador 
        /// *G009
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        DV,

        /// <summary>
        /// Praça Cobradora 
        /// *B001
        /// </summary>
        [RegFormat(RegType.PX, 10)]
        Praça,

        /// <summary>
        /// Código da Carteira 
        /// *C006
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        Carteira,

        /// <summary>
        /// Espécie do Título 
        /// *C015
        /// </summary>
        [RegFormat(RegType.PX, 2)]
        Especie,
        /// <summary>
        /// Título Data da Emissão do Título 
        /// *G071
        /// </summary>
        [RegFormat(RegType.PD, 8)]
        DataEmissao,

        /// <summary>
        /// Juros de Mora por Dia 
        /// *C020
        /// </summary>
        [RegFormat(RegType.PV, 15)]
        Juros,

        /// <summary>
        /// Código do Desconto 1 
        /// *C021
        /// </summary>
        [RegFormat(RegType.P9, 1)]
        Desc1_Cod,

        /// <summary>
        /// Data do Desconto 1 
        /// *C022
        /// </summary>
        [RegFormat(RegType.PD, 8)]
        Desc1_Data,

        /// <summary>
        /// Percentual a ser Concedido 
        /// *C023
        /// </summary>
        [RegFormat(RegType.PV, 15)]
        Desc1_Valor,

        /// <summary>
        /// Código para Protesto 
        /// *C026
        /// </summary>
        [RegFormat(RegType.P9, 1)]
        Protesto_Codigo,

        /// <summary>
        /// Número de Dias para Protesto 
        /// *C027
        /// </summary>
        [RegFormat(RegType.P9, 2)]
        Protesto_Prazo,

        /// <summary>
        /// Data Limite para Pagamento do Título 
        /// *C075
        /// </summary>
        [RegFormat(RegType.PD, 8)]
        DataLimite,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB 
        /// *G004
        /// </summary>
        [RegFormat(RegType.PX, 1)]
        CNABzz

    }

    /// <summary>
    /// Pagina 16: 3.4.1.2.1 Segmento P (Obrigatório Remessa)
    /// </summary>
    [RegLayout("", DateFormat8 = "ddMMyyyy", Upper = true)]
    public enum CNAB240SegmentoPSicredi
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 1
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 4
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 8
        Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Nº Sequencial do Registro no Lote
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 9
        Nregistro,

        /// <summary>
        /// Cód. Segmento do Registro Detalhe
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "P")] // 14
        Segmento,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 15
        CNAB,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "1")] // 16
        CodMov,

        #endregion
        /// <summary>
        /// Agencia mantenedora da conta
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 18
        ContaAgencia,

        /// <summary>
        /// Dígito verificador da agencia
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 23
        ContaAgenciaDV,

        /// <summary>
        /// Numero da conta corrente
        /// </summary>
        [RegFormat(RegType.P9, 12)] // 24
        ContaNumero,

        /// <summary>
        /// Dígito verificador da conta
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 36
        ContaNumeroDV,

        /// <summary>
        /// Dígito verificador da coop/ag/conta
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 37
        ContaDV,

        /// <summary>
        /// Identificação do titulo no banco C069
        /// (zeros a direita)
        /// </summary>
        [RegFormat(RegType.PX, 20)] // 38
        NossoNumero,

        /// <summary>
        /// Código da carteira C006
        /// 1-Cobrança Simples
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "1")] // 58
        Carteira,

        /// <summary>
        /// Forma de cadastro do tipo no banco C007
        /// 1-Cobrança Registrada
        /// 2-Cobrança sem Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 59
        FormaCadastro,

        /// <summary>
        /// Tipo de documento C008
        /// 1-Tradicional
        /// 2-Escritural
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "1")] // 60
        TipoDocumento,

        /// <summary>
        /// Identificaçã da emissão do boleto C009
        /// 1-SICRED emite (auto-envelopável)
        /// 2-CLIENTE emite
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "2")] // 61
        EmissaoBloqueto,

        /// <summary>
        /// Identificação da distribuição
        /// 1-SICRED distribui
        /// 2-CLIENTE distribui
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "2")] // 64
        DistribuicaoBloqueto,

        /// <summary>
        /// Numero do documento de cobrança
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 63
        NumeroDocumento,

        /// <summary>
        /// Data do vencimento
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 78
        Vencimento,

        /// <summary>
        /// Valor nominal do titulo
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 86
        Valor,

        /// <summary>
        /// Cooperativa / Agencia cobradora
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 101
        AgenciaCobradora,

        /// <summary>
        /// Dígito de verificação da Coop / Agencia
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 106
        AgenciaCobradoraDV,

        /// <summary>
        /// Espécie do titulo C015
        /// 03-DMI
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "03")] // 107
        Especie,

        /// <summary>
        /// Identificação do titulo aceito/não aceito C016
        /// A-Aceito
        /// N-Não Aceito
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "A")] // 109
        Aceite,

        /// <summary>
        /// Data de emissão do titulo
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 110
        DataEmissao,

        /// <summary>
        /// Código do Juro mora C018
        /// 1-Valor por dia
        /// 2-Taxa Mensal
        /// 3-Isento
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 118
        JurosCodigo,

        /// <summary>
        /// Data do juro de mora
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 119
        JurosData,

        /// <summary>
        /// Juros de mora por dia/taxa
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 127
        JurosValor,

        /// <summary>
        /// Código de desconto
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 142
        DescontoCodigo,

        /// <summary>
        /// Data do desconto
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 143
        DescontoData,

        /// <summary>
        /// Valor percentual a ser consedido
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 151
        DescontoValor,

        /// <summary>
        /// Valor do IOF a ser recolhido
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 166
        IOF,

        /// <summary>
        /// Valor do Abatimento
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 181
        Abatimento,

        /// <summary>
        /// Identificação do titulo na empresa
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 196
        Identificacao,

        /// <summary>
        /// Código para identificar o tipo de protesto
        /// 1-Protestar dias corrido
        /// 3-Não Protestar
        /// 9-Cancelamento protesto automatico
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 221
        ProtestoCodigo,

        /// <summary>
        /// Numero de dias apos a data do vencimento para protesto
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 222
        ProtestoPrazo,

        /// <summary>
        /// Tipo de procedimento a ser adotado
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 224
        DevolucaoCodigo,

        /// <summary>
        /// Numero de dias corridos apos a data de vencimento de um titulo não pago que deverá ser baixado e devolvido ao cedente (padrão 60 dias)
        /// </summary>
        [RegFormat(RegType.PX, 3, Default = "060")] // 225
        DevolucaoPrazo,

        /// <summary>
        /// Código da moeda (9-Real exclusivo)
        /// </summary>
        [RegFormat(RegType.PX, 2, Default = "09")] // 228
        Moeda,

        /// <summary>
        /// Numero do contrato da operação de credito
        /// </summary>
        [RegFormat(RegType.P9, 10)] // 230
        Contrato,

        /// <summary>
        /// Uso FEBRABAN
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 240
        CNABz
    }

    /// <summary>
    /// Segmento Q (Obrigatório Remessa)
    /// </summary>
    public enum CNAB240SegmentoQSicredi
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 1
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 4
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 8
        Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Nº Sequencial do Registro no Lote
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 9
        Nregistro,

        /// <summary>
        /// Cód. Segmento do Registro Detalhe
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "Q")] // 14
        Segmento,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 15
        CNAB,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "1")] // 16
        CodMov,

        #endregion

        /// <summary>
        /// Tipo de inscrição
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 18
        Inscricao_Tipo,

        /// <summary>
        /// CPF/CNPJ
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 19
        Inscricao_Numero,

        /// <summary>
        /// Nome
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 34
        Nome,

        /// <summary>
        /// Endereço
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 74
        Endereco,

        /// <summary>
        /// Bairro
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 114
        Bairro,

        /// <summary>
        /// CEP
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 129
        CEP,

        /// <summary>
        /// Cidade
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 137
        Cidade,

        /// <summary>
        /// Unidade Federal (estado)
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 152
        UF,

        /// <summary>
        /// Tipo de Sacador
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 154
        Sacador_Tipo,

        /// <summary>
        /// Numedo CPF/CNPJ do sacador
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 155
        Sacador_Numero,

        /// <summary>
        /// Nome do sacador
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 170
        Sacador_Nome,

        [RegFormat(RegType.P9, 3)] // 210
        Correspondente_Banco,

        [RegFormat(RegType.PX, 20)] // 213
        Correspondente_NossoNumero,

        [RegFormat(RegType.PX, 8)] // 233
        Brancos
    }

    /// <summary>
    /// Tipo: 5
    /// </summary>
    public enum CNAB240TrailerLoteSicredi
    {
        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 1
        Controle_Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 4
        Controle_Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "5")] // 8
        Controle_Registro,

        /// <summary>
        /// Uso Exclusivo FEBRABAN / CNAB
        /// </summary>
        [RegFormat(RegType.PX, 9)] // 9
        CNAB_Brancos1,

        /// <summary>
        /// Quantidade de Registros do Lote
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 18
        Total_Registros,


        /// <summary>
        /// Quantidade de titulos em carteira (simples)
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 24
        Total_Quantidade,

        /// <summary>
        /// Valor Total de titulos em carteira (simples)
        /// </summary>
        [RegFormat(RegType.PV, 17)] // 30
        Total_Valor,


        /// <summary>
        /// Quantidade de titulos Vinculada em carteira 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 47
        Vinculada_Quantidade,

        /// <summary>
        /// Valor Total de titulos Vinculada em carteira
        /// </summary>
        [RegFormat(RegType.PV, 17)] // 53
        Vinculada_Valor,


        /// <summary>
        /// Quantidade de titulos Caucinados
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 70
        Caucinado_Quantidade,

        /// <summary>
        /// Valor Total de titulos Caucinados
        /// </summary>
        [RegFormat(RegType.PV, 17)] // 76
        Caucinado_Valor,


        /// <summary>
        /// Quantidade de titulos descontado
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 93
        Descontado_Quantidade,

        /// <summary>
        /// Valor Total de titulos descontados
        /// </summary>
        [RegFormat(RegType.PV, 17)] // 99
        Descontado_Valor,

        /// <summary>
        /// Aviso
        /// </summary>
        [RegFormat(RegType.PX, 8)] // 116
        Aviso,

        /// <summary>
        /// Uso Exclusivo FEBRABAN / CNAB
        /// </summary>
        [RegFormat(RegType.PX, 117)] // 124
        CNAB_Brancos2

    }

    public enum CNAB240TrailerArquivoSicred
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "104")] // 1
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4, Default = "9999")] // 4
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "9")] // 8
        Registro,

        [RegFormat(RegType.PX, 9)]
        CNAB1,

        #endregion

        [RegFormat(RegType.P9, 6)]
        LotesQTD,

        [RegFormat(RegType.P9, 6)]
        RegistrosQTD,

        [RegFormat(RegType.PX, 6)]
        CNAB2,

        [RegFormat(RegType.PX, 205)]
        CNAB3,

    }
*/
}
