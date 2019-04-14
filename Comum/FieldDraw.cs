using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Impactro.Cobranca
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("E7ECDEC9-CB12-4C91-ADD2-D2538D5CD983")]
    [ProgId("FieldDraw")]
    public class FieldDraw
    {
        public int X, Y, Width, Height;
        public string Campo;
        public string Valor;
        public StringAlignment Align { get { return (StringAlignment)AlignText; } set { AlignText = (int)value; } }
        public int AlignText;
        public byte Linhas;
        public bool Destaque;
        public Image Image; // por ser o logo, o código de barras, ou qualquer imagem (logo empresa)

        public static Font FontCampo, FontValor, FontLinha;
        public static Color DestaqueColor = Color.LightGray;

        public static float FontCampoSize=7;
        public static float FontValorSize = 9; 
        public static float FontLinhaSize = 11;

        public static string FontCampoName = "Verdana";
        public static string FontValorName = "Arial";
        public static string FontLinhaName = "Arial";

        public static FontStyle FontCampoStyle = FontStyle.Regular;
        public static FontStyle FontValorStyle = FontStyle.Bold;
        public static FontStyle FontLinhaStyle = FontStyle.Bold;
        
        static FieldDraw()
        {
            Reset();
        }

        public static void Reset()
        {
            FontCampo = new Font(FontCampoName, FontCampoSize, FontCampoStyle);
            FontValor = new Font(FontValorName, FontValorSize, FontValorStyle);
            FontLinha = new Font(FontLinhaName, FontLinhaSize, FontLinhaStyle);
        }

        public static void ResetSize(double escala)
        {
            if (escala >= 1)
            {
                // Tamanho dos titulos dos campos
                FieldDraw.FontCampoSize = (float)(5 * escala / 3);

                // Tamanho dos valores dos campos
                FieldDraw.FontValorSize = (float)(7 * escala / 3);

                // Tamanho da linha digitável
                FieldDraw.FontLinhaSize = (float)(9 * escala / 3);

                // Recria as instancias dos fontes
                FieldDraw.Reset();
            }
        }

        public FieldDraw()
        {
            X = 0;
            Y = 0;
            Width = 40;
            Height = 7;
            Campo = null;
            Valor = null;
            Align = StringAlignment.Far;
            Linhas = 0x0F; // TOP, RIGHT, BOTTOM, LEFT: 0011 => 3
            Image = null;
        }

        /// <summary>
        /// Cria uma imagem de um campo e renderiza com as linhas de acordo com os bits de desenho
        /// </summary>
        public FieldDraw(int nX, int nY, string cCampo, string cValor)
        {
            X = nX;
            Y = nY;
            Width = 40;
            Height = 7;
            Campo = cCampo;
            Valor = cValor;
            Align = StringAlignment.Far;
            Linhas = 0x0F; // TOP, RIGHT, BOTTOM, LEFT: 0011 => 3
            Image = null;
        }

        public FieldDraw(int nX, int nY, string cCampo, string cValor, int nWidth, int nHeight = 7, StringAlignment sAlign = StringAlignment.Far, byte bLinhas = 0x0F)
        {
            X = nX;
            Y = nY;
            Width = nWidth;
            Height = nHeight;
            Campo = cCampo;
            Valor = cValor;
            Align = sAlign;
            Linhas = bLinhas; // TOP, RIGHT, BOTTOM, LEFT: 0011 => 3
            Image = null;
        }

        /// <summary>
        /// Desenha um campo em Tela de acordo com o Fator de Conversçao atual
        /// </summary>
        public void Draw(Graphics g, int nTop, double fat)
        {
            int yt = nTop + Y;

            if (Image != null)
            {
                if (Width == 0 || Height == 0)
                    g.DrawImage(Image, (int)(X * fat), (int)(yt * fat));
                else
                    g.DrawImage(Image, (int)(X * fat), (int)(yt * fat), (int)(Width * fat), (int)(Height * fat));
            }
            else
            {
                if (Destaque)
                    g.FillRectangle(new SolidBrush(FieldDraw.DestaqueColor), (int)(X * fat) + 1, (int)(yt * fat) + 1, (int)((Width) * fat), (int)(Height * fat));

                if ((Linhas & 0x08) == 0x08) // TOP
                    g.DrawLine(Pens.Black, (int)(X * fat), (int)(yt * fat), (int)((X + Width) * fat), (int)(yt * fat));
                if ((Linhas & 0x04) == 0x04) // RIGHT
                    g.DrawLine(Pens.Black, (int)((X + Width) * fat), (int)(yt * fat), (int)((X + Width) * fat), (int)((yt + Height) * fat));
                if ((Linhas & 0x02) == 0x02) // BOTTOM
                    g.DrawLine(Pens.Black, (int)(X * fat), (int)((yt + Height) * fat), (int)((X + Width) * fat), (int)((yt + Height) * fat));
                if ((Linhas & 0x01) == 0x01) // LEFT
                    g.DrawLine(Pens.Black, (int)(X * fat), (int)(yt * fat), (int)(X * fat), (int)((yt + Height) * fat));

                if (string.IsNullOrEmpty( Campo ))
                    g.DrawString(Valor, FontLinha, Brushes.Black,
                        new RectangleF((int)(X * fat), (int)((yt + 1) * fat), (int)(Width * fat), (int)((Height - 1) * fat)),
                        new StringFormat() { Alignment = Align });
                else
                {
                    g.DrawString(Campo, FontCampo, Brushes.Black, (int)(X * fat), (int)(yt * fat));
                    g.DrawString(Valor, FontValor, Brushes.Black,
                        new RectangleF((int)(X * fat) + 1, (int)((yt + 3) * fat), (int)(Width * fat) - 2, (int)((Height - 3) * fat)),
                        new StringFormat() { Alignment = Align });
                }
            }
        }
    }
}