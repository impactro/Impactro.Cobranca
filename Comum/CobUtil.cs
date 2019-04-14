using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Drawing;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Impactro.Cobranca
{

    /// <summary>
    /// Utilitários para funções de cobrança
    /// </summary>
    [ComVisible(false)]
    public partial class CobUtil
    {
        /// <summary>
        /// Encoding Default: // https://en.wikipedia.org/wiki/Windows-1252
        /// </summary>
        public static int DefaultEncoding = 1252; // "ISO-8859-1";
        /// <summary>
        /// Clona um objeto e suas propriedades
        /// </summary>
        public static object Clone(object orignal)
        {
            Type tp = orignal.GetType();

            // cria uma nova instancia
            object newobj = tp.Assembly.CreateInstance(tp.FullName);

            // Define todos os campos
            foreach (FieldInfo fi in tp.GetFields())
                fi.SetValue(newobj, fi.GetValue(orignal));

            // Define todas as propriedades
            foreach (PropertyInfo pi in tp.GetProperties())
            {
                try
                {
                    if (pi.CanWrite && pi.CanRead)
                        pi.SetValue(newobj, pi.GetValue(orignal, null), null);
                }
                catch(Exception ex)
                {
                    throw new Exception($"propriedade {pi.Name} não pode ser clonada", ex);
                }
            }

            // retorna o novo objeto
            return newobj;
        }

        /// <summary>
        /// Gera a string do boleto em base64
        /// </summary>
        public static string ToBase64String(Bitmap bmp, ImageFormat imageFormat)
        {
            string base64String = string.Empty;

            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, imageFormat);

            memoryStream.Position = 0;
            byte[] byteBuffer = memoryStream.ToArray();

            memoryStream.Close();

            base64String = Convert.ToBase64String(byteBuffer);
            byteBuffer = null;

            return base64String;
        }

        /// <summary>
        /// Gera uma imagen HTML codigicada em base64
        /// </summary>
        public static string ToBase64ImageTag(Bitmap bmp, ImageFormat imageFormat)
        {
            string imgTag = string.Empty;
            string base64String = string.Empty;

            base64String = ToBase64String(bmp, imageFormat);

            imgTag = "<img src=\"data:image/" + imageFormat.ToString() + ";base64,";
            imgTag += base64String + "\" ";
            imgTag += "width=\"" + bmp.Width.ToString() + "\" ";
            imgTag += "height=\"" + bmp.Height.ToString() + "\" />";

            return imgTag;
        }

        public static string ClearHTML(string cValor)
        {
            if (cValor == null)
                return "";
            return Regex.Replace(cValor, @"<[^>]*>", " / ");
        }

        /// <summary>
        /// Retorna só os numeros de uma string, ou nulo se a string de entrada também for nula
        /// </summary>
        public static string SoNumeros(string s)
        {
            if (s == null)
                return null;
            Regex re = new Regex(@"\d");
            Match m = re.Match(s);
            string cOut = "";
            while (m.Success)
            {
                cOut += m.Value;
                m = m.NextMatch();
            }
            return cOut;
        }

        /// <summary>
        /// Gera um BitMap baseado em uma string 'bfpfbl' para geração de código de barras com escala padrão
        /// </summary>
        /// <param name="NumTexto"></param>
        /// <returns></returns>
        public static Bitmap BarCodeImage(string NumTexto)
        {
            return BarCodeImage(NumTexto, 3, Boleto.defaultDPI);
        }

        /// <summary>
        /// Gera um BitMap baseado em uma string 'bfpfbl' para geração de código de barras ( Alterado por Alexandre Savelli Bencz _
        /// </summary>
        /// <param name="NumTexto">String no formarto 'bfplblpl'</param>
        /// <param name="nScale">Quanto é a escala do codigo de barras</param>
        /// <param name="resolucao">Resolucao do código de barras</param>
        /// <returns>Bitmap</returns>
        public static Bitmap BarCodeImage(string NumTexto, int nScale, float resolucao = 600f)
        {
            // Transforma o numero em uma string padrão de barras
            string cCodBar = BarCode(NumTexto);

            if (nScale < 3)
                throw new Exception("Escala minima é 3");

            // Ajusta a Escala padrão 
            //nScale /= 3; // Atenção, o ideal para a escala é ser multiplo de 3

            int wSF = nScale / 3;
            int wSL = nScale;

            // Codigo de Barras 2 por 5  =>  2 digitos são representados por 5 Barras PBPBP largas ou finas
            int nWidth = NumTexto.Length * 4 * nScale;

            Bitmap bmp = new Bitmap(nWidth, 50);
            bmp.SetResolution(resolucao, resolucao);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            // Posição da linha atualmente desenhada (cursor)
            int nX = 0;
            for (int n = 0; n < cCodBar.Length; n += 2)
            {
                switch (cCodBar.Substring(n, 2))
                {
                    case "bf":
                        g.FillRectangle(Brushes.White, nX, 0, wSF, 50);
                        nX += wSF;
                        break;
                    case "pf":
                        g.FillRectangle(Brushes.Black, nX, 0, wSF, 50);
                        nX += wSF;
                        break;
                    case "bl":
                        g.FillRectangle(Brushes.White, nX, 0, wSL, 50);
                        nX += wSL;
                        break;
                    case "pl":
                        g.FillRectangle(Brushes.Black, nX, 0, wSL, 50);
                        nX += wSL;
                        break;
                }
            }

            // Extrai apenas a imagem 100% util
            Bitmap bmp2 = new Bitmap(nX, 50);
            bmp2.SetResolution(resolucao, resolucao);
            g = Graphics.FromImage(bmp2);
            g.DrawImage(bmp, 0, 0);

            return bmp2;
        }

        /// <summary>
        /// Tenta obter um numero Int32 de um objeto qualquer, se der erro retorna 0
        /// </summary>
        /// <param name="oInt">Valor a ser obtido</param>
        public static int GetInt(object oInt)
        {
            try
            {
                if (oInt == DBNull.Value || oInt == null)
                    return 0;

                string cInt = oInt.ToString().ToUpper();
                cInt = cInt.Replace(".", "");
                cInt = cInt.Replace(" ", "");
                cInt = cInt.Replace("R$", "");
                int n = cInt.IndexOf(",");
                if (n > 0)
                    cInt = cInt.Substring(0, n);
                return Int32.Parse(cInt);
            }
            catch
            {
                return 0;
            }
        }

        public static long GetLong(object oLong, int nDefault = 0)
        {
            try
            {
                if (oLong == DBNull.Value || oLong == null)
                    return nDefault;

                return Int64.Parse(oLong.ToString());
            }
            catch
            {
                return nDefault;
            }
        }

        public static string RemoveAcentos(string cTexto)
        {
            string s = cTexto.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < s.Length; k++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(s[k]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    if (s[k] >= 32 && s[k] <= 126) // Somente os caracteres basicos
                        sb.Append(s[k]);
                    else
                        sb.Append(" "); // precisa manter o tamanho de caracteres
                } 
            }
            return sb.ToString();
        }

        /// <summary>
        /// Le uma imagem especifica dentro dos Resources da Impactro.Cobranca
        /// </summary>
        /// <param name="cImg">ome da Imagem</param>
        /// <returns>Objeto BITMAP da Imagem solicitada ou null se não for encontrada</returns>
        public static Bitmap ResLoadImage(string cImg)
        {
            System.Reflection.Assembly thisDLL = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream file = thisDLL.GetManifestResourceStream("Impactro.Resources." + cImg);
            System.Drawing.Bitmap resBMP = new System.Drawing.Bitmap(file);
            return resBMP;
        }
