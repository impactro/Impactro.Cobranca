using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

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
    /// Atributo de propriedades de formatação
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RegFormat : Attribute
    {
        internal int _length;
        internal RegType _type;
        //internal object Value;
        //internal string lineValue;

        /// <summary>
        /// Obtem a descrição do tipo de registro
        /// </summary>
        public string Type { get { return _type.ToString(); } }

        /// <summary>
        /// Obtem a tamanho do campo do registro
        /// </summary>
        public int Length { get { return _length; } }

        /// <summary>
        /// Numero de casas decimal para campos com valores [PV] (padrão 2)
        /// </summary>
        public int nDecimal { get; set; }

        /// <summary>
        /// Valor padrão apenas para campos texto e numericos
        /// </summary>
        public string Default;

        /// <summary>
        /// Informações adicional para dump
        /// </summary>
        public string Dump;

        /// <summary>
        /// Existe apenas se o tamanho total da linha for maior que N
        /// </summary>
        public int ExisteSe;

        ///// <summary>
        ///// retorna a representação texto do valor gerado ou lido
        ///// </summary>
        //public string Text { get { return lineValue; } }

        /// <summary>
        /// Cria um atributo de formatação na ordem sequencial do enumerador
        /// (cuidado com compilação incremental)
        /// </summary>
        /// <param name="nTamanho">Tamanho do campo (incluindo decimans se houver)</param>
        /// <param name="rTipo">Tipo de campo</param>
        public RegFormat(RegType rTipo, int nTamanho)
        {
            this._type = rTipo;
            this._length = nTamanho;
            nDecimal = 2;
        }

        public RegFormat(RegType rTipo, int nTamanho, int nDec)
        {
            this._type = rTipo;
            this._length = nTamanho;
            this.nDecimal = nDec;
        }

        /// <summary>
        /// Limpa o valor da variável
        /// </summary>
        //public void Clear()
        //{
        //    lineValue = null;
        //    if (Value == null)
        //    {
        //        if (Default != null && _type == RegType.PX)
        //            Value = Default;
        //        else if (Default != null && _type == RegType.P9)
        //            Value = int.Parse(Default);
        //        else if (_type == RegType.P9 || _type == RegType.PV)
        //            Value = 0;
        //        else if (_type == RegType.PX)
        //            Value = "";
        //    }
        // }

        internal static RegFormat Get(FieldInfo fi)
        {
            RegFormat rf;
            Type tp = fi.DeclaringType;
            rf = (RegFormat)Attribute.GetCustomAttribute(fi, typeof(RegFormat));
            if (rf == null)
            {
                string[] erf = fi.Name.Split('_');
                if (erf.Length != 3)
                    throw new Exception("Enumerador não contem auto definição no formato 'Nome_Tipo_Tamanho', e nem atributo de 'RegFormat'");
                RegType t = (RegType)Enum.Parse(typeof(RegType), erf[1]);
                int l = Int32.Parse((erf[2]));
                rf = new RegFormat(t, l);
            }
            //rf.Clear();
            return rf;
        }

        internal object GetDefault()
        {
            if (Default != null && _type == RegType.PX)
                return Default;
            else if (Default != null && _type == RegType.P9)
                return int.Parse(Default);
            else if (_type == RegType.P9 || _type == RegType.PV)
                return 0;
            else if (_type == RegType.PX)
                return "";
            else
                return null;
        }
    }
}
