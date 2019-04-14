using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

// 18/03/2011
#if IMPACTRO
using Impactro.Cobranca;
namespace Impactro.Layout
{
#else
namespace WC
{
#endif

    /// <summary>
    /// Define e gera linhas de registro de acordo com um enumerador de layout
    /// </summary>
    /// <typeparam name="T">Enumerados de layouts (Veja: RegFormat e RegType)</typeparam>
    public class Reg<T> : IReg //<T> IDisposable
    {

        /// <summary>
        /// Especifica a forma de formatação para datas de 6 digitos
        /// </summary>
        public string DateFormat6 = "yyMMdd";

        /// <summary>
        /// Especifica a forma de formatação para datas de 8 digitos
        /// </summary>
        public string DateFormat8 = "yyyyMMdd";

        /// <summary>
        /// Especifica a forma de formatação para datas com horas em 10 digitos
        /// </summary>
        public string DateFormat10 = "ddMMyyHHmm";

        /// <summary>
        /// Especifica a forma de formatação para datas com horas em 12 digitos
        /// </summary>
        public string DateFormat12 = "ddMMyyyyHHmm";

        /// <summary>
        /// Especifica a forma de formatação para datas com horas em 14 digitos
        /// </summary>
        public string DateFormat14 = "ddMMyyyyHHmmss";

        /// <summary>
        /// Se verdadeiro, permite acentos
        /// </summary>
        public bool Acentos;

        /// <summary>
        /// Se verdadeiro transforma a linha para caixa alta
        /// </summary>
        public bool Upper;

        private SortedList<string, RegFormat> valuesRF;
        private SortedList<string, object> valuesItem;

        // TODO: O que é isso ???
        public object extra;

        private Type tp;
        private string[] fields;

        string _readline;
        public string OriginalLine { get { return _readline; } }

        /// <summary>
        /// Construtora
        /// </summary>
        public Reg()
        {
            tp = typeof(T);
            RegLayout rl = (RegLayout)Attribute.GetCustomAttribute(tp, typeof(RegLayout));
            if (rl != null)
            {
                DateFormat6 = rl.DateFormat6 ?? DateFormat6;
                DateFormat8 = rl.DateFormat8 ?? DateFormat8;
                DateFormat10 = rl.DateFormat10 ?? DateFormat10;
                DateFormat12 = rl.DateFormat12 ?? DateFormat12;
                DateFormat14 = rl.DateFormat14 ?? DateFormat14;
                Acentos = rl.Acentos;
                Upper = rl.Upper;
            }
            fields = rft.GetFields(tp);
            valuesRF = new SortedList<string, RegFormat>();
            valuesItem = new SortedList<string, object>();
            RegFormat rf;
            foreach (string cField in fields)
            {
                rf = RegFormat.Get(tp.GetField(cField));
                valuesRF.Add(cField, rf);
                valuesItem.Add(cField, rf.GetDefault());
            }
        }

        internal Reg(Reg<T> orig)
        {
            tp = orig.tp;
            DateFormat6 = orig.DateFormat6;
            DateFormat8 = orig.DateFormat8;
            DateFormat10 = orig.DateFormat10;
            DateFormat12 = orig.DateFormat12;
            DateFormat14 = orig.DateFormat14;
            Acentos = orig.Acentos;
            Upper = orig.Upper;
            fields = orig.fields;
            valuesRF = orig.valuesRF;
            valuesItem = new SortedList<string, object>();
        }

        public IReg Copy()
        {
            return new Reg<T>(this);
        }

        /// <summary>
        /// Obtem o tipo 'T' do Regitro
        /// </summary>
        public Type NameType { get { return tp; } }

        public object[] Values
        {
            get
            {
                List<object> o = new List<object>();
                foreach (string cField in fields)
                {
                    if (valuesItem.ContainsKey(cField))
                        o.Add(valuesItem[cField]);
                    else
                        o.Add(null);
                }
                return o.ToArray();
            }
        }

