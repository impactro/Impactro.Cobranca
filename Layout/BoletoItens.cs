using System;
using System.Collections.Generic;
using System.Text;
using Impactro.Cobranca;
using System.Runtime.InteropServices;

namespace Impactro.Layout
{
    //public interface IBoletoItens
    //{
    //    int Count { get; }
    //    void Add(BoletoInfo boleto);
    //    void Remove(string cNossoNumero);
    //    BoletoInfo this[string cNossoNumero] { get; }
    //}

    /// <summary>
    /// Lista de Boletos
    /// </summary>
    [ComVisible(false)]
    //[ClassInterface(ClassInterfaceType.AutoDual)]
    //[Guid("46852782-A68C-4589-BEEC-B095B6807E59")]
    //[ProgId("BoletoItens")]
    public class BoletoItens // : IBoletoItens
    {
        SortedList<string, BoletoInfo> _boletos;
        /// <summary>
        /// Lista e gerencia um grupo de itens de Boletos
        /// </summary>
        public BoletoItens()
        {
            _boletos = new SortedList<string, BoletoInfo>();
            Duplicados = new List<BoletoInfo>();
        }

        /// <summary>
        /// Limpa a lista de boletos, erros e duplicados
        /// </summary>
        public void Clear()
        {
            _boletos.Clear();
            Duplicados.Clear();
            sbLinhasErro = null;
        }

        /// <summary>
        /// Retorna as linhas que deram erro
        /// </summary>
        public string ErroLinhas
        {
            get
            {
                if (sbLinhasErro == null)
                    return "";
                else
                    return sbLinhasErro.ToString();
            }
        }

        /// <summary>
        /// Adiciona informação que uma linha está com algum erro
        /// </summary>
        /// <param name="linha">linha com erro</param>
        public void AddErroLine(string linha = null)
        {
            if (sbLinhasErro == null)
                sbLinhasErro = new StringBuilder();

            sbLinhasErro.AppendLine(linha);
        }

        /// <summary>
        /// Adiciona um item completo já predefinido
        /// </summary>
        /// <param name="boleto">Boleto</param>
        /// <param name="linha">linha que deu origem ao boleto</param>
        public void Add(BoletoInfo boleto, string linha = null)
        {
            if (!string.IsNullOrEmpty(linha))
            {
                if (boleto.LinhaOrigem != "")
                    boleto.LinhaOrigem += "\r\n";

                boleto.LinhaOrigem += linha;
            }

            if (_boletos.ContainsKey(boleto.NossoNumero))
            {
                if (sbLinhasErro == null)
                    sbLinhasErro = new StringBuilder();

                sbLinhasErro.AppendLine(linha);

                if (AddErroType == BoletoDuplicado.Erro)
                    throw new Exception("Já existe um BoletoID com este numero: " + boleto.NossoNumero);
                else if (AddErroType == BoletoDuplicado.Ultimo)
                    _boletos[boleto.NossoNumero] = boleto;
                else if (AddErroType == BoletoDuplicado.Lista)
                    Duplicados.Add(boleto);

                // else ignore!
            }
            else
                _boletos.Add(boleto.NossoNumero, boleto);
        }

        /// <summary>
        /// Define a ação a ser tomada em caso de boleto duplicado
        /// </summary>
        public BoletoDuplicado AddErroType = BoletoDuplicado.Erro;

        private StringBuilder sbLinhasErro;

        /// <summary>
        /// Lista de boletos duplicados quando AddErroType = BoletoDuplicado.Lista
        /// </summary>
        public List<BoletoInfo> Duplicados;

        /// <summary>
        /// Retorna um Boleto definida pelo seu numero
        /// </summary>
        public BoletoInfo this[string cNossoNumero]
        {
            get
            {
                if (_boletos.ContainsKey(cNossoNumero))
                    return _boletos[cNossoNumero];
                else
                    throw new Exception("Este Boleto não existe");
            }
        }

        /// <summary>
        /// Remove um Boleto da lista
        /// </summary>
        /// <param name="cNossoNumero">Numero do Boleto</param>
        public void Remove(string cNossoNumero)
        {
            if (_boletos.ContainsKey(cNossoNumero))
                _boletos.Remove(cNossoNumero);
        }

        /// <summary>
        /// Conta o numero de Boletos atuais
        /// </summary>
        public int Count { get { return _boletos.Count; } }

        /// <summary>
        /// Retorna os numeros dos boletos
        /// </summary>
        public IList<string> NossoNumeros { get { return _boletos.Keys; } }
    }

    /// <summary>
    /// Ao adicionar boletos, se este já existir o que deve ser feito?
    /// </summary>
    public enum BoletoDuplicado
    {
        /// <summary>
        /// Gerar um erro: Exception
        /// </summary>
        Erro,

        /// <summary>
        /// Ignorar proximos, vale o primeiro incluido
        /// </summary>
        Ignore,

        /// <summary>
        /// Substitui sempre pelo ultimo
        /// </summary>
        Ultimo,

        /// <summary>
        /// Adiciona em uma lista a parte os duplicados
        /// </summary>
        Lista
    }
}