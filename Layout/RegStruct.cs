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

    public static class rft // Reflection Tools
    {
        private static SortedList<string, string[]> _FieldsType;

        internal static string[] GetFields(Type tp)
        {
            if (_FieldsType == null)
                _FieldsType = new SortedList<string, string[]>();

            if (_FieldsType.ContainsKey(tp.Name))
                return _FieldsType[tp.Name];

            string[] cFields = Enum.GetNames(tp);

            _FieldsType.Add(tp.Name, cFields);
            
            return cFields;
        }
    }

    /// <summary>
    /// Interface de integração IREG[object]
    /// </summary>
    public interface IReg
    {
        string Line { get; set; }
        string OriginalLine { get; }
        Type NameType { get; }
        object[] Values { get; }
        IReg Copy();
        string Dump { get; }
        void Set(string key, object value);
        object Get(string key);
    }

    /// <summary>
    /// Tipos de registro
    /// </summary>
    public enum RegType
    {
        /// <summary>
        /// digitos numericos inteiros (Int32)
        /// </summary>
        P9,

        /// <summary>
        /// digitos numericos decimais com 2 digitos (Double)
        /// </summary>
        PV,

        /// <summary>
        /// Texto (String)
        /// </summary>
        PX,

        /// <summary>
        /// Data (DateTime)
        /// </summary>
        PD,

        /// <summary>
        /// Hora (DateTime)
        /// </summary>
        PH
    }
}
