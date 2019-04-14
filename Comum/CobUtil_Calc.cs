
using System;

namespace Impactro.Cobranca
{

    /// <summary>
    /// Utilitários para funções de cobrança
    /// </summary>
    public partial class CobUtil
    {
        /// <summary>
        /// Retorna o Código de barras com os campos separados por ponto e espeço
        /// </summary>
        /// <param name="cCodBarras">Código de barras a ser formatado</param>
        /// <param name="TamanhosCampoLivre">Tamanho dos pedaçoes do campo livre</param>
        public static string CodigoBarrasFormatado(string cCodBarras, int[] TamanhosCampoLivre)
        {
            string cOut = cCodBarras.Substring(0, 3) + "." +
                          cCodBarras.Substring(3, 1) + "." +
                          cCodBarras.Substring(4, 1) + "." +
                          cCodBarras.Substring(5, 4) + "." +
                          cCodBarras.Substring(9, 10) + "-";
            int n = 19;
            foreach (int i in TamanhosCampoLivre)
            {
                cOut += cCodBarras.Substring(n, i) + ".";
                n += i;
            }
            cOut = cOut.Substring(0, cOut.Length - 1);
            return cOut;
        }

        /// <summary>
        /// Retorna o Código de barras com os campos separados por ponto e espeço
        /// </summary>
        /// <param name="cCampoLivre">Código de barras a ser formatado</param>
        /// <param name="TamanhosCampoLivre">Tamanho dos pedaçoes do campo livre</param>
        public static string CampoLivreFormatado(string cCampoLivre, int[] TamanhosCampoLivre)
        {
            string cOut = "";
            int n = 0;
            foreach (int i in TamanhosCampoLivre)
            {
                cOut += cCampoLivre.Substring(n, i) + ".";
                n += i;
            }
            cOut = cOut.Substring(0, cOut.Length - 1);
            return cOut;
        }

        /// <summary>
        /// Calcula apenas a SOMA dos digitos retornados pela multiplicação dos pesos de acordo com a base selecionada
        /// Esta rotina é utilizada pelas rotinas Modulo11Padrao, Modulo11Especial para obter o valor total dos pessos.
        /// </summary>
        /// <param name="Sequencia">Sequencia Numerica a ser calculada</param>
        /// <param name="NumBase">É o Valor do Peso Máximo do multiplicador, 7, se de 7 a 2 (765432), ou 9, se for de 9 a 2 (98765432)</param>
        /// <returns>Valor Total da soma dos pesos</returns>
        public static int Modulo11Total(string Sequencia, int NumBase)
        {
            int Numero;
            int Contador = 0;
            int Multiplicador = 2;
            int TotalNumero = 0;

            // Para ser passado na geração da Exception em caso de conter conteudo não numerico na Sequencia informada
            string Caracter = "NULL";

            try
            {
                for (Contador = Sequencia.Length - 1; Contador >= 0; Contador--)
                {
                    Caracter = Sequencia.Substring(Contador, 1);
                    Numero = Int32.Parse(Caracter) * Multiplicador;
                    TotalNumero += Numero;
                    Multiplicador++;
                    if (Multiplicador > NumBase)
                        Multiplicador = 2;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("ERRO: {0} \r\nSequencia: '{1}' Base: '{2}' Posição: '{3}' Caracter: '{4}'", ex.Message, Sequencia, NumBase, Contador, Caracter), ex);
            }
            return TotalNumero;
        }

        /// <summary>
        /// O Modulo 11 Padrão, é utilizado utilizado para o calculo do digito verificador do código de barras, e tambem por alguns bancos para calculo de digitos no Nosso Numero, Conteoles, e verificações.
        /// É reconhecido por não retornar Zero
        /// </summary>
        /// <param name="Sequencia">Sequencia Numerica a ser calculada</param>
        /// <param name="NumBase">É o Valor do Peso Máximo do multiplicador, 7, se de 7 a 2 (765432), ou 9, se for de 9 a 2 (98765432)</param>
        /// <returns>O Digito Verificador</returns>
        /// <remarks></remarks>
        public static int Modulo11Padrao(string Sequencia, int NumBase)
        {
            int TotalNumero = Modulo11Total(Sequencia, NumBase);
            TotalNumero *= 10;
            int Resto = TotalNumero % 11;
            if (Resto == 0 || Resto == 10)
                return 1;
            else
                return Resto;

        }

