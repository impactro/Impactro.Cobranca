using System;
using Impactro.Cobranca;

// Baseado na versão 8.4 (01/09/2009)
// http://www.febraban.org.br/Acervo1.asp?id_texto=717&id_pagina=173
// Revisado 06/09/2012 baseado no SICREDI
namespace Impactro.Layout
{
    /// <summary>
    /// Classe generica para processamento de arquivos CNAB240
    /// </summary>
    public abstract class CNAB240<HA,HL,P,Q,TL,TA> : CNAB
    {
      
        /// <summary>
        /// Define se é para gerar aqruivos como teste ou produção
        /// </summary>
        public static bool Producao = true;

        /// Registro Header de Arquivo (Tipo = 0)
        public readonly Reg<HA> regHeaderArquivo;
        /// Registro Header de Lote (Tipo = 1)
        public readonly Reg<HL> regHeaderLote;
        /// Registros Inciciais do lote (opcional) (Tipo = 2)
        /// Registros de detalhe Segmentos (Tipo = 3)
        /// Registros finais do lote (opcional) (Tipo = 4)
        /// Registro trailer de lote (Tipo = 5)
        public readonly Reg<TL> regTrailerLote;
        /// Registro trailer de arquivo (Tipo = 9)
        public readonly Reg<TA> regTrailerArquivo;
        
        /// <summary>
        /// Gera o arquivos de remessa
        /// </summary>
        /// <param name="eReg">Tipo de registro a ser usado</param>
        public CNAB240(params Type[] eTypes)
            : base(typeof(HA), typeof(HL), typeof(P), typeof(Q), typeof(TL), typeof(TA))
        {
            SequencialLote = 1;
            regHeaderArquivo = new Reg<HA>();
            regHeaderLote = new Reg<HL>();
            regTrailerLote = new Reg<TL>();
            regTrailerArquivo = new Reg<TA>();

            if (eTypes != null)
                AddTypes(eTypes);
        }
    }

    /// <summary>
    /// Retorno Generico
    /// </summary>
    public enum CNAB240CobrancaRetorno
    {
        [RegFormat(RegType.PX, 44)]
        Brancos1,

        [RegFormat(RegType.P9, 10)] // 54
        Numero,

        [RegFormat(RegType.PX, 23)] // 77
        Brancos2,

        [RegFormat(RegType.PV, 15)] // 92
        Valor,

        [RegFormat(RegType.PX, 45)] // 137
        Brancos3,

        [RegFormat(RegType.PD, 8)] // 145
        Data

    }
}
