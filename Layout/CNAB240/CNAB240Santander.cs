using System;
using Impactro.Cobranca;

namespace Impactro.Layout
{
    // Baseado no layout 240 da Caixa (17/12/2016)
    /// <summary>
    /// Layout Sanrander CNAB240
    /// </summary>
    public class CNAB240Santander : CNAB240<
        CNAB240HeaderArquivoSantander, CNAB240HeaderLoteSantander,
        CNAB240SegmentoPSantander, CNAB240SegmentoQSantander,
        CNAB240TrailerLoteSantander, CNAB240TrailerArquivoSantander>
    {
        /// <summary>
        /// Layout Santander
        /// </summary>
        public CNAB240Santander()
            : base()
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

            string[] cAgenciaDig = Cedente.Agencia.Split('-');
            string[] cContaDig = Cedente.Conta.Split('-');

            // Header do Arquivo
            regHeaderArquivo[CNAB240HeaderArquivoSantander.CodigoTransmissao] = Cedente.CodCedente; // por hora estou colocando o Código do Cedente, mas é preciso confirmar junto ao banco
            regHeaderArquivo[CNAB240HeaderArquivoSantander.InscricaoTipo] = Cedente.Tipo;
            regHeaderArquivo[CNAB240HeaderArquivoSantander.InscricaoNumero] = Cedente.DocumentoNumeros;
            regHeaderArquivo[CNAB240HeaderArquivoSantander.EmpresaNome] = Cedente.Cedente;
            regHeaderArquivo[CNAB240HeaderArquivoSantander.Data] = DataHoje;
            Add(regHeaderArquivo);

            // Header do lote
            regHeaderLote[CNAB240HeaderLoteSantander.Lote] = SequencialLote;
            regHeaderLote[CNAB240HeaderLoteSantander.InscricaoTipo] = Cedente.Tipo;
            regHeaderLote[CNAB240HeaderLoteSantander.CodigoTransmissao] = Cedente.CodCedente;
            regHeaderLote[CNAB240HeaderLoteSantander.InscricaoNumero] = Cedente.DocumentoNumeros;
            regHeaderLote[CNAB240HeaderLoteSantander.Beneficiario] = Cedente.Cedente;
            regHeaderLote[CNAB240HeaderLoteSantander.NumeroRemessaRetorno] = NumeroLote;
            regHeaderLote[CNAB240HeaderLoteSantander.Data] = DataHoje;
            Add(regHeaderLote);

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB240SegmentoPSantander> regP;
            Reg<CNAB240SegmentoQSantander> regQ;

            SequencialRegistro = 1;
            double ValorTotal = 0;

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                // Define as informações do segmento P
                regP = new Reg<CNAB240SegmentoPSantander>();

                regP[CNAB240SegmentoPSantander.Lote] = SequencialLote;
                regP[CNAB240SegmentoPSantander.Nregistro] = SequencialRegistro++;
                regP[CNAB240SegmentoPSantander.Agencia] = cAgenciaDig[0];
                regP[CNAB240SegmentoPSantander.AgenciaDAC] = cAgenciaDig[1];
                regP[CNAB240SegmentoPSantander.Conta] = cContaDig[0];
                regP[CNAB240SegmentoPSantander.ContaDAC] = cContaDig[1];
                //regP[CNAB240SegmentoPSantander.ContaCobranca] = ???
                regP[CNAB240SegmentoPSantander.NossoNumero] = boleto.NossoNumero;
                regP[CNAB240SegmentoPSantander.NumeroDocumento] = boleto.NumeroDocumento;
                regP[CNAB240SegmentoPSantander.Vencimento] = boleto.DataVencimento;
                regP[CNAB240SegmentoPSantander.ValorDocumento] = boleto.ValorDocumento;
                if(boleto.ValorMora >= 0.01)
                {
                    regP[CNAB240SegmentoPSantander.JurosCodigo] = 1;
                    regP[CNAB240SegmentoPSantander.JurosData] = boleto.DataVencimento.AddDays(1);
                    regP[CNAB240SegmentoPSantander.JurosMora] = boleto.ValorMora;
                }
                else
                    regP[CNAB240SegmentoPSantander.JurosCodigo] = 3; // isento

                regP[CNAB240SegmentoPSantander.Especie] = (int)boleto.Especie;
                regP[CNAB240SegmentoPSantander.Aceite] = boleto.Aceite;
                regP[CNAB240SegmentoPSantander.Emissao] = boleto.DataDocumento;
                regP[CNAB240SegmentoPSantander.UsoEmpresaCedente] = boleto.NossoNumero;
                int nDiasBaixa;
                if(boleto.DiasProtesto > 1)
                {
                    regP[CNAB240SegmentoPSantander.ProtestoCodigo] = 1;
                    regP[CNAB240SegmentoPSantander.ProtestoPrazo] = boleto.DiasProtesto;
                    regP[CNAB240SegmentoPSantander.BaixaDevolucaoCodigo] = 2; // Não Baixar / Não Devolver
                    // Baixar em no minimo 8 dias apos o protesto baseado na data de geração do arquivo
                    nDiasBaixa = 0; // boleto.DataVencimento.AddDays(boleto.DiasProtesto + 8).Subtract(DateTime.Now).Days;
                }
                else
                {
                    regP[CNAB240SegmentoPSantander.ProtestoCodigo] = 3; // não protestar
                    regP[CNAB240SegmentoPSantander.BaixaDevolucaoCodigo] = boleto.DiasBaixa > 0 ? 1 : 2; // 1 => Baixar / Devolver
                    if (boleto.DiasBaixa > 0)
                        nDiasBaixa = boleto.DataVencimento.AddDays(boleto.DiasBaixa).Subtract(DataHoje).Days;
                    else
                        nDiasBaixa = 0;
                }
                if (nDiasBaixa <= 0)
                    nDiasBaixa = 0;
                else if (nDiasBaixa < 5)
                    nDiasBaixa = 5;
                else if (nDiasBaixa > 120)
                    nDiasBaixa = 120;

                regP[CNAB240SegmentoPSantander.BaixaDevolucaoPrazo] = nDiasBaixa;
                ValorTotal += boleto.ValorDocumento;

                // Define as informações do segmento Q
                regQ = new Reg<CNAB240SegmentoQSantander>();
                regQ[CNAB240SegmentoQSantander.Lote] = SequencialLote;
                regQ[CNAB240SegmentoQSantander.Nregistro] = SequencialRegistro++;
                regQ[CNAB240SegmentoQSantander.Sacado_Tipo] = boleto.Sacado.Tipo;
                regQ[CNAB240SegmentoQSantander.Sacado_Numero] = CobUtil.GetLong(CobUtil.SoNumeros(boleto.Sacado.DocumentoNumeros));
                regQ[CNAB240SegmentoQSantander.Nome] = boleto.Sacado.Sacado;
                regQ[CNAB240SegmentoQSantander.Endereco] = boleto.Sacado.Endereco;
                regQ[CNAB240SegmentoQSantander.Bairro] = boleto.Sacado.Bairro;
                regQ[CNAB240SegmentoQSantander.CEP] = boleto.Sacado.CepNumeros;
                regQ[CNAB240SegmentoQSantander.Cidade] = boleto.Sacado.Cidade;
                regQ[CNAB240SegmentoQSantander.UF] = boleto.Sacado.UF;
                long avalista = CobUtil.GetLong(boleto.Sacado.AvalistaNumeros);
                if(avalista>0)
                {
                    regQ[CNAB240SegmentoQSantander.Avalista_Tipo] = boleto.Sacado.AvalistaTipo;
                    regQ[CNAB240SegmentoQSantander.Avalista_Numero] = avalista;
                    regQ[CNAB240SegmentoQSantander.Avalista_Nome] = boleto.Sacado.Avalista;
                }

                // adiciona o boleto convertido em registro
                // AddSegmentoPQ(regP, regQ);
                AddBoleto(regP, boleto);
                AddBoleto(regQ, boleto);

                AddOpcionais(boleto);
            }

