using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using Impactro.Cobranca;

// 18/03/2011
namespace Impactro.Layout {
    //class Util : Impactro.Cobranca.CobUtil { }
    /// <summary>
    /// Delegate para evento de geração e leitura de linhas
    /// </summary>
    /// <param name="lay">Objeto de controle de layout</param>
    /// <param name="reg">Item de Registro</param>
    public delegate void LayoutDelegate(Layout lay, IReg reg);

    /// <summary>
    /// Gera uma linha customizada.
    /// </summary>
    /// <returns>Retorna a nova linha.</returns>
    public delegate string LayoutLineReg(Layout lay, IReg reg);

    public delegate void LayoutLineInvalid(Layout lay, string cLine, int n);

    // otimiza o tratamento de tipos
    internal struct LayoutType
    {
        internal Type type;
        internal RegLayout layout;
        internal Type reg;
    }

    /// <summary>
    /// Gerador de layouts
    /// </summary>
    [Guid("F1BE8ED6-FBB4-4DB0-8649-876707CAA45D")]
    [ProgId("Layout")]
    public class Layout : IEnumerator, IEnumerable
    {
        // Lista dos tipos
        private List<LayoutType> type;

        private LayoutType LastRegType;

        /// <summary>
        /// Evento a ser invocado ante de gerar uma linha no arquivo
        /// </summary>
        public event LayoutDelegate onBeforeAppendLine;

        /// <summary>
        /// Evento a ser invocado apos ler uma linha do arquivo
        /// </summary>
        public event LayoutDelegate onAfterReadLine;

        /// <summary>
        /// Evento a ser invocado para gerar a linha quando necessário.
        /// É possível assim customizar a linha gerada.
        /// </summary>
        public event LayoutLineReg onAfterAppendLine;

        /// <summary>
        /// Evento a ser chamado quando houver erro em uma linha
        /// </summary>
        public event LayoutLineInvalid onInvalidLine;


        /// <summary>
        /// Para armazenar valores de variáveis de trabalho
        /// </summary>
        public SortedList<string, object> Data;

        /// <summary>
        /// Exibe as informações de DUMP de uma linha gerada no arquivo de saida
        /// </summary>
        public bool ShowDumpLine;

        /// <summary>
        /// Testa a integridade de conversão e desconversão dos dados
        /// </summary>
        public bool TesteTntegridade;

        /// <summary>
        /// Armazena um o objeto qualquer como sendo o criado 'proprietário'
        /// </summary>
        public object Ower;

        #region "Controle das Lista dos Itens"

        /// <summary>
        /// Lista dos itens
        /// </summary>
        public readonly List<IReg> itens;

        /// <summary>
        /// obtem um item da lista
        /// </summary>
        /// <param name="n">posição absoluta</param>
        /// <returns>Objeto Reg[T]</returns>
        public object this[int n] { get { return itens[n]; } }

#if NET4
        /// <summary>
        /// Efetua uma ação para um tipo de objeto ( Adicionado por Alexandre Savelli Bencz )
        /// </summary>
        public void ForEach(Type[] tp, Action<IReg, Type> selector)
        {
            IReg reg;
            for (int n = 0; n < itens.Count; n++)
            {
                if (itens[n].GetType().GetInterface("IReg") != null)
                {
                    reg = (IReg)itens[n];
                    if (tp == null || ContaisIReg(ref tp, reg.NameType))
                        selector(reg, reg.NameType);
                }
            }
        }
#endif
        /// <summary>
        /// Procura pelo tipo do iReg no array de typos ( Adicionado por Alexandre Savelli Bencz )
        /// </summary>
        /// <param name="tp">Array de tipos</param>
        /// <param name="t">Tipo para ser procurado no array</param>
        /// <returns>True caso exista e False caso nao encontre</returns>
        protected bool ContaisIReg(ref Type[] tp, Type t)
        {
            for (int i = 0; i < tp.Length; i++)
                if (tp[i] == t)
                    return true;
            return false;
        }

