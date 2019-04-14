using System;
using Impactro.Cobranca;

namespace Impactro.Layout
{
    /// <summary>
    /// Layout no padrão CNAB240
    /// </summary>
    public class CNAB240Caixa : CNAB240<
        CNAB240HeaderArquivoCaixa, CNAB240HeaderLoteCaixa,
        CNAB240SegmentoPCaixa, CNAB240SegmentoQCaixa,
        CNAB240TrailerLoteCaixa, CNAB240TrailerArquivoCaixa>
    {
        /// <summary>
        /// Layout Caixa
        /// </summary>
        public CNAB240Caixa()
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

            // Header do Arquivo
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.InscricaoTipo] = Cedente.Tipo;
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.InscricaoNumero] = Cedente.DocumentoNumeros;
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.Agencia] = cAgenciaDig[0];
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.AgenciaDAC] = cAgenciaDig[1];
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.CodigoCedente] = Cedente.CodCedente;
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.EmpresaNome] = Cedente.Cedente;
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.Data] = DataHoje;
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.NumeroLote] = NumeroLote;
            regHeaderArquivo[CNAB240HeaderArquivoCaixa.ReservadoEmpresa] = Producao ? "REMESSA-PRODUÇÃO" : "REMESSA-TESTE";
            Add(regHeaderArquivo);

            // Header do lote
            regHeaderLote[CNAB240HeaderLoteCaixa.Lote] = SequencialLote;
            regHeaderLote[CNAB240HeaderLoteCaixa.InscricaoTipo] = Cedente.Tipo;
            regHeaderLote[CNAB240HeaderLoteCaixa.CodigoConvenio] = Cedente.Convenio;
            regHeaderLote[CNAB240HeaderLoteCaixa.InscricaoNumero] = Cedente.DocumentoNumeros;
            regHeaderLote[CNAB240HeaderLoteCaixa.Agencia] = cAgenciaDig[0];
            regHeaderLote[CNAB240HeaderLoteCaixa.AgenciaDAC] = cAgenciaDig[1];
            regHeaderLote[CNAB240HeaderLoteCaixa.CodigoCedente] = Cedente.CodCedente;
            regHeaderLote[CNAB240HeaderLoteCaixa.EmpresaNome] = Cedente.Cedente;
            regHeaderLote[CNAB240HeaderLoteCaixa.NumeroRemessaRetorno] = NumeroLote;
            regHeaderLote[CNAB240HeaderLoteCaixa.Data] = DataHoje;
            Add(regHeaderLote);

            BoletoInfo boleto;
            SacadoInfo sacado;
            Reg<CNAB240SegmentoPCaixa> regP;
            Reg<CNAB240SegmentoQCaixa> regQ;

            SequencialRegistro = 1;
            double ValorTotal = 0;

            foreach (string n in this.Boletos.NossoNumeros)
            {
                boleto = Boletos[n];
                sacado = boleto.Sacado;

                // Define as informações do segmento P
                regP = new Reg<CNAB240SegmentoPCaixa>();

                regP[CNAB240SegmentoPCaixa.Lote] = SequencialLote;
                regP[CNAB240SegmentoPCaixa.Nregistro] = SequencialRegistro++;
                regP[CNAB240SegmentoPCaixa.Agencia] = cAgenciaDig[0];
                regP[CNAB240SegmentoPCaixa.AgenciaDAC] = cAgenciaDig[1];
                regP[CNAB240SegmentoPCaixa.CodigoCedente] = Cedente.CodCedente;
                regP[CNAB240SegmentoPCaixa.CarteiraModalidade] = Cedente.Modalidade;
                regP[CNAB240SegmentoPCaixa.NossoNumero] = boleto.NossoNumero;
                regP[CNAB240SegmentoPCaixa.NumeroDocumento] = boleto.NumeroDocumento;
                regP[CNAB240SegmentoPCaixa.Vencimento] = boleto.DataVencimento;
                regP[CNAB240SegmentoPCaixa.ValorDocumento] = boleto.ValorDocumento;
                if(boleto.ValorMora >= 0.01)
                {
                    regP[CNAB240SegmentoPCaixa.Juros] = 1;
                    regP[CNAB240SegmentoPCaixa.JurosData] = boleto.DataVencimento.AddDays(1);
                    regP[CNAB240SegmentoPCaixa.JurosMora] = boleto.ValorMora;
                }
                else
                    regP[CNAB240SegmentoPCaixa.Juros] = 3; // isento

                regP[CNAB240SegmentoPCaixa.Especie] = (int)boleto.Especie;
                regP[CNAB240SegmentoPCaixa.Aceite] = boleto.Aceite;
                regP[CNAB240SegmentoPCaixa.Emissao] = boleto.DataDocumento;
                regP[CNAB240SegmentoPCaixa.UsoEmpresaCedente] = boleto.NossoNumero;
                int nDiasBaixa;
                if(boleto.DiasProtesto > 1)
                {
                    regP[CNAB240SegmentoPCaixa.ProtestoCodigo] = 1;
                    regP[CNAB240SegmentoPCaixa.ProtestoPrazo] = boleto.DiasProtesto;
                    regP[CNAB240SegmentoPCaixa.BaixaDevolucaoCodigo] = 2; // Não Baixar / Não Devolver
                    // Baixar em no minimo 8 dias apos o protesto baseado na data de geração do arquivo
                    nDiasBaixa = 0; // boleto.DataVencimento.AddDays(boleto.DiasProtesto + 8).Subtract(DateTime.Now).Days;
                }
                else
                {
                    regP[CNAB240SegmentoPCaixa.ProtestoCodigo] = 3; // não protestar
                    regP[CNAB240SegmentoPCaixa.BaixaDevolucaoCodigo] = boleto.DiasBaixa > 0 ? 1 : 2; // 1 => Baixar / Devolver
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

                regP[CNAB240SegmentoPCaixa.BaixaDevolucaoPrazo] = nDiasBaixa;
                ValorTotal += boleto.ValorDocumento;

                // Define as informações do segmento Q
                regQ = new Reg<CNAB240SegmentoQCaixa>();
                regQ[CNAB240SegmentoQCaixa.Lote] = SequencialLote;
                regQ[CNAB240SegmentoQCaixa.Nregistro] = SequencialRegistro++;
                regQ[CNAB240SegmentoQCaixa.Sacado_Tipo] = boleto.Sacado.Tipo;
                regQ[CNAB240SegmentoQCaixa.Sacado_Numero] = CobUtil.GetLong(CobUtil.SoNumeros(boleto.Sacado.DocumentoNumeros));
                regQ[CNAB240SegmentoQCaixa.Nome] = boleto.Sacado.Sacado;
                regQ[CNAB240SegmentoQCaixa.Endereco] = boleto.Sacado.Endereco;
                regQ[CNAB240SegmentoQCaixa.Bairro] = boleto.Sacado.Bairro;
                regQ[CNAB240SegmentoQCaixa.CEP] = boleto.Sacado.CepNumeros;
                regQ[CNAB240SegmentoQCaixa.Cidade] = boleto.Sacado.Cidade;
                regQ[CNAB240SegmentoQCaixa.UF] = boleto.Sacado.UF;
                long avalista = CobUtil.GetLong(boleto.Sacado.AvalistaNumeros);
                if(avalista>0)
                {
                    regQ[CNAB240SegmentoQCaixa.Avalista_Tipo] = boleto.Sacado.AvalistaTipo;
                    regQ[CNAB240SegmentoQCaixa.Avalista_Numero] = avalista;
                    regQ[CNAB240SegmentoQCaixa.Avalista_Nome] = boleto.Sacado.Avalista;
                }

                // adiciona o boleto convertido em registro
                // AddSegmentoPQ(regP, regQ);
                AddBoleto(regP, boleto);
                AddBoleto(regQ, boleto);

                AddOpcionais(boleto);
            }

            regTrailerLote[CNAB240TrailerLoteCaixa.Lote] = SequencialLote;
            regTrailerLote[CNAB240TrailerLoteCaixa.QTD] = SequencialRegistro+1; // tem que incluir o header
            regTrailerLote[CNAB240TrailerLoteCaixa.CobrancaQTD] = this.Boletos.Count;
            regTrailerLote[CNAB240TrailerLoteCaixa.CobrancaValor] = ValorTotal;
            Add(regTrailerLote);

            regTrailerArquivo[CNAB240TrailerArquivoCaixa.LotesQTD] = 1;
            regTrailerArquivo[CNAB240TrailerArquivoCaixa.RegistrosQTD] = itens.Count + 1;
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
            Layout retorno = new Layout(typeof(CNAB240SegmentoTCaixa), typeof(CNAB240SegmentoUCaixa));
            retorno.onInvalidLine += Retorno_onInvalidLine;
            retorno.Conteudo = cData;
            /*
            retorno.ForEach<CNAB240SegmentoTCaixa>(reg =>
                Boletos.Add(new BoletoInfo()
                {
                    NossoNumero = reg[CNAB240SegmentoTCaixa.NossoNumero].ToString(),
                    ValorDocumento = (double)reg[CNAB240SegmentoTCaixa.ValorDocumento],
                    DataVencimento = (DateTime)reg[CNAB240SegmentoTCaixa.DataVencimento],
                    ValorPago = (double)reg[CNAB240SegmentoTCaixa.ValorDocumento]
                }, reg.OriginalLine)
            );
            */
            BoletoInfo blt = null;
            retorno.ForEachIReg(ireg =>
            {
                if (ireg.NameType == typeof(CNAB240SegmentoTCaixa))
                {
                    if (blt != null)
                        Boletos.Add(blt);

                    // Para registros Tipo T cria o boleto
                    var reg = ireg as Reg<CNAB240SegmentoTCaixa>;
                    blt = new BoletoInfo()
                    {
                        NossoNumero = reg[CNAB240SegmentoTCaixa.NossoNumero].ToString(),
                        ValorDocumento = (double)reg[CNAB240SegmentoTCaixa.ValorDocumento],
                        DataVencimento = (DateTime)reg[CNAB240SegmentoTCaixa.DataVencimento]
                        //                        ValorPago = (double)reg[CNAB240SegmentoTCaixa.ValorDocumento]
                    };
                    blt.LinhaOrigem = reg.OriginalLine;
                }
                else if (ireg.NameType == typeof(CNAB240SegmentoUCaixa))
                {
                    // registros de outros tipos busca na lista de boletos existentes uma instancia válida para atualizar os dados complementares
                    var reg = ireg as Reg<CNAB240SegmentoUCaixa>;
                    blt.ValorAcrescimo = (double)reg[CNAB240SegmentoUCaixa.ValorAcrescimos];
                    blt.ValorDesconto = (double)reg[CNAB240SegmentoUCaixa.ValorDesconto];
                    blt.ValorDesconto2 = (double)reg[CNAB240SegmentoUCaixa.ValorAbatimento];
                    blt.ValorIOF = (double)reg[CNAB240SegmentoUCaixa.ValorIOF];
                    blt.ValorPago = (double)reg[CNAB240SegmentoUCaixa.ValorPago];
                    blt.ValorLiquido = (double)reg[CNAB240SegmentoUCaixa.ValorLiquido];
                    blt.DataPagamento = blt.DataProcessamento = (DateTime)reg[CNAB240SegmentoUCaixa.DataOcorrencia];
                    blt.DataCredito = (DateTime)reg[CNAB240SegmentoUCaixa.DataCredito];
                    blt.DataTarifa = (DateTime)reg[CNAB240SegmentoUCaixa.DataTarifa];
                }
            });
            if (blt != null)
                Boletos.Add(blt);
            return retorno;
        }
    }

    [RegLayout(@"^10400000", DateFormat8 = "ddMMyyyy", DateFormat14 = "ddMMyyyyHHmmss")]
    public enum CNAB240HeaderArquivoCaixa
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
        [RegFormat(RegType.P9, 4, Default = "0")] // 4
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

    [RegLayout(@"^10400011R01", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240HeaderLoteCaixa
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
        [RegFormat(RegType.P9, 4, Default = "1")] // 4
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "1")] // 8
        Registro,

        #endregion

        /// <summary>
        /// Tipo de Operação G028 
        /// R = Arquivo Remessa
        /// T = Arquivo Retorno
        /// </summary>
        [RegFormat(RegType.PX, 1, Default = "R")] // 9-9
        ServicoOperacao,

        /// <summary>
        /// Tipo de Serviço G025 
        /// 1 = Cobrança Registrada
        /// 2 = Cobrança Sem Registro / Serviços
        /// 3 = Desconto de Títulos
        /// 4 = Caução de Títulos
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "01")] // 10-11
        ServicoTipo,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB G004 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 12-13
        CNAB1,

        /// <summary>
        /// Layout do Lote Nº da Versão do Layout do Lote G030 
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "030")] // 14-16
        Layout,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB G004 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 17-17
        CNAB2,

        /// <summary>
        /// Tipo Tipo de Inscrição da Empresa G005 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 18-18
        InscricaoTipo,

        /// <summary>
        /// Número Nº de Inscrição da Empresa G006 
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 19-33
        InscricaoNumero,

        /// <summary>
        /// Código do Cedente no Banco G007 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 34-39
        CodigoConvenio,

        /// <summary>
        /// Uso Exclusivo Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 14)] // 40-53
        UsoCaixa,

        /// <summary>
        /// Código Agência Mantenedora da Conta G008 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 54-58
        Agencia,

        /// <summary>
        /// Dígito Verificador da Conta G011 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 59-59
        AgenciaDAC,

        /// <summary>
        /// Código do Convênio no Banco G007 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 60-65
        CodigoCedente,

        /// <summary>
        /// Cód Mod Personalizado Código do Modelo Personalizado C078 
        /// </summary>
        [RegFormat(RegType.P9, 7)] // 66-72
        CodModPersonalizado,

        /// <summary>
        /// Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 73-73
        UsoCaixa2,

        /// <summary>
        /// Nome da Empresa G013 
        /// </summary>
        [RegFormat(RegType.PX, 30)] // 74-103
        EmpresaNome,

        /// <summary>
        /// Mensagem 1 C073 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 104-143
        Informacao1,

        /// <summary>
        /// Mensagem 2 C073 
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 144-183
        Informacao2,

        /// <summary>
        /// Nº Rem./Ret. Número Remessa/Retorno G079 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 184-191
        NumeroRemessaRetorno,

        /// <summary>
        /// Gravação Data de Gravação Remessa/Retorno G068 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 192-199
        Data,

        /// <summary>
        /// Data do Crédito Data do Crédito C003 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 200-207
        DataCredito,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB G004 
        /// </summary>
        [RegFormat(RegType.PX, 33)] // 208-240
        CNAB3
    }
    
    [RegLayout(@"^10400013\d{5}P", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoPCaixa
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "104")] // 1-3
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

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 15-15
        CNAB,

        /// <summary>
        /// Código de Movimento Remessa
        /// </summary>
        [RegFormat(RegType.P9, 2, Default = "1")] // 16-17
        CodMov,

        #endregion

        /// <summary>
        /// Código Agência Mantenedora da Conta *G008 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 18-22
        Agencia,

        /// <summary>
        /// Dígito Verificador da Agência *G009 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 23-23
        AgenciaDAC,

        /// <summary>
        /// Código do Convênio no Banco *G007 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 24-29
        CodigoCedente,

        /// <summary>
        /// Uso Exclusivo da CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 8)] // 30-37
        UsoExclusivo1,

        /// <summary>
        /// Uso Exclusivo da CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 38-40
        UsoExclusivo2,

        /// <summary>
        /// Modalidade da Carteira *G069 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 41-42
        CarteiraModalidade,

        /// <summary>
        /// Identificação do Título no Banco *G069 
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 43-57
        NossoNumero,

        /// <summary>
        /// Código da Carteira *C006 
        /// 1 = Cobrança Simples
        /// 3 = Cobrança Caucionada
        /// 4 = Cobrança Descontada
        /// </summary>
        [RegFormat(RegType.P9, 1, Default="1")] // 58
        Carteira,

        /// <summary>
        /// Título no Banco *C007 
        /// 1 = Com Cadastramento (Cobrança Registrada)
        /// 2 = Sem Cadastramento (Cobrança sem Registro)
        /// </summary>
        [RegFormat(RegType.P9, 1, Default="1")] // 59
        Forma,

        /// <summary>
        /// Tipo de Documento *C008 
        /// Código adotado pela FEBRABAN para identificar a existência material do documento no processo. Informar fixo ‘2’
        /// </summary>
        [RegFormat(RegType.PX, 1, Default="2")] // 60
        TipoDocumento,

        /// <summary>
        /// Identificação da Emissão do Bloqueto *C009 
        /// 1 = Banco Emite
        /// 2 = Cliente Emite ou para Bloqueto Pré-Impresso Registrado (entrega do bloqueto pelo Cedente)
        /// 4 = Banco Reemite
        /// 5 = Banco Não Reemite
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "2")] // 61
        TipoEmissao,

        /// <summary>
        /// Identificação da Entrega do Bloqueto *C010 
        /// 0 = Postagem pelo Cedente
        /// 1 = Sacado via Correios
        /// 2 = Cedente via Agência CAIXA
        /// 3 = Sacado via e-mail
        /// 4 = Sacado via SMS
        /// </summary>
        [RegFormat(RegType.PX, 1, Default="0")] // 62
        Distribuicao,

        /// <summary>
        /// (Seu Nº) Número do Documento de Cobrança *C011 
        /// </summary>
        [RegFormat(RegType.PX, 11)] // 63-73
        NumeroDocumento,

        /// <summary>
        /// Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.PX, 4)] // 74-77
        UsoExclusivo3,

        /// <summary>
        /// Data de Vencimento do Título *C012 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 78-85
        Vencimento,

        /// <summary>
        /// Valor Nominal do Título *G070 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 86-100
        ValorDocumento,

        /// <summary>
        /// Agência Encarregada da Cobrança *C014 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 101-105
        AgenciaCobradora,

        /// <summary>
        /// Verificador da Agência *C014 
        /// </summary>
        [RegFormat(RegType.PX, 1, Default ="0")] // 106
        AgenciaCobradoraDV,

        /// <summary>
        /// Espécie do Título *C015 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 107-108
        Especie,

        /// <summary>
        /// Identific. de Título Aceito/Não Aceito *C016 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 109
        Aceite,

        /// <summary>
        /// Emissão do Título Data da Emissão do Título *G071 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 110-117
        Emissao,

        /// <summary>
        /// Cód. Juros Mora Código do Juros de Mora *C018 
        /// Código adotado pela FEBRABAN para identificação do tipo de pagamento de juros de mora.
        /// 1 = Valor por Dia
        /// 2 = Taxa Mensal
        /// 3 = Isento
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 118
        Juros,

        /// <summary>
        /// Juros Mora Data do Juros de Mora *C019 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 119-126
        JurosData,

        /// <summary>
        /// Mora Juros de Mora por Dia/Taxa *C020 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 127-141
        JurosMora,

        /// <summary>
        /// Código do Desconto 1 *C021 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 142
        DescontoCodigo1,

        /// <summary>
        /// Data do Desconto 1 *C022 
        /// </summary>
        [RegFormat(RegType.PD, 8)] // 143-150
        DescontoData1,

        /// <summary>
        /// Valor/Percentual a ser Concedido *C023 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 151-165
        DescontoValor1,

        /// <summary>
        /// Valor do IOF a ser Recolhido *C024 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 166-180
        ValorIOF,

        /// <summary>
        /// Valor do Abatimento *G045 
        /// </summary>
        [RegFormat(RegType.PV, 15)] // 181-195
        ValorAbatimento,

        /// <summary>
        /// Identificação do Título na Empresa *G072 
        /// </summary>
        [RegFormat(RegType.PX, 25)] // 196-220
        UsoEmpresaCedente,

        /// <summary>
        /// Código para Protesto *C026 
        /// 1 = Protestar
        /// 3 = Não Protestar
        /// 9 = Cancelamento Protesto Automático
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 221
        ProtestoCodigo,

        /// <summary>
        /// Número de Dias para Protesto *C027 
        /// </summary>
        [RegFormat(RegType.P9, 2)] // 222-223
        ProtestoPrazo,

        /// <summary>
        /// Código para Baixa/Devolução *C028 
        ///  1 - Baixar
        ///  2 - Devolver
        /// </summary>
        [RegFormat(RegType.P9, 1, Default="1")] // 224
        BaixaDevolucaoCodigo,

        /// <summary>
        /// Número de Dias para Baixa/Devolução *C029 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 225-227
        BaixaDevolucaoPrazo,

        /// <summary>
        /// Código da Moeda *G065 
        /// Código adotado pela FEBRABAN para identificar a moeda referenciada no Título. 
        /// Informar fixo: ‘09’ = REAL
        /// </summary>
        [RegFormat(RegType.P9, 2, Default="9")] // 228-229
        Moeda,

        /// <summary>
        /// Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 10)] // 230-239
        UsoExclusivo4,

        /// <summary>
        /// Uso Exclusivo FEBRABAN/CNAB G004 
        /// </summary>
        [RegFormat(RegType.PX, 1)] // 240
        CNAB2

    }

    [RegLayout(@"^10400013\d{5}Q", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoQCaixa
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
        Sacado_Tipo,

        /// <summary>
        /// CPF/CNPJ
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 19
        Sacado_Numero,

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
        /// Tipo de Avalista
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 154
        Avalista_Tipo,

        /// <summary>
        /// Numero CPF/CNPJ do avalista
        /// </summary>
        [RegFormat(RegType.P9, 15)] // 155
        Avalista_Numero,

        /// <summary>
        /// Nome do sacador
        /// </summary>
        [RegFormat(RegType.PX, 40)] // 170
        Avalista_Nome,

        /// <summary>
        /// 20.3Q Banco Correspondente Cód. Bco. Corresp. na Compensação
        /// </summary>
        [RegFormat(RegType.PX, 3)] // 210
        Correspondente_Banco,

        [RegFormat(RegType.PX, 20)] // 213
        Correspondente_NossoNumero,

        [RegFormat(RegType.PX, 8)] // 233
        Brancos
    }

    [RegLayout(@"^10400015")]
    public enum CNAB240TrailerLoteCaixa
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
        [RegFormat(RegType.P9, 4, Default = "1")] // 4
        Lote,

        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "5")] // 8
        Registro,

        [RegFormat(RegType.PX, 9)] // 9-17
        CNAB1,

        #endregion

        [RegFormat(RegType.P9, 6)] // 18-23
        QTD,

        [RegFormat(RegType.P9, 6)] // 24-29
        CobrancaQTD,

        [RegFormat(RegType.PV, 17)] // 30-47
        CobrancaValor,

        [RegFormat(RegType.P9, 6)] 
        CalcionadaQTD,

        [RegFormat(RegType.PV, 17)]
        CalcionadaValor,

        [RegFormat(RegType.P9, 6)]
        DescontadaQTD,

        [RegFormat(RegType.PV, 17)]
        DescontadaValor,

        [RegFormat(RegType.PX, 31)] 
        CNAB2,

        [RegFormat(RegType.PX, 117)] 
        CNAB3,

    }

    [RegLayout(@"^10499999")]
    public enum CNAB240TrailerArquivoCaixa
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
        [RegFormat(RegType.P9, 4, Default="9999")] // 4
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

    /// <summary>
    /// Registro Detalhe - Segmento T (Obrigatório - Retorno)
    /// </summary>
    [RegLayout(@"^104\d{4}[3]\d{5}T", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoTCaixa
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "104")] // 1-3
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
        /// 08.3T Código Identif. Agência Código Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 5)] // 18-22
        AgenciaCaixa,

        /// <summary>
        /// 09.3T DV Uso Exclusivo CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 1)] // 23-23
        AgenciaDVCaixa,

        /// <summary>
        /// 10.3T- Código Cedente Código do Convênio no Banco - G007 
        /// </summary>
        [RegFormat(RegType.P9, 6)] // 24-29
        CodigoCedente,

        /// <summary>
        /// 11.3T Uso Exclusivo Uso Exclusivo da CAIXA 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 30-32
        Zero1,

        /// <summary>
        /// 11.3T Número do Banco Número do Banco de Sacados - C079 
        /// </summary>
        [RegFormat(RegType.P9, 3)] // 33-35
        NumeroBanco,

        /// <summary>
        /// 12.3T Uso Exclusivo Uso Exclusivo da CAIXA 
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
        /// 13.3T Uso Exclusivo Uso Exclusivo CAIXA 
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
        /// 15.3T Uso Exclusivo Uso Exclusivo CAIXA 
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
        [RegFormat(RegType.P9, 1, Default ="0")] // 105-105
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
    [RegLayout(@"^104\d{4}[3]\d{5}U", DateFormat8 = "ddMMyyyy")]
    public enum CNAB240SegmentoUCaixa
    {
        #region "Controle"

        /// <summary>
        /// Código do Banco na Compensação
        /// </summary>
        [RegFormat(RegType.P9, 3, Default = "104")] // 1-3
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
        UsoCaixa1,

        [RegFormat(RegType.PD, 8)] // 158-165
        DataTarifa,

        [RegFormat(RegType.PX, 15)] // 166-180
        CodigoSacado,

        [RegFormat(RegType.PX, 30)] // 181-210
        UsoCaixa2,

        [RegFormat(RegType.P9, 3)] // 211-213
        BancoCorrespondente,

        [RegFormat(RegType.P9, 20)] // 214-233
        NossoNumero,

        [RegFormat(RegType.PX, 7)] // 234-240
        CNAB2
    }
}
