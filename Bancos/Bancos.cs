using System;
using System.Text;
using System.ComponentModel;

namespace Impactro.Cobranca
{
    /// <summary>
    /// Bancos Suportados
    /// </summary>
    public enum Bancos
    {
        [Description("001-Banco do Brasil")] BANCO_DO_BRASIL=1,
        [Description("021-Banestes")] BANESTES = 21,
        [Description("027-Besc")] BESC = 27,
        [Description("033-Banespa Santander")] BANESPA_SANTANDER=33,
        [Description("041-Barinsul")] BARINSUL = 41,
        [Description("047-Banese")] BANESE = 47,
        [Description("047-BRB")] BRB = 70,
        [Description("136-UniCred")] UniCred = 136, // Antigo 091
        [Description("104-Caixa Económica Federal")] CAIXA_ECONOMICA_FEDERAL=104,
        [Description("151-Nossa Caixa")] NOSSA_CAIXA=151,
        [Description("237-Bradesco")] BRADESCO=237,
        [Description("341-Itaú SA")] ITAU = 341,
        [Description("347-Sudameris")] SUDAMERIS = 347,
        [Description("353-Santander")] SANTANDER = 353,
        [Description("356-Real")] REAL = 356,
        [Description("389-Mercantil")]MERCANTIL = 389,
        [Description("399-HSBC")] HSBC = 399,
        [Description("409-Unibanco")]UNIBANCO = 409,
        [Description("422-Safra")] SAFRA = 422,
        [Description("745-CitiBank")] CITIBANK = 745,
        [Description("748-Sicredi")] SICREDI = 748,
        [Description("756-SICOOB")] SICOOB = 756
    }
}