        /// <summary>
        /// Efetua uma ação para um tipo de objeto
        /// </summary>
        public void ForEach<T>(Action<Reg<T>> selector)
        {
            //Predicate: essentially Func<T, bool>; asks the question "does the specified argument satisfy the condition represented by the delegate?" Used in things like List.FindAll.
            //Action: Perform an action given the arguments. Very general purpose. Not used much in LINQ as it implies side-effects, basically.
            //Func: Used extensively in LINQ, usually to transform the argument, e.g. by projecting a complex structure to one property.
            IReg reg;
            Type tp = typeof(T);
            for (int n = 0; n < itens.Count; n++)
            {
                if (itens[n].GetType().GetInterface("IReg") != null)
                {
                    reg = (IReg)itens[n];
                    if (tp == null || reg.NameType == tp)
                        selector(reg as Reg<T>);
                }
            }
        }

        public void ForEachIReg(Action<IReg> selector)
        {
            //Predicate: essentially Func<T, bool>; asks the question "does the specified argument satisfy the condition represented by the delegate?" Used in things like List.FindAll.
            //Action: Perform an action given the arguments. Very general purpose. Not used much in LINQ as it implies side-effects, basically.
            //Func: Used extensively in LINQ, usually to transform the argument, e.g. by projecting a complex structure to one property.
            IReg reg;
            for (int n = 0; n < itens.Count; n++)
            {
                if (itens[n].GetType().GetInterface("IReg") != null)
                    selector(itens[n] as IReg);
            }
        }

        /// <summary>
        /// Achar o tipo de registro de acordo com o tipo de linha
        /// </summary>
        protected bool FindReg(ref IReg reg, string cLine)
        {
            if (reg == null || !LastRegType.layout.IsThis(cLine))
            {
                foreach (LayoutType lt in type)
                {
                    if (lt.layout.IsThis(cLine))
                    {
                        // Define o ultimo tipo, como o atual encontrado
                        LastRegType = lt;
                        reg = (IReg)Activator.CreateInstance(LastRegType.reg);
                        return true;
                    } 
                }
                // Não Achou
                return false;
            }
            reg = reg.Copy(); 
            // É igual ao ultimo tipo (LastRegType)
            return true;
        }

        /// <summary>
        /// Obtem o numero de itens na lista
        /// </summary>
        public int Count { get { return itens.Count; } }

        /// <summary>
        /// Adiciona um item
        /// </summary>
        /// <param name="oReg">Objeto Reg[T]</param>
        public virtual void Add(IReg oReg)
        {
            itens.Add(oReg);
        }

        //int nProcess;
        //public int Process { get { return nProcess; } }

        #region IEnumerator / IEnumerable

        // Ponteido de controle da posição do IEnumerator
        private int niPos;

        /// <summary>
        /// Obtem o valor atual
        /// </summary>
        public object Current
        {
            get { return this[niPos]; }
        }

        /// <summary>
        /// Aponta o proxima valor
        /// </summary>
        /// <returns>Retorna Verdadeiro sempre que houver um proximo valor</returns>
        public bool MoveNext()
        {
            return ++niPos < Count;
        }

        /// <summary>
        /// Reinicia para o elemento 'Zero'
        /// </summary>
        public void Reset()
        {
            niPos = -1;
        }

        /// <summary>
        /// Obtem a instancia de enumerado para controle de foreach()
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        #endregion

        /// <summary>
        /// Define um modelo de layout
        /// </summary>
        /// <param name="eTypes">Tipos de enumeradores de estruturra</param>
        public Layout(params Type[] eTypes)
        {

            Data = new SortedList<string, object>();
            type = new List<LayoutType>();
            itens = new List<IReg>();

            AddTypes(eTypes);

            if (type.Count > 0)
                LastRegType = type[0];
        }

        /// <summary>
        /// Adiciona outros tipos no array de reconhecimento
        /// </summary>
        protected void AddTypes(params Type[] eTypes)
        {
            RegLayout l;
            Type r;
            foreach (Type tp in eTypes)
            {
                if (tp == null)
                    continue;

                l = (RegLayout)Attribute.GetCustomAttribute(tp, typeof(RegLayout));
                r = typeof(Reg<>).MakeGenericType(tp); // Cria o tipo Generico 'T' para o Reg<T>
                type.Add(new LayoutType()
                {
                    type = tp,
                    layout = l,
                    reg = r
                });
            }
        }

        /// <summary>
        /// Remove todos os itens
        /// </summary>
        public void Clear()
        {
            itens.Clear();
        }