        /// <summary>
        /// O Modulo 11 Especial, é utilizado por alguns bancos que utilizam o digito 0 quando o reto é 10
        /// </summary>
        /// <param name="Sequencia">Sequencia Numerica a ser calculada</param>
        /// <param name="NumBase">É o Valor do Peso Máximo do multiplicador, 7, se de 7 a 2 (765432), ou 9, se for de 9 a 2 (98765432)</param>
        /// <returns>O Digito Verificador</returns>
        public static int Modulo11Especial(string Sequencia, int NumBase)
        {
            int TotalNumero = Modulo11Total(Sequencia, NumBase);
            TotalNumero *= 10;
            int Resto = TotalNumero % 11;
            if (Resto == 10) // Se Resto = 0 (zero) retorna o normalmente via 'else'
                return 0;
            else
                return Resto;
        }

        /// <summary>
        /// Logica especial para modulo 11, que retorna -1 quando o resto é 1
        /// </summary>
        /// <param name="sequencia">Sequencia a ser calculada</param>
        /// <returns>Retorna o digito, ou -1 quando nescessário</returns>
        public static int Modulo11Negativo(string sequencia)
        {
            int Total = CobUtil.Modulo11Total(sequencia, 7);
            // De acordo com a nota disponivel no manual do Banco Nossa Caixa na página 22
            // o resto não deve ser aproximado, e deveremos considerar o digito=0
            int Resto = Total % 11;
            if (Resto == 0)
                return 0;
            else if (Resto > 1)
                return 11 - Resto;
            else
                // Recalcular D2 com (D1+1)
                // Retorna um valor negativo informando que deve ser recalculado com incremento
                return -1;
        }

        /// <summary>
        /// soma 2 numeros grandes em forma de string
        /// </summary>
        /// <param name="A">Numero grande: 34523987459287634587639845769</param>
        /// <param name="B">Numero grande: 99283498453874657862384628376</param>
        /// <returns>O resuldado da Soma</returns>
        public static string Soma(string A, string B)
        {
            int n, nC, n1, n2;
            // Ajusta o tamanho das string
            if (A.Length > B.Length)
                B = Right(B, A.Length);
            else if (B.Length > A.Length)
                A = Right(A, B.Length);

            nC = 0;
            string C = "";

            for (n = A.Length - 1; n >= 0; n--)
            {
                n1 = Int32.Parse(A.Substring(n, 1));
                n2 = Int32.Parse(B.Substring(n, 1));
                n1 = n1 + n2 + nC;

                if (n1 > 9)
                {
                    nC = 1;
                    n1 = n1 - 10;
                }
                else
                    nC = 0;

                C = n1.ToString() + C;

            }

            C = nC.ToString() + C;

            return C;
        }

        /// <summary>
        /// Calcula o Fator de vencimento, que representa o numero de dias corridos desde a datab base 7/10/1997
        /// </summary>
        /// <param name="DataVencimento">Data de Vendimento, a ser calculada!
        /// Conforme determinado pelo Banco Central do Brasil, por meio das circulares 3.598 e 3.656, em vigor a partir de 28/06/2013,  fica proibido boletos sem valor  e  sem vencimento,  ou com  as informações “Vencimento à vista” e “Contra apresentação”.</param>
        /// <returns>Retorna o numero de dias desde 7/10/1997, ou 0(Zero) se DataVencimento=DateTime.MinValue</returns>
        public static int CalcFatVenc(DateTime DataVencimento)
        {
            // Verifica se é sem data de Vencimento (contra apresentação)
            if (DataVencimento == DateTime.MinValue)
                return 0;

            DateTime dtBase = new DateTime(1997, 10, 7);
            TimeSpan Result = DataVencimento.Subtract(dtBase);

            if (Result.Days > 9999)
            {
                DateTime dtBaseNew = new DateTime(2022, 5, 29);
                Result = DataVencimento.Subtract(dtBaseNew);
            }
                
            if (Result.Days < 0 && Result.Days > 9999)
                throw new Exception("Data de vencimento inválida");

            return Result.Days;
        }

        /// <summary>
        /// Calcula a Linha Digitável do Código de Barras
        /// </summary>
        /// <param name="CodBarras">Código de barras com 44 numeros, sem espaços ou pontos, SOMENTE NUMEROS</param>
        /// <returns></returns>
        public static string CalcLinDigitavel(string CodBarras)
        {
            if (CodBarras.Length != 44)
                throw new Exception("O Código de Barras deve ter 44 numeros! Valor Informado: '" + CodBarras + "' Lenth: " + CodBarras.Length.ToString());

            String cCampo1, cCampo2, cCampo3, cCampo4, cCampo5;

            cCampo1 = CodBarras.Substring(0, 4) + CodBarras.Substring(19, 5);
            cCampo1 += Modulo10(cCampo1);
            cCampo1 = cCampo1.Substring(0, 5) + "." + cCampo1.Substring(5, 5);

            cCampo2 = CodBarras.Substring(24, 10);
            cCampo2 += Modulo10(cCampo2);
            cCampo2 = cCampo2.Substring(0, 5) + "." + cCampo2.Substring(5, 6);

            cCampo3 = CodBarras.Substring(34, 10);
            cCampo3 += Modulo10(cCampo3);
            cCampo3 = cCampo3.Substring(0, 5) + "." + cCampo3.Substring(5, 6);

            cCampo4 = CodBarras.Substring(4, 1);
            cCampo5 = CodBarras.Substring(5, 14);

            return cCampo1 + " " + cCampo2 + " " + cCampo3 + " " + cCampo4 + " " + cCampo5;
        }