#if NET2 || NET4
        /// <summary>
        /// Retorna o resultado de um requerimento WebPost passando dados de formularios codificados
        /// </summary>
        /// <param name="cURL">URL do requerimento</param>
        /// <param name="cPost">Dados a serem enviados</param>
        /// <returns>Conteudo (string) resultante</returns>
        public static string WebPostRequest(string cURL, string cPost)
        {
            Byte[] byteArray = System.Text.UTF8Encoding.UTF8.GetBytes(cPost);
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(cURL);
            req.UserAgent = UserAgent;
            req.Method = WebRequestMethods.Http.Post;
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = byteArray.Length;
            Stream newStream = req.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length);
            newStream.Close();
            req.Timeout = 60000;
            WebResponse webResponse = req.GetResponse();
            StreamReader srResponseReader = new StreamReader(webResponse.GetResponseStream());
            string strResponseData = srResponseReader.ReadToEnd();
            srResponseReader.Close();
            return strResponseData;
        }

        /// <summary>
        /// Efetua um download de uma string usando um objeto do tipo HttpWebRequest
        /// </summary>
        /// <param name="cURL">URL</param>
        /// <param name="cGet">GET - QueryString</param>
        /// <returns></returns>
        public static string WebGetRequest(string cURL, string cGet)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(cURL + "?" + cGet);
            req.Method = WebRequestMethods.Http.Get;
            req.Timeout = 15000;
            req.UserAgent = UserAgent;
            WebResponse webResponse = req.GetResponse();
            StreamReader srResponseReader = new StreamReader(webResponse.GetResponseStream());
            string strResponseData = srResponseReader.ReadToEnd();
            srResponseReader.Close();
            return strResponseData;
        }

        /// <summary>
        /// Efetua um download de um conteudo usando objetod do tipo WebClient
        /// </summary>
        /// <param name="cURL">URL</param>
        /// <param name="cGet">GET - QueryString</param>
        /// <returns>Conteudo em UTF-8</returns>
        public static string WebClientRequest(string cURL, string cGet)
        {
            WebClient web = new WebClient();
            byte[] bt = web.DownloadData(cURL + "?" + cGet);
            return System.Text.UTF8Encoding.Default.GetString(bt);
        }
#endif
        /// <summary>
        /// UserAgent padrão para as chamadas de GetResponseHtml
        /// </summary>
        public static string UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727)";
        /// <summary>
        /// Obtem o conteudo HTML de uma página (URL)
        /// </summary>
        /// <param name="cURL">URL da página a ser obtida</param>
        /// <returns>Conteudo HTML da URL informada</returns>
        public static string WebGetRequest(string cURL)
        {
            HttpWebRequest req;
            WebResponse res;
            Stream str;
            StreamReader sRead;
            String cResult;
            req = (HttpWebRequest)HttpWebRequest.Create(cURL);
            req.Method = "GET";
            req.Timeout = 60000;
            req.UserAgent = UserAgent;
            res = req.GetResponse();
            str = res.GetResponseStream();
            sRead = new StreamReader(str, System.Text.ASCIIEncoding.UTF7, true);
            cResult = sRead.ReadToEnd();
            return cResult;
        }
    }
}