        /// <summary>
        /// Define um valor de campo, se este for permitido
        /// </summary>
        public void Set(string key, object value)
        {
            int n = key.LastIndexOf(".");
            if (n > 0)
            {
                string eName = key.Substring(0, n);
                if (NameType.Name == eName)
                    key = key.Substring(n + 1);
                else
                    return;
            }
            if (valuesRF.ContainsKey(key))
                valuesItem[key] = value;
        }

        /// <summary>
        /// Obtem um valor baseado no nome, ou nulo se não existir
        /// </summary>
        public object Get(string key)
        {
            return valuesItem[key];
        }

        /// <summary>
        /// Retorna ou atribui um valor de registro
        /// </summary>
        /// <param name="o">Deve ser um elemento de um enumerador de campos, onde cada campo deve conter um atributo 'RegFormat'</param>
        /// <returns>Valor (Numerico, Data, String)</returns>
        public object this[T o]
        {
            set
            {
                Type tp = o.GetType();
                string cFiled = o.ToString();
                //RegFormat rf = RegFormat.Get(tp.GetField(cFiled));
                try
                {
                    valuesItem[o.ToString()] = value;

                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao definir o valor do campos: " + cFiled, ex);
                }
            }
            get
            {
                return valuesItem[o.ToString()];
            }
        }

