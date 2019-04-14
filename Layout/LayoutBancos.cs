using Impactro.Cobranca;
using System;
using System.Data;
using System.Runtime.InteropServices;

namespace Impactro.Layout
{
    /// <summary>
    /// Santander CNAB400
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("E15AB88D-E2D5-44B4-AB4D-08B0C7AD2B38")]
    [ProgId("LayoutBancos")]
    public class LayoutBancos
    {
        /// <summary>
        /// instancia do layout criado
        /// </summary>
        public CNAB cnab { get; private set; }

        #region Remapeamento CNAB para facilitar

        /// <summary>
        /// Define um evento para customizar os registros
        /// </summary>
        public LayoutBoletoReg onRegBoleto { set { cnab.onRegBoleto += value; } }

        /// <summary>
        /// Define um evento para criar registros opcionais
        /// </summary>
        public LayoutBoletoReg2 onRegOpcional { set { cnab.onRegOpcional += value; } }

        /// <summary>
        /// Define o tipo de erro em caso de duplicidade
        /// </summary>
        public BoletoDuplicado ErroType
        {
            get { return Boletos.AddErroType; }
            set { Boletos.AddErroType = value; }
        }

        /// <summary>
        /// obtem um item da lista
        /// </summary>
        /// <param name="n">posição absoluta</params>
        /// <returns>Objeto Reg[T]</returns>
        public object this[int n] { get { return cnab.itens[n]; } }

        /// <summary>
        /// Define a data/hora de geração o arquivo como referencia sempre em substituição do 'DateTime.Now'
        /// </summary>
        public DateTime DataHoje { get { return cnab.DataHoje; } set { cnab.DataHoje = value; } }

        /// <summary>
        /// Retorna as linhas onde houve qualquer erro de processamento
        /// </summary>
        public string ErroLinhas { get { return Boletos.ErroLinhas; } }

        /// <summary>
        /// Obtem um layout estruturado em tabela de acordo com o tipo informado
        /// </summary>
        public DataTable Table(Type tp)
        {
            return cnab.Table(tp);
        }

        /// <summary>
        /// Obtem os dados estruturados na posição N
        /// </summary>
        public DataTable Table(int n)
        {
            return cnab.Table(cnab.GetLayoutType(n));
        }

        /// <summary>
        /// Obtem um numero de lote, se for 0 (zero) no inicio da remessa será gerado automaticamente
        /// </summary>
        public int Lote { get { return cnab.NumeroLote; } set { cnab.NumeroLote = value; } }

        /// <summary>
        /// Sequencial do Lote
        /// </summary>
        public int SequencialLote { get { return cnab.SequencialLote; } set { cnab.SequencialLote = value; } }

        /// <summary>
        /// Coleção de boletos
        /// </summary>
        public BoletoItens Boletos { get { return cnab.Boletos; } }

        /// <summary>
        /// Exibe informações de dump das linhas
        /// </summary>
        public bool ShowDumpLine { get { return cnab.ShowDumpLine; } set { cnab.ShowDumpLine = value; } }

        /// <summary>
        /// Remove um boleto pelo nossonumero
        /// </summary>
        public void Remove(string cNossoNumero)
        {
            cnab.Remove(cNossoNumero);
        }

        /// <summary>
        /// Processa o conteudo arquivo de retorno contido na string de entrada
        /// </summary>
        public Layout Retorno(string cData)
        {
            return cnab.Retorno(cData);
        }

        /// <summary>
        /// Le o arquivo de retorno de um arquivo
        /// </summary>
        public Layout RetornoFrom(string cfile)
        {
            return cnab.RetornoFrom(cfile);
        }

        public int BoletoCount()
        {
            return Boletos.Count;
        }

        public BoletoInfo BoletoNumero(string cNossoNumero)
        {
            return Boletos[cNossoNumero];
        }

        public BoletoInfo BoletoItem(int nItem)
        {
            return Boletos[Boletos.NossoNumeros[nItem]];
        }

        #endregion

        /// <summary>
        /// Inicializa o gerador de layouts de acordo com o cedente, usando a instancia apropriada para cada banco
        /// </summary>
        public void Init(CedenteInfo cedente)
        {
            string[] cBanco = cedente.Banco.Split('-');
            Bancos banco = (Bancos)CobUtil.GetInt(cBanco[0]);

            if (banco == Bancos.SANTANDER || banco == Bancos.BANESPA_SANTANDER)
            {
                if (cedente.Layout == LayoutTipo.CNAB240)
                    cnab = new CNAB240Santander();
                else
                    cnab = new CNAB400Santander();
            }
            else if (banco == Bancos.BRADESCO)
                cnab = new CNAB400Bradesco();
            else if (banco == Bancos.ITAU)
                cnab = new CNAB400Itau();
            else if (banco == Bancos.BANCO_DO_BRASIL)
                cnab = new CNAB400BB();
            else if (banco == Bancos.SICREDI)
                cnab = new CNAB400Sicredi();
            else if (banco == Bancos.UniCred)
                cnab = new CNAB400UniCred();
            else if (banco == Bancos.BANESTES) // Em homologação
                cnab = new CNAB400Banestes();
            else if (banco == Bancos.BRB) // Em homologação
                cnab = new CNAB400BRB();
            else if (banco == Bancos.CAIXA_ECONOMICA_FEDERAL)
            {
                if (cedente.Layout == LayoutTipo.Auto || cedente.Layout == LayoutTipo.CNAB240)
                    cnab = new CNAB240Caixa();
            }
            else if (banco == Bancos.SICOOB)
                cnab = new CNAB240Sicoob();

            if (cnab == null)
                throw new Exception("Banco " + banco.ToString() + " não implementado para layout " + cedente.Layout);

            cnab.Cedente = CobUtil.Clone(cedente) as CedenteInfo;
        }

        /// <summary>
        /// Adiciona um boleto e seu respectivo sacado (será armazenado uma cópia (cole) destas instancias
        /// </summary>
        public void Add(BoletoInfo bol, SacadoInfo sac)
        {
            BoletoInfo boleto = bol.Clone(); // é feita uma cópia do boleto com os dados adicionais para não alterar o original
            boleto.SacadoInit((SacadoInfo)CobUtil.Clone(sac));
            cnab.Boletos.Add(boleto);
#if TEST_LOG
            cnab.Log += "\r\n +" + boleto.NossoNumero + " QTD: " + cnab.Boletos.Count + "\r\n";
            BoletoInfo b;
            foreach (string n in Boletos.NossoNumeros)
            {
                b = Boletos[n];
                cnab.Log += string.Format("'{0}' '{1}' / ", n, b.NossoNumero);
            }
            //cnab.Log += " - bi - ";
            //foreach (string n in bi.Keys)
            //{
            //    b = null;
            //    if (bi.TryGetValue(n, out b))
            //        cnab.Log += string.Format("'{0}' '{1}' / ", n, b.NossoNumero);
            //    else
            //        cnab.Log += n + "! ";
            //}
#endif
        }

        /// <summary>
        /// Gera o arquivo de remessa retornando o conteudo em uma string
        /// </summary>
        public string Remessa()
        {
            if (cnab.NumeroLote == 0)
                cnab.NumeroLote = (int)DataHoje.Subtract(new DateTime(2000, 1, 1)).TotalHours;

            //if (onRegBoleto != null)
            //    cnab.onRegBoleto += onRegBoleto;

            return cnab.Remessa();
        }

        /// <summary>
        /// Gera o arquivo de remessa no local especificado
        /// </summary>
        public void RemessaTo(string cFile)
        {
            if (cnab.NumeroLote == 0)
                cnab.NumeroLote = (int)DataHoje.Subtract(new DateTime(2000, 1, 1)).TotalHours;

            //if (onRegBoleto != null)
            //    cnab.onRegBoleto += onRegBoleto;

            cnab.RemessaTo(cFile);
        }

#if TEST_LOG
        public string Log { get { return cnab.Log; } }
#endif

    }
}