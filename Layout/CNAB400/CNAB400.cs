using System;
using Impactro.Cobranca;
using System.IO;
using System.Runtime.InteropServices;

// Baseado no ITAU Cobrança 400 (maio/2010) e Bradesco 07 (26/08/09)
namespace Impactro.Layout
{
    /// <summary>
    /// Classe base para os geradores de arquivos CNAB400
    /// Cada banco tem seu algoritomo e personalizações de geração
    /// Essa classe é apenas um facilitador de comum reuso para base de cada banco
    /// </summary>
    [ComVisible(false)]
    public abstract class CNAB400<H,R,T> : CNAB
    {
        /// <summary>
        /// Informações do Header do arquivo
        /// </summary>
        public readonly Reg<H> regArqHeader;

        /// <summary>
        /// Informações do Trailer(rodape/linha final) do arquivo
        /// </summary>
        public readonly Reg<T> regArqTrailer;
     
        /// <summary>
        /// Gera o arquivos de remessa
        /// </summary>
        public CNAB400()
            : base(typeof(H), typeof(R) , typeof(T))
        {
            regArqHeader = new Reg<H>();
            regArqTrailer = new Reg<T>();
        }
    }

    #region "Layout Comum de Remessa Arquivo"

    /// <summary>
    /// Trailer Geral do Arquivo CNAB400 (Tipo=9)
    /// </summary>
    [RegLayout(@"^9", Upper = true)]
    public enum CNAB400ArquivoTrailer
    {
        /// <summary>
        /// Tipo de Registro
        /// </summary>
        [RegFormat(RegType.P9, 1, Default = "9")]
        Controle_Registro,

        /// <summary>
        /// Espaços em branco
        /// </summary>
        [RegFormat(RegType.PX, 393)]
        CNAB_Brancos,

        /// <summary>
        /// Número seqüencial do registro no arquivo
        /// </summary>
        [RegFormat(RegType.P9, 6)]
        Sequencia
    }

    #endregion

}