        /// <summary>
        /// Retorna o tipo de layout
        /// </summary>
        public Type GetLayoutType(int n)
        {
            return type[n].type;
        }

        /// <summary>
        /// Transforma os dados em uma tabela (DataTable)
        /// </summary>
        /// <param name="tp">Tipo de dados a ser obtido (filtrado)</param>
        /// <returns>DataBable co mos dados de um determinado tipo</returns>
        public DataTable Table(Type tp)
        {
            LayoutType lt = type.Find(t => t.type == tp);
            if (lt.type == null)
                throw new Exception("Tipo não existe no layout: " + tp.FullName);

            DataTable tb = new DataTable(tp.Name);
            RegFormat rf;
            foreach (FieldInfo fi in lt.type.GetFields())
            {
                if (!fi.IsSpecialName)
                {
                    rf = (RegFormat)Attribute.GetCustomAttribute(fi, typeof(RegFormat));
                    switch (rf._type)
                    {
                        case RegType.P9:
                            //Para evitar qualquer problema de conversão
                            //if (rf._length < 10)
                            //    tb.Columns.Add(fi.Name, typeof(int));
                            //else
                                tb.Columns.Add(fi.Name, typeof(string));
                            break;
                        case RegType.PD:
                            tb.Columns.Add(fi.Name, typeof(DateTime));
                            break;
                        default:
                            tb.Columns.Add(fi.Name, typeof(string));
                            break;
                    }
                }
            }
            IReg reg;
//            int n = 0;
            foreach (object obj in itens)
            {
                if (obj.GetType().GetInterface("IReg") != null)
                {
                    reg = (IReg)obj;
                    if (reg.NameType == tp)
                    {
                        tb.Rows.Add(reg.Values);
                        //if (++n % 100 == 0)
                        //    Thread.CurrentThread.MonitorStatus(100,"Layout {0}: {1}/{2}", reg.NameType, n, itens.Count);
                    }
                }
            }
            return tb;
        }

        public DataSet DataSet()
        {
            DataSet ds=new DataSet();
            foreach(var l in type)
            {
                DataTable tb = Table(l.type);
                if (tb != null)
                {
                    tb.TableName = l.type.Name;
                    ds.Tables.Add(tb);
                }
            }
            return ds;
        }

