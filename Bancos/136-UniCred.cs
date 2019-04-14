using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Rotinas para o Banco UniCred 
    /// Antigo banco 091
    /// http://www.unicred.com.br/florianopolis/frame.php?class=PaginaDinamica&method=Visualizar&cd_pagina_dinamica=2946&caption[]=Informativos++&caption[]=Compensa%E7%E3o+Pr%F3pria
    /// </summary>
    public abstract class Banco_UniCred
    {
        /// <summary>
        /// Digito do Código do Banco
        /// </summary>
        public const string BancoDigito = "?";

        /// <summary>
        /// Rotina de Geração do Campo livre usado no Código de Barras para formar o IPTE
        /// </summary>
        /// <param name="blt">Intancia da Classe de Boleto</param>
        /// <returns>String de 25 caractere que representa 'Campo Livre'</returns>
        public static string CampoLivre(Boleto blt, string cAgenciaNumero, string cContaNumero, string cNossoNumero)
        {
            cAgenciaNumero = CobUtil.Right(cAgenciaNumero, 4);
            cContaNumero = CobUtil.Right(cContaNumero, 10);
            cNossoNumero = CobUtil.Right(cNossoNumero, 10);

            string cDV = CobUtil.Modulo11Padrao(cNossoNumero, 9).ToString();

            //O campo nosso numero tem que iniciar com 9 e ter 9 digitos
            string cLivre = 
                cAgenciaNumero +
                cContaNumero + 
                cNossoNumero +
                cDV;

            //Monta o Nosso Numero
            blt.NossoNumeroExibicao = cNossoNumero + "-" + cDV;

            return cLivre;
        }
    }
}