            regTrailerLote[CNAB240TrailerLoteSantander.Lote] = SequencialLote;
            regTrailerLote[CNAB240TrailerLoteSantander.QTD] = SequencialRegistro+1; // tem que incluir o header
            Add(regTrailerLote);

            regTrailerArquivo[CNAB240TrailerArquivoSantander.LotesQTD] = 1;
            regTrailerArquivo[CNAB240TrailerArquivoSantander.RegistrosQTD] = itens.Count + 1;
            Add(regTrailerArquivo);

            // Gera o Texto de saida da forma padrão
            return this.Conteudo;
        }

        /// <summary>
        /// Processa o Retorno
        /// </summary>
        /// <param name="cData">TXT de entrada</param>
        public override Layout Retorno(string cData)
        {
            Layout retorno = new Layout(typeof(CNAB240SegmentoTSantander), typeof(CNAB240SegmentoUSantander));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            BoletoInfo blt = null;
            retorno.ForEachIReg(ireg =>
            {
                if (ireg.NameType == typeof(CNAB240SegmentoTSantander))
                {
                    if (blt != null)
                        Boletos.Add(blt);

                    // Para registros Tipo T cria o boleto
                    var reg = ireg as Reg<CNAB240SegmentoTSantander>;
                    blt = new BoletoInfo()
                    {
                        NossoNumero = reg[CNAB240SegmentoTSantander.NossoNumero].ToString(),
                        ValorDocumento = (double)reg[CNAB240SegmentoTSantander.ValorDocumento],
                        DataVencimento = (DateTime)reg[CNAB240SegmentoTSantander.DataVencimento]
                        //                        ValorPago = (double)reg[CNAB240SegmentoTSantander.ValorDocumento]
                    };
                    blt.LinhaOrigem = reg.OriginalLine;
                }
                else if (ireg.NameType == typeof(CNAB240SegmentoUSantander))
                {
                    // registros de outros tipos busca na lista de boletos existentes uma instancia válida para atualizar os dados complementares
                    var reg = ireg as Reg<CNAB240SegmentoUSantander>;
                    blt.ValorAcrescimo = (double)reg[CNAB240SegmentoUSantander.ValorAcrescimos];
                    blt.ValorDesconto = (double)reg[CNAB240SegmentoUSantander.ValorDesconto];
                    blt.ValorDesconto2 = (double)reg[CNAB240SegmentoUSantander.ValorAbatimento];
                    blt.ValorIOF = (double)reg[CNAB240SegmentoUSantander.ValorIOF];
                    blt.ValorPago = (double)reg[CNAB240SegmentoUSantander.ValorPago];
                    blt.ValorLiquido = (double)reg[CNAB240SegmentoUSantander.ValorLiquido];
                    blt.DataPagamento = blt.DataProcessamento = (DateTime)reg[CNAB240SegmentoUSantander.DataOcorrencia];
                    blt.DataCredito = (DateTime)reg[CNAB240SegmentoUSantander.DataCredito];
                    blt.DataTarifa = (DateTime)reg[CNAB240SegmentoUSantander.DataTarifa];
                }
            });
            if (blt != null)
                Boletos.Add(blt);
            return retorno;
        }
    }

    /// <summary>
    /// REGISTRO HEADER DO ARQUIVO REMESSA SANTANDER
    /// </summary>
    [RegLayout(@"^03300000", DateFormat8 = "ddMMyyyy", DateFormat14 = "ddMMyyyyHHmmss")]
    public enum CNAB240HeaderArquivoSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço:
        /// Identifica um Lote de Serviço.Criado e controlado pelo responsável pela geração magnética dos dados contidos no arquivo.
        /// Preencher com "0001" para o primeiro lote do arquivo, mantendo seqüencial crescente para os demais 
        /// (número do lote anterior acrescido de 1). O número não poderá ser repetido dentro do arquivo.
        /// Se registro for Header do Arquivo preencher com "0000"
        /// Se registro for Trailer do Arquivo preencher com "9999"
        /// </summary>
        [RegFormat(RegType.P9, 4, Default = "0")] // 4-7
        Lote,

        /// <summary>
        /// Tipo de Registro:
        /// 0 Hearder do arquivo
        /// 1 Hearder do lote
        /// 3 Detalhe
        /// 5 Trailer do lote
        /// 9 Trailer do arquivo
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "0")] // 8-8
        Registro,

        #endregion

        /// <summary>
        /// Reservado (uso Banco)
        /// </summary>
        [RegFormat(RegType.PX, 8)] // 9-16
        CNAB1,

        /// <summary>
        /// Tipo de inscrição da empresa
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 17-17
        InscricaoTipo,

        /// <summary>
        /// Número de Inscrição da Empresa 
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 18-32
        InscricaoNumero,

        /// <summary>
        /// Código de Transmissão:
        /// Informação cedida pelo banco que identifica o arquivo remessa do cliente.
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 33-47
        CodigoTransmissao,

        /// <summary>
        /// Reservado (uso Banco)
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 48-72
        CNAB2,

        /// <summary>
        /// Nome da Empresa 
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 73-102
        EmpresaNome,

        /// <summary>
        /// Nome do Banco 
        /// </summary>
        [RegFormat(RegType.PX, 30, Default = "SANTANDER")] // 103-132
        BancoNome,

        /// <summary>
        /// Reservado (uso Banco) 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 133-142
        CNAB3,

        /// <summary>
        /// Código Código Remessa
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 143-143
        ArquivoCodigo,

        /// <summary>
        /// Data de geração do arquivo
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 144-151
        Data,

        /// <summary>
        /// Reservado (uso Banco) 
        /// </summary>
        [RegFormat(RegType.PX, 6)] // 152-157
        CNAB4,

        /// <summary>
        /// Nº seqüencial do arquivo 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 158-163
        Sequencial,

        /// <summary>
        /// Nº da versão do layout do arquivo
        /// </summary>
        [RegFormat(RegType.P9, 3, Default ="040")] // 164-166
        VersaoAplicativo,

        /// <summary>
        /// Reservado (uso Banco) 
        /// </summary>
        [RegFormat(RegType.PX, 74)] // 167-240
        CNAB5

    }

    /// <summary>
    /// REGISTRO HEADER DO LOTE REMESSA
    /// </summary>
    [RegLayout(@"^03300011R01", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240HeaderLoteSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4, Default = "1")] // 4-7
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 8-8
        Registro,

        #endregion

        /// <summary>
        /// Tipo de Operação 
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "R")] // 9-9
        ServicoOperacao,

        /// <summary>
        /// Tipo de Serviço 
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "01")] // 10-11
        ServicoTipo,

        /// <summary>
        /// Reservado (uso Banco)
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 12-13
        CNAB1,

        /// <summary>
        /// Nº da versão do layout do lote
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "030")] // 14-16
        Layout,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB G004 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 17-17
        CNAB2,

        /// <summary>
        /// Tipo de Inscrição da Empresa 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 18-18
        InscricaoTipo,

        /// <summary>
        /// Número Nº de Inscrição da Empresa 
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 19-33
        InscricaoNumero,

        /// <summary>
        /// Reservado (uso Banco) 
        /// </summary>
        [RegFormat(RegType.P9, 20)] // 34-53
        CNAB3,

        /// <summary>
        /// Código de Transmissão
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 54-68
        CodigoTransmissao,

        /// <summary>
        /// Reservado uso Banco 
        /// </summary>
        [RegFormat(RegType.PX, 5)] // 69-73
        CNAB4,

        /// <summary>
        /// Nome do Beneficiário 
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 74-103
        Beneficiario,

        /// <summary>
        /// Mensagem 1 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 104-143
        Informacao1,

        /// <summary>
        /// Mensagem 2
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 144-183
        Informacao2,

        /// <summary>
        /// Número remessa/retorno 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 184-191
        NumeroRemessaRetorno,

        /// <summary>
        /// Gravação Data de Gravação Remessa/Retorno 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 192-199
        Data,

        /// <summary>
        /// Reservado (uso Banco) 
        /// </summary>
        [RegFormat(RegType.PX, 41)] // 200-240
        CNAB5
    }

    /// <summary>
    /// REGISTRO DETALHE - SEGMENTO P REMESSA
    /// </summary>
    [RegLayout(@"^03300013\d{5}P", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoPSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4, Default = "1")] // 4-7
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 8-8
        Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Nº Sequencial do Registro no Lote
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 9-13
        Nregistro,

        /// <summary>
        /// Cód. Segmento do Registro Detalhe
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "P")] // 14-14
        Segmento,

        [RegFormat(RegType.PX, 1)] // 15-15
        CNAB1,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "1")] // 16-17
        CodMov,

        #endregion

        /// <summary>
        /// Código Agência Mantenedora da Conta
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 18-21
        Agencia,

        /// <summary>
        /// Dígito Verificador da Agência
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 22-22
        AgenciaDAC,

        /// <summary>
        /// Número da conta corrente 
        /// </summary>
        [RegFormat(RegType.P9, 9)] // 23-31
        Conta,

        /// <summary>
        /// Dígito verificador da conta
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 32-32
        ContaDAC,

        /// <summary>
        /// Conta cobrança 
        /// </summary>
        [RegFormat(RegType.P9, 9)] // 33-41
        ContaCobranca,

        /// <summary>
        /// Dígito da conta cobrança 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 41-42
        ContaCobrancaDAC,

        [RegFormat(RegType.PX, 2)] // 43-44
        CNAB2,

        /// <summary>
        /// Identificação do título no Banco
        /// </summary>
        [RegFormat(RegType.P9, 13, Default="1")] // 45-57
        NossoNumero,

        /// <summary>
        /// Tipo de cobrança
        /// </summary>
        [RegFormat(RegType.P9, 1, Default="1")] // 58-58
        TipoCobranca,

        /// <summary>
        /// 9 Forma de Cadastramento
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 59-59
        FormaCadastro,

        /// <summary>
        /// Tipo de Documento 
        /// </summary>
        [RegFormat(RegType.PX, 1, Default="2")] // 60
        TipoDocumento,

       
        [RegFormat(RegType.PX, 2)] // 61-62
        CNAB3,

        /// <summary>
        /// (Seu Nº) Número do Documento de Cobrança 
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 63-77
        NumeroDocumento,

        /// <summary>
        /// Data de Vencimento do Título
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 78-85
        Vencimento,

        /// <summary>
        /// Valor Nominal do Título
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 86-100
        ValorDocumento,

        /// <summary>
        /// Agência Encarregada da Cobrança *C014 
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 101-104
        AgenciaCobradora,

        /// <summary>
        /// Verificador da Agência *C014 
        /// </summary>
        [RegFormat(RegType.PX, 1, Default ="0")] // 105-105
        AgenciaCobradoraDV,

        [RegFormat(RegType.PX, 1)] // 106-106
        CNAB4,

        /// <summary>
        /// Espécie do Título
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 107-108
        Especie,

        /// <summary>
        /// Identific. de Título Aceito/Não Aceito
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 109
        Aceite,

        /// <summary>
        /// Emissão do Título Data da Emissão do Título
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 110-117
        Emissao,

        /// <summary>
        /// Cód. Juros Mora Código do Juros de Mora
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 118
        JurosCodigo,

        /// <summary>
        /// Juros Mora Data do Juros de Mora *C019 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 119-126
        JurosData,

        /// <summary>
        /// Valor da mora/dia ou Taxa mensal
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 127-141
        JurosMora,

        /// <summary>
        /// Código do Desconto 1
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 142
        DescontoCodigo1,

        /// <summary>
        /// Data do Desconto 1
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 143-150
        DescontoData1,

        /// <summary>
        /// Valor/Percentual a ser Concedido
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 151-165
        DescontoValor1,

        /// <summary>
        /// Valor do IOF a ser Recolhido 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 166-180
        ValorIOF,

        /// <summary>
        /// Valor do Abatimento
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 181-195
        ValorAbatimento,

        /// <summary>
        /// Identificação do Título na Empresa
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 196-220
        UsoEmpresaCedente,

        /// <summary>
        /// Código para Protesto 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 221-221
        ProtestoCodigo,

        /// <summary>
        /// Número de Dias para Protesto 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 222-223
        ProtestoPrazo,

        /// <summary>
        /// Código para Baixa/Devolução *C028 
        /// </summary>
        [RegFormat(RegType.P9, 1, Default="1")] // 224-224
        BaixaDevolucaoCodigo,

        [RegFormat(RegType.P9, 1)] // 225-225
        CNAB5,

        /// <summary>
        /// Número de Dias para Baixa/Devolução 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 226-227
        BaixaDevolucaoPrazo,

        /// <summary>
        /// Código da Moeda
        /// Código adotado pela FEBRABAN para identificar a moeda referenciada no Título. 
        /// Informar fixo: ‘09’ = REAL
        /// </summary>
        [RegFormat(RegType.P9, 2, Default="9")] // 228-229
        Moeda,

        [RegFormat(RegType.PX, 11)] // 230-240
        CNAB6
    }

    [RegLayout(@"^03300013\d{5}Q", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoQSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 4-7
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 8-8
        Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Nº Sequencial do Registro no Lote
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 9-13
        Nregistro,

        /// <summary>
        /// Cód. Segmento do Registro Detalhe
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "Q")] // 14-14
        Segmento,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 15-15
        CNAB1,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "1")] // 16-17
        CodMov,

        #endregion

        /// <summary>
        /// Tipo de inscrição
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 18-18
        Sacado_Tipo,

        /// <summary>
        /// CPF/CNPJ
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 19-33
        Sacado_Numero,

        /// <summary>
        /// Nome
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 34-73
        Nome,

        /// <summary>
        /// Endereço
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 74-113
        Endereco,

        /// <summary>
        /// Bairro
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 114-128
        Bairro,

        /// <summary>
        /// CEP
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 129-133
        CEP,

        /// <summary>
        /// Cidade
        /// </summary>
        [RegFormat(RegType.PX, 15)] // 137-151
        Cidade,

        /// <summary>
        /// Unidade Federal (estado)
        /// </summary>
        [RegFormat(RegType.PX, 2)] // 152-153
        UF,

        /// <summary>
        /// Tipo de Avalista
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 154-154
        Avalista_Tipo,

        /// <summary>-
        /// Numero CPF/CNPJ do avalista
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 155-169
        Avalista_Numero,

        /// <summary>
        /// Nome do sacador
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 170-209
        Avalista_Nome,

        /// <summary>
        /// Identificador de carne
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 210-212
        CarneIdentificador,

        [RegFormat(RegType.P9, 3)] // 213-215
        CarneParcela,

        [RegFormat(RegType.P9, 3)] // 216-218
        CarneTotalParcela,

        /// <summary>
        /// Número do plano
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 219-221
        NumeroPlano,

        [RegFormat(RegType.PX, 19)] // 222-240
        CNAB2
    }

    [RegLayout(@"^03300015")]
    public enum CNAB240TrailerLoteSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4, Default = "1")] // 4-47
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "5")] // 8-8
        Registro,

        [RegFormat(RegType.PX, 9)] // 9-17
        CNAB1,

        #endregion

        [RegFormat(RegType.P9, 6)] // 18-23
        QTD,

        [RegFormat(RegType.PX, 217)] // 24-240
        CNAB2,
    }

    [RegLayout(@"^03399999")]
    public enum CNAB240TrailerArquivoSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4, Default="9999")] // 4-7
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "9")] // 8-8
        Registro,

        #endregion

        [RegFormat(RegType.P9, 9)] // 9-17
        CNAB1,

        [RegFormat(RegType.P9, 6)] // 18-23
        LotesQTD,

        [RegFormat(RegType.P9, 6)] // 24-29
        RegistrosQTD,

        [RegFormat(RegType.PX, 211)] // 30-240
        CNAB2,

    }

    /// <summary>
    /// Registro Detalhe - Segmento T (Obrigatório - Retorno)
    /// </summary>
    [RegLayout(@"^033\d{4}[3]\d{5}T", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoTSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4, Default = "1")] // 4-7
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 8-8
        Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Nº Sequencial do Registro no Lote
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 9-13
        Nregistro,

        /// <summary>
        /// Cód. Segmento do Registro Detalhe
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "T")] // 14-14
        Segmento,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 15-15
        CNAB,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 16-17
        CodMov,

        #endregion

        /// <summary>
        /// 08.3T Código Identif. Agência Código Uso Exclusivo Santander 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 18-22
        AgenciaSantander,

        /// <summary>
        /// 09.3T DV Uso Exclusivo Santander 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 23-23
        AgenciaDVSantander,

        /// <summary>
        /// 10.3T- Código Cedente Código do Convênio no Banco - G007 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 24-29
        CodigoCedente,

        /// <summary>
        /// 11.3T Uso Exclusivo Uso Exclusivo da Santander 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 30-32
        Zero1,

        /// <summary>
        /// 11.3T Número do Banco Número do Banco de Sacados - C079 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 33-35
        NumeroBanco,

        /// <summary>
        /// 12.3T Uso Exclusivo Uso Exclusivo da Santander 
        /// </summary>
        [RegFormat(RegType.P9, 4)] // 36-39
        Brancos1,

        /// <summary>
        /// 13.3T Nosso Número Modalidade 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 40-41
        Modalidade,

        /// <summary>
        /// 13.3T Identificação do Título no Banco 
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 42-56
        NossoNumero,

        /// <summary>
        /// 13.3T Uso Exclusivo Uso Exclusivo Santander 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 57-57
        Zeros1,

        /// <summary>
        /// 14.3T Carteira Código da Carteira 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 58-58
        Carteira,

        /// <summary>
        /// 15.3T Número Documento (Seu Nº) Número do Documento de Cobrança 
        /// </summary>
        [RegFormat(RegType.PX, 11)] // 59-69
        NumeroDocumento,

        /// <summary>
        /// 15.3T Uso Exclusivo Uso Exclusivo Santander 
        /// </summary>
        [RegFormat(RegType.PX, 4)] // 70-73
        Brancos2,

        /// <summary>
        /// 16.3T Vencimento Data de Vencimento do Título  - *C012 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 74-81
        DataVencimento,

        /// <summary>
        /// 17.3T Valor do Título Valor Nominal do Título - G070 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 82-96
        ValorDocumento,

        /// <summary>
        /// 18.3T Banco Cobrador/Recebedor Código do Banco -C045 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 97-99
        BancoRecebedor,

        /// <summary>
        /// 19.3T Agência Cobradora/Recebedora Código da Agência Cobr/Receb - C086 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 100-104
        AgenciaCobradora,

        /// <summary>
        /// 20.3T DV Agência Cobr/Receb Dígito Verificador da Agência Cobr/Rec - G009 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 105-105
        AgenciaDVCobradora,

        /// <summary>
        /// 21.3T Uso da Empresa Identificação do Título na Empresa - C011 
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 106-130
        UsoEmpresaCedente,

        /// <summary>
        /// 22.3T Código da Moeda Código da Moeda - G065 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 131-132
        Moeda,

        /// <summary>
        /// 23.3T Sacado Tipo de Inscrição - G005 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 133-133
        Sacador_Tipo,

        /// <summary>
        /// 24.3T Número Número de Inscrição - G006 
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 134-148
        Sacador_Numero,

        /// <summary>
        /// 25.3T Nome 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 149-188
        Sacador_Nome,

        /// <summary>
        /// 26.3T CNAB Uso Exclusivo FEBRABAN/CNAB 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 189-198
        CNAB1,

        /// <summary>
        /// 27.3T Valor da Tar./Custas Valor da Tarifa / Custas - G076 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 199-213
        Tarifa,

        /// <summary>
        /// 28.3T Motivo da Ocorrência Identificação para Rejeições, Tarifas, Custas, Liquidação e Baixas - C047 
        /// </summary>
        [RegFormat(RegType.PX, 10)] // 214-223
        Ocorrencia,

        /// <summary>
        /// 29.3T CNAB Uso Exclusivo FEBRABAN/CNAB - G004 
        /// </summary>
        [RegFormat(RegType.PX, 17)] // 224-240
        CNAB2
    }

    /// <summary>
    /// Registro Detalhe - Segmento U (Obrigatório - Retorno)
    /// </summary>
    [RegLayout(@"^033\d{4}[3]\d{5}U", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoUSantander
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "033")] // 1-3
        Banco,

        /// <summary>
        /// Lote de Serviço
        /// </summary>
        [RegFormat(RegType.P9, 4, Default = "1")] // 4-7
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "3")] // 8-8
        Registro,

        #endregion

        #region "Serviço"

        /// <summary>
        /// Nº Sequencial do Registro no Lote
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 9-13
        Nregistro,

        /// <summary>
        /// Cód. Segmento do Registro Detalhe
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "U")] // 14-14
        Segmento,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 15-15
        CNAB,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 16-17
        CodMov,

        #endregion

        [RegFormat(RegType.PV, 15)] // 18-32
        ValorAcrescimos,

        [RegFormat(RegType.PV, 15)] // 33-47
        ValorDesconto,

        [RegFormat(RegType.PV, 15)] // 48-62
        ValorAbatimento,

        [RegFormat(RegType.PV, 15)] // 63-77
        ValorIOF,

        [RegFormat(RegType.PV, 15)] // 78-92
        ValorPago,

        [RegFormat(RegType.PV, 15)] // 933-107
        ValorLiquido,

        [RegFormat(RegType.PV, 15)] // 108-122
        ValorDespesas,

        [RegFormat(RegType.PV, 15)] // 123-137
        ValorCreditos,

        [RegFormat(RegType.PD, 8)] // 138-145
        DataOcorrencia,

        [RegFormat(RegType.PD, 8)] // 146-153
        DataCredito,

        [RegFormat(RegType.PX, 4)] // 154-157
        UsoSantander1,

        [RegFormat(RegType.PD, 8)] // 158-165
        DataTarifa,

        [RegFormat(RegType.PX, 15)] // 166-180
        CodigoSacado,

        [RegFormat(RegType.PX, 30)] // 181-210
        UsoSantander2,

        [RegFormat(RegType.P9, 3)] // 211-213
        BancoCorrespondente,

        [RegFormat(RegType.P9, 20)] // 214-233
        NossoNumero,

        [RegFormat(RegType.PX, 7)] // 234-240
        CNAB2
    }
}