        public TimeSpan LastProcess;
        private DateTime dtStart;
        /// <summary>
        /// Conteudo (E/S) do arquivo
        /// </summary>
        public string Conteudo
        {
            get
            {
                dtStart = DateTime.Now;
                StringBuilder cOut = new StringBuilder();
                IReg reg;
                foreach (object obj in itens)
                {
                   // nProcess = 100 * n++ / _itens.Count;
                    if (obj.GetType().GetInterface("IReg") != null)
                    {
                        reg = (IReg)obj;
                        if (onBeforeAppendLine != null)
                            // Assim é possivel associar editar e calcular campos antes de gerar a linha
                            onBeforeAppendLine(this, reg);

                        string cNew;
                        if (onAfterAppendLine != null)
                            cNew = onAfterAppendLine(this, reg);
                        else
                            cNew = reg.Line;

                        cOut.AppendLine(cNew);

                        // Se for para exibir informações de Dump
                        if (ShowDumpLine)
                            cOut.AppendLine(reg.Dump);
                        // Se for para fazer teste de integridade (IDA e VOLTA) de compilação
                        if (TesteTntegridade)
                        {
                            // Para teste de integridade 
                            // (linha gerada deve ser igual a linha lida)
                            reg.Line = cNew;
                            string cNew2 = reg.Line;
                            if (cNew != cNew2)
                                throw new Exception(String.Format("Erro na conversão e desconversão: \r\n'{0}' \r\n'{1}' \r\n{2}", cNew, cNew2, reg.Dump));
                        }

                    }
                    else if (obj.GetType() == typeof(string))
                        cOut.AppendLine((string)obj);
                    else
                        throw new Exception("Tipo de objeto inválido");
                }
                LastProcess = DateTime.Now.Subtract(dtStart);
                return cOut.ToString();
            }
            set
            {
                itens.Clear();
                dtStart = DateTime.Now;
                string line = "?";
                int n = -1;
                IReg reg = null;
                int nErros = 0;
                try
                {
                    int nLine = 0;
                    string[] cLines = value.Split('\r', '\n');
                    for (n = 0; n < cLines.Length; n++)
                    {
//#if !DEVELOPER
//                        if (n % 500 == 499)
//                            Thread.CurrentThread.MonitorStatus(500, "Conteudo {0}", 100 * n / cLines.Length);
//#endif
                        // Ignora linha em branco
                        if ((line = cLines[n]) == "")
                            continue;
                        else if (line.Contains("\0")) //if (reg.Line.ToCharArray().Any(x => x == 0))
                            throw new Exception(string.Format("[L] Caracter 'NULO' encontrado na linha {0}", n));

                        nLine++;

                        // Itentifica o tipo de registro
                        //reg = null;
                        //foreach (LayoutType lt in type)
                        //{
                        //    if (lt.layout.IsThis(line))
                        //    {
                        //        reg = (IReg)Activator.CreateInstance(lt.reg);
                        //        break;
                        //    }
                        //}

                        // Le o registro
                        if (this.FindReg(ref reg, line))
                        {
                            try
                            {
                                reg.Line = line;

                                if (onAfterReadLine != null)
                                    // Assim é possivel manipular o cada objeto lido
                                    onAfterReadLine(this, reg);

                                itens.Add(reg);
                            }
                            catch (ThreadAbortException)
                            {
                            }
                            catch (Exception ex)
                            {
                                Exception e = ex;
                                string cErro = string.Format("Erro no registro tipo '{0}' linha {1}: '{2}':\r\n", reg.NameType.Name, nLine, line);
                                while (e != null)
                                {
                                    cErro += e.Message + "\r\n";
                                    e = e.InnerException;
                                }
                                if (nErros++ > 10)
                                    break;
                                if (ex.Data.Contains("Layout.Cancel"))
                                    n = cLines.Length;
                                else if (onInvalidLine != null)
                                    onInvalidLine(this, cErro, -1); //TODO: inserir a Exception!
                                else
                                    throw new Exception(cErro, ex);
                            }
                        }
                        else
                        {
                            if (onInvalidLine != null)
                                // Assim é possivel manipular o cada objeto lido
                                onInvalidLine(this, line, nLine);

                            //itens.Add(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Data.Add("line", line);
                    ex.Data.Add("n", n);
                    throw ex;
                }
                LastProcess = DateTime.Now.Subtract(dtStart);
            }
        }

        public bool Process(Stream stream)
        {
            int nErros = 0;
            Encoding enc = Encoding.GetEncoding(CobUtil.DefaultEncoding);
            string cLine;
            int n = 0;
            IReg reg = null;
            Dictionary<string, IReg> regCache = new Dictionary<string, IReg>();
            try
            {
                using (StreamReader sr = new StreamReader(stream, enc))
                {
                    while ((cLine = sr.ReadLine()) != null)
                    {
                        n++;
//#if !DEVELOPER
//                        if (n % 100 == 0)
//                            Thread.CurrentThread.MonitorStatus(100, "Conteudo {0}", (int)(100 * sr.BaseStream.Position / sr.BaseStream.Length));
//#endif
                        //continue;

                        if (!this.FindReg(ref reg, cLine))
                            continue;

                        try
                        {
                            reg.Line = cLine;

                            if (onAfterReadLine != null)
                                // Assim é possivel manipular o cada objeto lido
                                onAfterReadLine(this, reg);

                            itens.Add(reg);
                        }
                        catch (ThreadAbortException)
                        {
                        }
                        catch (Exception ex)
                        {
                            Exception e = ex;
                            string cErro = string.Format("Erro no registro tipo '{0}' linha {1}: '{2}':\r\n", reg.NameType.Name, n, cLine);
                            while (e != null)
                            {
                                cErro += e.Message + "\r\n";
                                e = e.InnerException;
                            }
                            if (nErros++ > 10)
                                break;
                            //if (ex.Data.Contains("Layout.Cancel"))
                            //    n = cLines.Length;
                            if (onInvalidLine != null)
                                onInvalidLine(this, cErro, -1); //TODO: inserir a Exception!
                            else
                                throw new Exception(cErro, ex);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
