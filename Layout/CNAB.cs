using System;
using Impactro.Cobranca;
using System.IO;
using System.Runtime.InteropServices;

// Baseado no ITAU Cobrança 400 (maio/2010) e Bradesco 07 (26/08/09)
namespace Impactro.Layout
{
    /// <summary>
    /// A cada novo boleto processado esse evento será chamado ao final da definições padrão
    /// </summary>
    public delegate void LayoutBoletoReg(CNAB cnab, IReg reg, BoletoInfo boleto);

    /// <summary>
    /// Apos a geração dos registro basicos (obrigatórios) é possivel adicionar registros opcionais
    /// Deve retornar o numero de registros incluidos
    /// </summary>
    public delegate int LayoutBoletoReg2(CNAB cnab, BoletoInfo boleto);

    /// <summary>
    /// Tipo de de layout a ser gerado
    /// </summary>
    public enum LayoutTipo
    {
        /// <summary>
        /// Layout de 240 posições
        /// </summary>
        CNAB240,

        /// <summary>
        /// Layout de 400 posições
        /// </summary>
        CNAB400,

        /// <summary>
        /// Para os banco que só tem um unico modelo de layout, este será usado
        /// </summary>
        Auto
    }

    /// <summary>
    /// Classe base para os geradores de arquivos CNAB240 / CNAB400
    /// Cada banco tem seu algoritomo e personalizações de geração
    /// Essa classe é apenas um facilitador de comum reuso para base de cada banco
    /// </summary>
    [ComVisible(false)]
    public abstract class CNAB : Layout
    {
        /// <summary>
        /// DataHora de geração do arquivos
        /// </summary>
        public DateTime DataHoje = DateTime.Now;

#if TEST_LOG
        public string Log;
#endif
        /// <summary>
        /// Lê ou define o número do lote da remessa
        /// </summary>
        public int NumeroLote;

        /// <summary>
        /// Numero sequencial no lote (sub-lote) para CNAB240
        /// </summary>
        public int SequencialLote;

        /// <summary>
        /// Sequencial de geração do registro
        /// </summary>
        public int SequencialRegistro { get; protected set; }

        /// <summary>
        /// Dados do Cedente
        /// </summary>
        public CedenteInfo Cedente { get; set; }

        /// <summary>
        /// Retorna uma coleção de Boletos / Sacados para a geração do lote de remessa
        /// </summary>
        public BoletoItens Boletos { get; private set; }

        /// <summary>
        /// Evento a ser chamado antes de inserir boletos
        /// </summary>
        public event LayoutBoletoReg onRegBoleto;

        /// <summary>
        /// Evento a ser chamado para criação de registros opcionais
        /// </summary>
        public event LayoutBoletoReg2 onRegOpcional;

        /// <summary>
        /// Remove um Boleto da lista
        /// </summary>
        /// <param name="cNossoNumero">Numero do Boleto</param>
        public void Remove(string cNossoNumero)
        {
            Boletos.Remove(cNossoNumero);
        }

        /// <summary>
        /// Gera o arquivos de remessa
        /// </summary>
        /// <param name="eTypes">Array de tipos usados para a classe base Layout</param>
        public CNAB(params Type[] eTypes)
            : base(eTypes)
        {
            Boletos = new BoletoItens();
            // ShowDumpFooter = ShowDumpHeader = ShowDumpLine = false;
#if TEST_LOG
            Log="";
#endif
        }

        /// <summary>
        /// Renderiza a linha de um boleto, mas antes chama um evento proprio de customização
        /// </summary>
        protected void AddBoleto(IReg oReg, BoletoInfo boleto)
        {
            if (this.onRegBoleto != null)
                onRegBoleto(this, oReg, boleto);

            boleto.BindReg(oReg);
            Add(oReg);
        }

        /// <summary>
        /// Adiciona referencia dos tipos opcionais
        /// </summary>
        public void AddOpcionalType(params Type[] eTypes)
        {
            base.AddTypes(eTypes);
        }

        /// <summary>
        /// Chama o evento para adicions os registros opcionais
        /// </summary>
        protected void AddOpcionais(BoletoInfo boleto)
        {
            if (this.onRegOpcional != null)
                SequencialRegistro += onRegOpcional(this, boleto);
        }

        /// <summary>
        /// chama o evento de pré renderização antes de adicionar a linha
        /// </summary>
        public override void Add(IReg oReg)
        {
            if (this.onRegBoleto != null)
                onRegBoleto(this, oReg, null);

            base.Add(oReg);
        }

        /// <summary>
        /// Rotina a ser desenvolvida para gerar a remessa
        /// </summary>
        public abstract string Remessa();

        /// <summary>
        /// Retorno a ser processado
        /// </summary>
        /// <param name="cData">Dado a ser interpretado</param>
        public abstract Layout Retorno(string cData);

        /// <summary>
        /// Lê um arquivo de retorno já processando um arquivo específico
        /// </summary>
        /// <param name="cFile">Nome do arquivo a Lêr e Processar</param>
        public Layout RetornoFrom(string cFile)
        {
            FileInfo fi = new FileInfo(cFile);
            if (!fi.Exists)
                throw new IOException("Arquivo não existe");

            TextReader tr = fi.OpenText();
            string cData = tr.ReadToEnd(); // Lê Header
            tr.Close();
            return Retorno(cData);
        }

        /// <summary>
        /// Gera o arquivo de remessa em um arquivo
        /// </summary>
        /// <param name="cFile">Nome do Arquivo</param>
        public void RemessaTo(string cFile)
        {
            FileInfo fi = new FileInfo(cFile);
            if (fi.Exists)
                fi.Delete();

            TextWriter tw = fi.CreateText();
            tw.Write(Remessa());
            tw.Close();
        }

        /// <summary>
        /// Quando ocorrer qualquer erro ao ler linhas de retorno
        /// </summary>
        protected void Retorno_onInvalidLine(Layout lay, string cLine, int n)
        {
            if (n == -1) // somente exceptions
                Boletos.AddErroLine(cLine);
        }
    }
}