using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Impactro.Cobranca
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("40C71FBF-F84E-4845-BC3A-44AC89508120")]
    [ProgId("TesteNET")]
    public class TesteNET
    {
        public string GetString()
        {
            return "Texto vindo do C#";
        }

        public int GetInt()
        {
            return DateTime.Now.Day;
        }

        public int Soma(int a, int b)
        {
            return a + b;
        }

        public void Desenha(IntPtr hDC)
        {
            try
            {
                Graphics g = Graphics.FromHdc(hDC);
                g.Clear(Color.Yellow);
                g.DrawEllipse(Pens.Red, 10, 10, 100, 50);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public string Info
        {
            get
            {
                return this.GetType().Assembly.FullName + "\r\n" + this.GetType().Assembly.Location;
            }
        }
    }
}
