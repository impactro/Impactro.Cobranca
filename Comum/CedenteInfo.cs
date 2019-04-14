// Autor: Fábio Ferreira de Souza 
// email: fabio@impactro.com.br
// Sites: www.impactro.com.br / www.boletoasp.com.br

using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Impactro.Layout;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Classe responsável pelas informações do Cedente de um boleto
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("0BABF45A-C06E-4895-AFE5-64C430859B8E")]
    [ProgId("CedenteInfo")]
    public class CedenteInfo
    {
        /// <summary>
        /// Um código do cedente para remessa.
        /// </summary>
        public string CedenteCOD;

        /// <summary>
        /// Nome do Cedente, Pessoa Juridica ou Física, titular da conta
        /// </summary>
        public string Cedente;

        /// <summary>
        /// Código do Banco no format 999-9, informe sempre o Código do Banco e o digito
        /// </summary>
        public string Banco;

        /// <summary>
        /// Agencia do Banco responsável pela conta do cedente
        /// </summary>
        public string Agencia;

        /// <summary>
        /// Numero da conta do Cedente
        /// </summary>
        public string Conta;

        /// <summary>
        /// Código Carteira do convenio do Titulo de Cobrança
        /// (opcional para alguns bancos)
        /// </summary>
        public string Carteira;

        /// <summary>
        /// SubTipo da Carteira
        /// </summary>
        public string CarteiraTipo;

        /// <summary>
        /// Código da modalidade ou logica nescessária para a geração do boleto
        /// (opcional para alguns bancos)
        /// </summary>
        public string Modalidade;

        /// <summary>
        /// Código do convenio que representa a agencia/conta do cedente ou algum contrato junto ao banco
        /// (opcional para alguns bancos, veja também 'CodCedente')
        /// </summary>
        public string Convenio;

        /// <summary>
        /// Código do cendente junto ao banco que representa agencia/conta, ou algum contrato junto ao banco
        /// (opcional para alguns bancos, veja também 'Convenio')
        /// </summary>
        public string CodCedente;

        /// <summary>
        /// Código interno do uso do Banco para CIP
        /// </summary>
        public string UsoBanco;

        /// <summary>
        /// Codigo CIP
        /// </summary>
        public string CIP;

        /// <summary>
        /// Apenas para o banco Banespa, quando é necessário que seja emitido o boleto pelo layout do Santander
        /// </summary>
        public bool useSantander;

        /// <summary>
        /// Endereço do Cedente
        /// </summary>
        public string Endereco;

        /// <summary>
        /// Praça da Agencia (Bairro)
        /// </summary>
        public string Praca;

        /// <summary>
        /// CNPJ do Cedente (ou CPF)
        /// </summary>
        public string CNPJ;

        /// <summary>
        /// Informações que serão exibidas no final do boleto
        /// </summary>
        public string Informacoes;

        public LayoutTipo Layout = LayoutTipo.Auto;

        /// <summary>
        /// Cria uma instancia das informações do Cedente com todos os dados em branco
        /// </summary>
        public CedenteInfo()
        {
            CedenteCOD = "";
            Cedente = "";
            Banco = "";
            Agencia = "";
            Conta = "";
            Carteira = "";
            Convenio = "";
            CodCedente = "";
            Modalidade = "";
            UsoBanco = "";
            CIP = "";
            useSantander = false;
            //ExibirCedenteEndereco = false;
            //ExibirCedenteDocumento = false;
            Informacoes = "";
            Layout = LayoutTipo.Auto;
        }

        /// <summary>
        /// Retorna somente os digitos do numero do CNPJ
        /// </summary>
        public string DocumentoNumeros
        {
            get
            {
                return CobUtil.SoNumeros(CNPJ);
                //if (CNPJ == null)
                //    return null;
                //Regex re = new Regex(@"\d");
                //Match m = re.Match(CNPJ);
                //string cOut = "";
                //while (m.Success)
                //{
                //    cOut += m.Value;
                //    m = m.NextMatch();
                //}
                //return cOut;
            }
        }

        /// <summary>
        /// Retorna 1 para CPF ou 2 para CNPJ
        /// </summary>
        public int Tipo
        {
            get
            {
                if (CNPJ != null && DocumentoNumeros.Length == 14)
                    return 2; // CNPJ
                else
                    return 1; // CPF
            }
        }
    }

}