        /// <summary>
        /// Gera uma linha de texto baseado no enumerador de layout, ou preenche o enumerador de layout baseado em uma linha lida
        /// </summary>
        public string Line
        {
            get
            {
                // Define-se os valores do enumerados, e gera-se uma linha
                StringBuilder cLine = new StringBuilder();
                string cFormat;
                RegFormat rf = null; // = new RegFormat(RegType.PX, 0);
                object oValue;
                string cValor;
                string fieldName = "?";
                string cLineValue;
                try
                {
                    foreach (string field in fields)
                    {
                        FieldInfo fi = tp.GetField(fieldName = field);

                        rf = valuesRF[field];
                        oValue = valuesItem[field];
                        if (oValue == null && rf.ExisteSe > 0)
                            continue;

                        cLineValue = null;

                        switch (rf._type)
                        {
                            case RegType.P9:
                                if (oValue == null)
                                    cLineValue = new string('0', rf._length);
                                else if (oValue.GetType() == typeof(string))
                                {
                                    cValor = (string)oValue;

                                    if (cValor.Length > rf._length)
                                        throw new Exception("Tamanho " + cValor.Length + " ultrapassa o espaço necessario para o número: '" + cValor + "' P9(" + rf._length + ")");

                                    cLineValue = cValor.PadLeft(rf._length, '0');
                                }
                                else
                                {
                                    cFormat = new string('0', rf._length);
                                    cLineValue = String.Format("{0:" + cFormat + "}", oValue);
                                }
                                break;

                            case RegType.PV:
                                cValor = String.Format("{0:N" + rf.nDecimal + "}", oValue);
                                cValor = cValor.Replace(".", "").Replace(",", "");
                                if (cValor.Length > rf._length)
                                    throw new Exception("Tamanho " + cValor.Length + " ultrapassa o espaço necessario para o número: '" + cValor + "' PV(" + rf._length + ")");
                                else if (cValor.Length < rf._length)
                                    cValor = new string('0', rf._length - cValor.Length) + cValor;
                                cLineValue = cValor;
                                break;

                            case RegType.PX:
                                if (rf._length == 0)
                                    cLineValue = (string)oValue ?? "";
                                else
                                {

                                    if (oValue == null)
                                        cValor = "";
                                    else if (oValue.GetType() == typeof(string))
                                        cValor = (string)oValue;
                                    else
                                        cValor = oValue.ToString();

                                    if (cValor == null)
                                        cLineValue = new string(' ', rf._length);
                                    else if (cValor.Length >= rf._length)
                                        cLineValue = cValor.Substring(0, rf._length);
                                    else
                                        cLineValue = cValor.PadRight(rf._length, ' ');
                                }
                                break;

                            case RegType.PD:
                                if (oValue == null || (DateTime)oValue == DateTime.MinValue)
                                    cLineValue = new string('0', rf._length);
                                else if (rf._length == 14)
                                    cLineValue = string.Format("{0:" + DateFormat14 + "}", oValue);
                                else if (rf._length == 12)
                                    cLineValue = string.Format("{0:" + DateFormat12 + "}", oValue);
                                else if (rf._length == 8)
                                    cLineValue = string.Format("{0:" + DateFormat8 + "}", oValue);
                                else if (rf._length == 6)
                                    cLineValue = string.Format("{0:" + DateFormat6 + "}", oValue);
                                else
                                    throw new Exception("Tamanho de data inválido");
                                break;

                            case RegType.PH:
                                if (oValue == null)
                                    cLineValue = new string('0', rf._length);
                                else if (rf._length == 6)
                                    cLineValue = string.Format("{0:HHmmss}", oValue);
                                else if (rf._length == 4)
                                    cLineValue = string.Format("{0:HHmm}", oValue);
                                else
                                    throw new Exception("Tamanho de hora inválido");
                                break;
                        }

                        if (cLineValue.Length > rf._length && rf._length > 0)
                            throw new Exception("Valor ultrapassou o tamanho máximo permitido");

                        if (cLineValue.Length < rf._length)
                            throw new Exception("Valor não atingiu o tamanho necessário");

                        cLine.Append(cLineValue);
                    }

                    string lineOut = cLine.ToString();

                    string cOut = cLine.ToString();

                    if (!Acentos)
                        cOut = CobUtil.RemoveAcentos(cOut);

                    if (Upper)
                        cOut = cOut.ToUpper();

                    return cOut;
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao gerar linha por causa do campo: " + fieldName + " (" + rf._type.ToString() + ") " + rf._length.ToString(), ex);
                }
            }
            set
            {
                // Lido um linha, esta é validada e os valores lidos para a lista do enumerados
                // Define-se os valores do enumerados, e gera-se uma linha
                string cLine = _readline = value;
                //                string cField = "?";
                RegFormat rf = null; // = new RegFormat(RegType.PX, 0);
                int nPos = 0;

#if TEST_LOG
                string cLog = "[";
#endif
                // _values.Clear();
                string cField = "";
                string cLineValue = "";
                try
                {

                    for (int nf = 0; nf < fields.Length; nf++)
                    {
                        cField = fields[nf];
#if TEST_LOG
                        cLog += " " + cField + "=";
#endif
                        rf = valuesRF[cField];

                        if (cLine.Length < nPos + rf._length)
                        {
                            if (rf.ExisteSe > 0)
                            {
                                valuesItem[cField] = null;
                                continue;
                            }
                            else
                            {
                                if (nf < fields.Length - 1 && valuesRF[fields[nf + 1]].ExisteSe == 0)
                                    throw new Exception("[RT] Total de caracteres na linha é insuficiente");

                                cLineValue = cLine.Substring(nPos);
                            }
                        }
                        else if (rf.ExisteSe > cLine.Length)
                        {
                            // Campo opcional no meio não atende o critério de existencia
                            valuesItem[cField] = null;
                            continue;
                        }
                        else
                            cLineValue = cLine.Substring(nPos, rf._length);

                        nPos += rf._length; // já posiciona a proxima leitura
#if TEST_LOG
                        cLog += cLineValue + "(" + nPos + ")";
#endif
                        switch (rf._type)
                        {
                            case RegType.P9:
                                if (cLineValue.Trim().Length == 0)
                                    valuesItem[cField] = 0;
                                else if (cLineValue.Length < 10)
                                    valuesItem[cField] = int.Parse(cLineValue);
                                else
                                    valuesItem[cField] = cLineValue;
                                break;

                            case RegType.PV:
                                if (cLineValue.Trim().Length == 0)
                                    valuesItem[cField] = 0;
                                else if (rf.nDecimal == 0)
                                    valuesItem[cField] = double.Parse(cLineValue);
                                else
                                    valuesItem[cField] = long.Parse(cLineValue) / Math.Pow(10, rf.nDecimal);
                                break;

                            case RegType.PX:
                                valuesItem[cField] = cLineValue;
                                break;

                            case RegType.PD:
                                if (cLineValue.Trim().Length == 0 || Int64.Parse(cLineValue) == 0) // data em branco
                                    valuesItem[cField] = DateTime.MinValue;
                                else if (rf._length == 6)
                                    switch (DateFormat6)
                                    {
                                        case "yyMMdd": // ano base 2000 (este seculo!)
                                            valuesItem[cField] = new DateTime(
                                                2000 + int.Parse(cLineValue.Substring(0, 2)), // ano
                                                int.Parse(cLineValue.Substring(2, 2)), // mes
                                                int.Parse(cLineValue.Substring(4, 2)));// dia
                                            break;
                                        case "ddMMyy": // ano base 2000 (este seculo!)
                                            valuesItem[cField] = new DateTime(
                                                2000 + int.Parse(cLineValue.Substring(4, 2)), // ano
                                                int.Parse(cLineValue.Substring(2, 2)), // mes
                                                int.Parse(cLineValue.Substring(0, 2)));// dia
                                            break;
                                        default:
                                            throw new NotImplementedException("[RT] Formato de conversão reversa não implementado: " + DateFormat6);
                                    }
                                else if (rf._length == 8)
                                    switch (DateFormat8)
                                    {
                                        case "yyyyMMdd":
                                            valuesItem[cField] = new DateTime(
                                                int.Parse(cLineValue.Substring(0, 4)), // ano
                                                int.Parse(cLineValue.Substring(4, 2)), // mes
                                                int.Parse(cLineValue.Substring(6, 2)));// dia
                                            break;
                                        case "ddMMyyyy":
                                            valuesItem[cField] = new DateTime(
                                                int.Parse(cLineValue.Substring(4, 4)), // ano
                                                int.Parse(cLineValue.Substring(2, 2)), // mes
                                                int.Parse(cLineValue.Substring(0, 2)));// dia
                                            break;
                                        default:
                                            throw new NotImplementedException("[RT] Formato de conversão reversa não implementado: " + DateFormat8);
                                    }
                                else if (rf._length == 12)
                                    switch (DateFormat12)
                                    {
                                        case "yyyyMMddHHmm":
                                            valuesItem[cField] = new DateTime(
                                                int.Parse(cLineValue.Substring(0, 4)), // ano
                                                int.Parse(cLineValue.Substring(4, 2)), // mes
                                                int.Parse(cLineValue.Substring(6, 2)), // dia
                                                int.Parse(cLineValue.Substring(8, 2)), // hora
                                                int.Parse(cLineValue.Substring(10, 2)), // minuto
                                                0);// segundo
                                            break;
                                        case "ddMMyyyyHHmm":
                                            valuesItem[cField] = new DateTime(
                                                int.Parse(cLineValue.Substring(4, 4)), // ano
                                                int.Parse(cLineValue.Substring(2, 2)), // mes
                                                int.Parse(cLineValue.Substring(0, 2)), // dia
                                                int.Parse(cLineValue.Substring(8, 2)), // hora
                                                int.Parse(cLineValue.Substring(10, 2)), // minuto
                                                0);// segundo
                                            break;
                                        default:
                                            throw new NotImplementedException("[RT] Formato de conversão reversa não implementado: " + DateFormat8);
                                    }
                                else if (rf._length == 14)
                                {
                                    switch (DateFormat14)
                                    {
                                        case "yyyyMMddHHmmss":
                                            valuesItem[cField] = new DateTime(
                                                Int32.Parse(cLineValue.Substring(0, 4)),   // ano
                                                Int32.Parse(cLineValue.Substring(4, 2)),   // mes
                                                Int32.Parse(cLineValue.Substring(6, 2)),   // dia
                                                CobUtil.GetInt(cLineValue.Substring(8, 2)),   // hora
                                                CobUtil.GetInt(cLineValue.Substring(10, 2)),  // minuto
                                                CobUtil.GetInt(cLineValue.Substring(12, 2))); // segundo
                                            break;
                                        case "ddMMyyyyHHmmss":
                                            valuesItem[cField] = new DateTime(
                                                Int32.Parse(cLineValue.Substring(4, 4)),   // ano
                                                Int32.Parse(cLineValue.Substring(2, 2)),   // mes
                                                Int32.Parse(cLineValue.Substring(0, 2)),   // dia
                                                CobUtil.GetInt(cLineValue.Substring(8, 2)),   // hora
                                                CobUtil.GetInt(cLineValue.Substring(10, 2)),  // minuto
                                                CobUtil.GetInt(cLineValue.Substring(12, 2))); // segundo
                                            break;
                                    }
                                }
                                else
                                    throw new NotImplementedException("[RT] Formato de conversão reversa não implementado para este numero de digitos:" + rf._length);
                                break;

                            case RegType.PH:
                                if (rf._length == 6)
                                    valuesItem[cField] = new DateTime(1901, 1, 1, // Data base 1/1/1901
                                                int.Parse(cLineValue.Substring(0, 2)), // hora
                                                int.Parse(cLineValue.Substring(2, 2)), // minuto
                                                int.Parse(cLineValue.Substring(4, 2)));// segundo
                                else if (rf._length == 4)
                                    valuesItem[cField] = new DateTime(1901, 1, 1, // Data base 1/1/1901
                                                int.Parse(cLineValue.Substring(0, 2)), // hora
                                                int.Parse(cLineValue.Substring(2, 2)), // minuto
                                                0);
                                else
                                    throw new NotImplementedException("[RT] Tamanho de hora inválido");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string cErro = " - Campo [" + cField + " (" + cLineValue + ") Pos: " + nPos +
#if TEST_LOG
                        cLog +
#endif
                        "]";
                    if (ex.Message.StartsWith("["))
                        throw new Exception(ex.Message + cErro, ex);
                    else
                        throw new Exception("[RT] Erro na linha" + cErro, ex);
                }
            }
        }

        /// <summary>
        /// Esta propriedade exibe as informações de geração com os nomes das variáveis, valores e tipos.
        /// (É preciso usar a propriedade Line primeiro)
        /// </summary>
        public string Dump
        {
            get
            {
                // Define-se os valores do enumerados, e gera-se uma linha
                //string cField = "?";
                RegFormat rf = new RegFormat(RegType.PX, 0);
                string cValor;
                string cDump = "";
                int nPos = 0;
                int i;
                foreach (string cField in fields)
                {
                    //if (!fi.IsSpecialName)
                    //{
                    //    cField = fi.Name;
                    // if (values.ContainsKey(cField))
                    rf = valuesRF[cField];
                    // else
                    //     rf = RegFormat.Get(tp.GetField(cField));

                    cValor = new string('_', nPos) +
                        ": " + cField + " " +
                        rf._type.ToString() + "(" + rf._length + ") " +
                        (nPos + 1).ToString() + "-" + (nPos + rf._length).ToString() +
                        ((rf.Default != null) ? " Default: " + rf.Default : "") +
                        ((rf.Dump != null) ? " * " + rf.Dump : "");

                    // Gera marcações de 10 em 10 e 20 em 20
                    for (i = 10; i < (nPos - 3); i += 10)
                    {
                        if (i % 20 == 0)
                            cValor = cValor.Substring(0, i - 1) + i + cValor.Substring(i + (i.ToString().Length - 1));
                        else if (i % 10 == 0)
                            cValor = cValor.Substring(0, i - 1) + "x" + cValor.Substring(i);
                    }
                    cDump += cValor + "\r\n";
                    nPos += rf._length;
                    // } // if

                } // for
                cDump = "DUMP-" + tp.Name + ": " + nPos + "\r\n" + cDump;
                return cDump;
            }
        }
    }
}