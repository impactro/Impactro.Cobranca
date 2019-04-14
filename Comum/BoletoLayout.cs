using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Interface para geração de layouts customizados de boleto
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("D8377D2F-4C77-4096-9F0B-4DB8C8659D74")]
    [ProgId("BoletoLayout")]
    public abstract class BoletoLayout
    {
        /// <summary>
        /// Metodo responsável por gerar os campos
        /// </summary>
        /// <param name="blt"></param>
        /// <returns></returns>
        abstract public void MakeFields(Boleto blt);

        /// <summary>
        /// Altura da imagem
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Largura da imagem
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Array com as posições dos textos
        /// </summary>
        protected List<FieldDraw> Fields { get; set; }

        /// <summary>
        /// Indica o numero de campos do array com as posições dos textos, ou -1 se ainda não foi inicializado
        /// </summary>
        public int Count { get { return Fields == null ? -1: Fields.Count; } }

        /// <summary>
        /// Retorna um campo em dada posição
        /// </summary>
        public FieldDraw Get(int n)
        {
            return Fields[n];
        }

        /// <summary>
        /// Limpa variáveis
        /// </summary>
        public void Clear()
        {
            Fields = null;
            Width = Height = 0;
        }

        /// <summary>
        /// Adiciona um novo campom apos o araay de campos ser iniciado
        /// </summary>
        public void Add(FieldDraw fd)
        {
            Fields.Add(fd);

            if (fd.Y + fd.Height > Height)
                Height = fd.Y + fd.Height;

            if (fd.X + fd.Width > Width)
                Width = fd.X + fd.Width;
        }

        /// <summary>
        /// Desenha os campos
        /// </summary>
        /// <param name="g">Instancia do ponteiro grafico</param>
        /// <param name="fat">Fator de tamanho</param>
        public virtual void Render(Graphics g, double fat)
        {
            // Impressão dos Campos e imagens
            for (int n = 0; n < Fields.Count; n++)
                Fields[n].Draw(g, 0, fat);
        }
    }
}
