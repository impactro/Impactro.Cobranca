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
    /// Espeficica uma expressão regular para identificar o tipo registro
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class RegLayout : Attribute
    {
        private Regex re;

        public RegLayout(string cExprecao)
        {
            re = new Regex(cExprecao);
        }

        public bool IsThis(string cLine)
        {
            return re.IsMatch(cLine);
        }

        public string DateFormat6;
        public string DateFormat8;
        public string DateFormat10;
        public string DateFormat12;
        public string DateFormat14;

        public bool Acentos = false;
        public bool Upper = false;

    }

}