        /// <summary>
        /// O Modulo 10 representa é um digito baseado mo modulo da soma dos pesos de cada digito multiplicados por 2 ou 1
        /// </summary>
        /// <param name="Cadeia">Sequencia Numerica a ser calculada</param>
        /// <returns>O Digito Verificador</returns>
        public static int Modulo10(string Cadeia)
        {
            int Mult, Total, Pos, Res;
            Mult = Cadeia.Length % 2;
            Mult++;
            Total = 0;
            for (Pos = 0; Pos < Cadeia.Length; Pos++)
            {
                Res = Int32.Parse(Cadeia.Substring(Pos, 1)) * Mult;
                if (Res > 9)
                    Res = Res / 10 + (Res % 10);

                Total += Res;

                if (Mult == 2)
                    Mult = 1;
                else
                    Mult = 2;
            }

            Total = ((10 - (Total % 10)) % 10);

            return Total;
        }

        /// <summary>
        /// Gera uma string que representa um código de barras de um numero especifico
        /// </summary>
        /// <param name="NumTexto">digitos a serem codificados</param>
        /// <returns>retona uma sequancia das ssequencias "bf","bl","pf","pl"</returns>
        /// <remarks>
        /// bf -> Branco Fino
        /// bl -> Branco Largo
        /// pf -> Preto Fino
        /// pl -> Preto Largo
        /// </remarks>
        public static String BarCode(String NumTexto)
        {
            var cOut = new System.Text.StringBuilder();
            String f, texto;
            int fi, f1, f2, i;
            string[] BarCodes = new string[100];
            BarCodes[0] = "00110";
            BarCodes[1] = "10001";
            BarCodes[2] = "01001";
            BarCodes[3] = "11000";
            BarCodes[4] = "00101";
            BarCodes[5] = "10100";
            BarCodes[6] = "01100";
            BarCodes[7] = "00011";
            BarCodes[8] = "10010";
            BarCodes[9] = "01010";

            for (f1 = 9; f1 >= 0; f1--)
            {
                for (f2 = 9; f2 >= 0; f2--)
                {
                    fi = f1 * 10 + f2;
                    texto = "";
                    for (i = 0; i < 5; i++)
                    {
                        texto +=
                            BarCodes[f1].Substring(i, 1) +
                            BarCodes[f2].Substring(i, 1);
                    }
                    BarCodes[fi] = texto;
                }
            }

            // Inicialização padrão
            cOut.Append("pf");
            cOut.Append("bf");
            cOut.Append("pf");
            cOut.Append("bf");

            texto = NumTexto;
            if (texto.Length % 2 != 0)
                texto = "0" + texto;

            //Draw dos dados
            while (texto.Length > 0)
            {
                i = Int32.Parse(texto.Substring(0, 2));
                texto = texto.Substring(2);
                f = BarCodes[i];
                for (i = 0; i < 10; i += 2)
                {
                    if (f.Substring(i, 1) == "0")
                        cOut.Append("pf");
                    else
                        cOut.Append("pl");

                    if (f.Substring(i + 1, 1) == "0")
                        cOut.Append("bf");
                    else
                        cOut.Append("bl");

                }
            }

            // Finalização padrão
            cOut.Append("pl");
            cOut.Append("bf");
            cOut.Append("pf");

            return cOut.ToString();
        }

        /// <summary>
        /// Alinha uma string contendo digitos a direita, preenchendo a parte esquerda com zeros, forçando ser uma string com um numero exato de digitos
        /// </summary>
        /// <param name="cValor">Valor a ser completado, ex: "123"</param>
        /// <param name="nSize">Tamanho desejado ex: 5</param>
        /// <returns>Nova string completada com zeros ex: "00123"</returns>
        public static String Right(String cValor, int nSize)
        {
            cValor = cValor.PadLeft(nSize, '0');
            cValor = cValor.Substring(cValor.Length - nSize);
            return cValor;
        }
    }
}
