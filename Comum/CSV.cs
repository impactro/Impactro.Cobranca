using System;
using System.Data;
using System.Text;
using System.IO;
using System.Collections.Generic;

#if IMPACTRO
using Impactro.WebControls;
using System.Data.OleDb;
namespace Impactro.Cobranca
#else
using FrameWork.WebControls;
namespace FrameWork.Cobranca
#endif
{
    public class CSV
    {
        //public static string TxtDriver = "Driver={Driver da Microsoft para arquivos texto (*.txt; *.csv)};Dbq='";
        //public static string TxtDriver="Driver={Microsoft Text Driver (*.txt;*.csv)};Dbq='";
        //public static string TxtDriver = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='";
        public DataTable data;
        public Exception erro;

        /// <summary>
        /// Transforma linha em colunas
        /// </summary>
        /// <param name="linha">Linha a ser desmenbrada </param>
        /// <returns>Colunas da linha passada</returns>
        public static string[] SepararCampos(string linha, char separador)
        {
            //Primerio limpo a linha

            //Removo as aspas e caracteres de tabulação do meio do texto de uma celula para não separar as colunas erradas
            while (linha.Contains(('\"').ToString()))
            {
                //Pego a linha a partir da primeira ", sem pegar a "
                string original = linha.Substring(linha.IndexOf('\"') + 1);

                //Corto a palavra novamente na primira ", sem pegar a "
                original = original.Remove(original.IndexOf('\"'));

                //Removo o caracter separador da frase selecionada
                string novo = original.Replace(separador.ToString(), "{@|}");

                // Para gerantir proteção contra loop infinito
                int n = linha.Length;

                //Troco na linha a frase original com " no inicio e no fim para não entrar novamente no while, pela nova frase já limpa
                linha = linha.Replace("\"" + original + "\"", novo);

                // Tem que ter havido a troca!
                if (n == linha.Length)
                    throw new Exception("LOOP INFINITO! Detectado");
            }

            //Separo os campos
            string []a=linha.Split(new Char[] { separador });

            // retorna os separadores quando necessário
            for(int i=0; i<a.Length; i++)
                a[i] = a[i].Replace("{@|}", separador.ToString());

            return a;
        }

        //[Obsolete("Importe usando o arquivo fisico")]
        public bool Load(string txt, string[] header, System.Type[] types)
        {
            data = new DataTable();

            //linhas separadas por enter
            string[] linhas = txt.Replace('\r', '\n').Split(new Char[] { '\n' });
            if (linhas.Length == 0)
            {
                erro = new Exception("Sem dados de cabeçalho");
                return false;
            }

            //ok, vamos passar os dados
            //dados a cada duas linhas

            string[] headerLido1 = linhas[0].Split(new Char[] { ',' });
            string[] headerLido2 = linhas[0].Split(new Char[] { ';' });
            string[] headerLido3 = linhas[0].Split(new Char[] { '|' });

            char cSeparator;
            string[] headerLido;

            if (headerLido1.Length > headerLido2.Length && headerLido1.Length > headerLido3.Length)
            {
                cSeparator = ',';
                headerLido = headerLido1;
            }
            else if (headerLido2.Length > headerLido1.Length && headerLido2.Length > headerLido3.Length)
            {
                cSeparator = ';';
                headerLido = headerLido2;
            }
            else
            {
                cSeparator = '|';
                headerLido = headerLido3;
            }

            int linha = -1;
            int coluna = 0;
            //vamos bater o header
            if (header != null && types != null)
            {

                List<string> lst = new List<string>();
                foreach (string s in headerLido)
                {
                    if (s.Trim() != "")
                        lst.Add(s.Trim());
                }

                string[] headerRecebido = lst.ToArray();
                if (headerRecebido.Length != header.Length)
                {
                    erro = new Exception("Número de colunas inválido: esperados " + header.Length.ToString() + ", recebidos " + headerRecebido.Length.ToString());
                    return false;
                }
                else if (header.Length != types.Length)
                {
                    erro = new Exception("Número de campos e tipos incopatíveis - campos: " + header.Length.ToString() + ", tipos: " + types.Length.ToString());
                    return false;
                }
                for (int i = 0; i < headerRecebido.Length; i++)
                {
                    if (string.Compare(headerRecebido[i], header[i], true) != 0)
                    {
                        //nao podemos meter o que o cara mandou....
                        erro = new Exception("Coluna " + i.ToString() + " incorreta, esperado '" + header[i] + "' encontrado '" + headerRecebido[i] + "'");
                        return false;
                    }
                }

                //criamos a tabela
                for (int i = 0; i < header.Length; i++)
                    data.Columns.Add(header[i], types[i]);

            }
            else if (header == null)
            {
                linha++;
                header = headerLido;
                for (coluna = 0; coluna < header.Length; coluna++)
                    data.Columns.Add(header[coluna], typeof(string));
            }

            while (linha < linhas.Length - 1)
            {
                linha++;
                if (linhas[linha].Length < 1)
                    continue;

                //se der erro, avisamos
                try
                {
                    string[] linhaDados = SepararCampos( linhas[linha], cSeparator );
                    DataRow row = data.NewRow();
                    for (coluna = 0; coluna < linhaDados.Length; coluna++)
                    {
                        while (data.Columns.Count < linhaDados.Length)
                        {
                            data.Columns.Add("c" + data.Columns.Count, typeof(string));
                        }

                        linhaDados[coluna] = linhaDados[coluna].Trim();
                        bool bTratado = false;

                        if (types == null || types[coluna] == typeof(string))
                        {
                            bTratado = true;
                            string cValor = linhaDados[coluna];
                            if (cValor.Length > 2 && cValor.StartsWith("\"") && cValor.EndsWith("\""))
                                cValor = cValor.Substring(1, cValor.Length - 2);
                            cValor = cValor.Replace("\"\"", "\"");
                            if (header != null)
                                row[header[coluna]] = cValor.Trim();
                            else
                                row[coluna] = cValor.Trim();
                        }
                        else if (types[coluna] == typeof(System.Int32))
                        {
                            bTratado = true;
                            if (linhaDados[coluna] != "")
                                row[header[coluna]] = Convert.ToInt32(linhaDados[coluna]);
                            else
                                row[header[coluna]] = 0;
                        }
                        else if (types[coluna] == typeof(double))
                        {
                            bTratado = true;
                            if (linhaDados[coluna] != "")
                            {
                                linhaDados[coluna] = linhaDados[coluna].Replace(",", "");
                                linhaDados[coluna] = linhaDados[coluna].Replace(".", ",");
                                row[header[coluna]] = Convert.ToDouble(linhaDados[coluna]);
                            }
                            else
                                row[header[coluna]] = 0;
                        }
                        else if (types[coluna] == typeof(DateTime))
                        {
                            //por enquanto nao sabemos fazer
                            bTratado = true;
                            if (linhaDados[coluna] != "")
                                row[header[coluna]] = DateTime.Parse(linhaDados[coluna]);
                            else
                                row[header[coluna]] = DateTime.MaxValue;
                        }
                        if (!bTratado)
                        {
                            erro = new Exception("Tipo de dado desconhecido, " + types[coluna].ToString());
                            return false;
                        }
                    }
                    data.Rows.Add(row);
                }
                catch (Exception ex)
                {
                    if (header == null)
                        erro = new Exception("Linha " + linha.ToString() + " incorreta! dados inválidos", ex);
                    else
                        erro = new Exception("Linha " + linha.ToString() + " incorreta, coluna " + coluna.ToString() + " (" + header[coluna] + "), dados inválidos", ex);
                    return false;
                }
            }

            return true;
        }

        public bool Load(string cFileName)
        {
            StreamReader sr = new StreamReader(cFileName);
            string csv = sr.ReadToEnd();
            sr.Close();
            return Load(csv, null, null);
        }

        /// <summary>
        /// Gera um CSV por meio de uma tabela
        /// </summary>
        /// <param name="tb">Tabela com os dados</param>
        /// <param name="cSeparador">separador, (',' virgula por padrão)</param>
        /// <returns></returns>
        public static string TableCSV(DataTable tb, string cSeparador=",")
        {

            StringBuilder sb = new StringBuilder();

            int nCols = tb.Columns.Count;
            int n = tb.Columns.Count - 1;
            string cValor;
            for (n = 0; n < nCols; n++)
            {
                cValor = "";
                //if (tb.Columns[n].ColumnName != "DigitalREP_BIO" && tb.Columns[n].ColumnName != "DigitalREP_IDX")
                if (!tb.Columns[n].ColumnName.Contains("Digital"))
                    cValor = tb.Columns[n].ColumnName;
                else
                    cValor = ";"; // termina a linha com um ponto e vírgula para indicar que está sendo importada a digital

                if (cValor.Contains("\""))
                    cValor = "\"" + cValor.Replace("\"", "\"\"") + "\"";
                else if (cValor.Contains(cSeparador) || cValor.Contains("\n") || cValor.Contains("\r"))
                    cValor = "\"" + cValor + "\"";

                //if (tb.Columns[n].ColumnName != "DigitalREP_BIO" && tb.Columns[n].ColumnName != "DigitalREP_IDX")
                if (!tb.Columns[n].ColumnName.Contains("Digital"))
                    sb.Append(cValor + (n < nCols - 1 ? cSeparador : ""));
            }

            sb.AppendLine();

            foreach (DataRow row in tb.Rows)
            {
                for (n = 0; n < nCols; n++)
                {
                    if (row[n] == DBNull.Value)
                        cValor = "";
                    else
                    {
                        if (row[n].GetType() == typeof(DateTime))
                            cValor = ((DateTime)row[n]).ToString("dd/MM/yyyy HH:mm:ss").Replace(" 00:00:00", "");
                        else
                            cValor = row[n].ToString();

                        if (cValor.Contains("\""))
                            cValor = "\"" + cValor.Replace("\"", "\"\"") + "\"";
                        else if ((cValor.Contains(cSeparador) || cValor.Contains("\n") || cValor.Contains("\r")) &&
                            !tb.Columns[n].ColumnName.Contains("Digital"))
                            //tb.Columns[n].ColumnName != "DigitalREP_BIO" && tb.Columns[n].ColumnName != "DigitalREP_IDX")
                            cValor = "\"" + cValor + "\"";
                    }
                    sb.Append(cValor + (n < nCols - 1 ? cSeparador : ""));
                }
                if (tb.Columns[nCols - 1].ColumnName.Contains("Digital"))
                    sb.Append(cSeparador);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}