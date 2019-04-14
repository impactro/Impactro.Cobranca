using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using System;
using System.Web.UI;

[assembly: AssemblyTitle("Impactro.Cobranca")]
[assembly: AssemblyCompany("IMPACTRO Informática")]
[assembly: AssemblyCopyright("Copyright © IMPACTRO Informática 2005-2019")]
[assembly: AssemblyTrademark("Fábio Ferreira de Souza")]
[assembly: AssemblyDescription("BoletoNet-Layouts")]
[assembly: AssemblyProduct("Cobranças")]
[assembly: AssemblyVersion("2.17.4.14")]
[assembly: Guid("49578a1e-e65b-4f50-a244-8ced0c0c5f88")]
[assembly: NeutralResourcesLanguageAttribute("pt-BR")]

#if VB6
// http://support.microsoft.com/kb/817248/pt-br
// http://msdn.microsoft.com/en-us/library/ms973800.aspx
// http://msdn.microsoft.com/en-us/library/aa645712(v=vs.71).aspx
// Habilite manualmente a opção: "register for COM interop" nas configurações do projeto (build)
// Password: "BoletoNet" (Gerado na tela de propriedades diretamente pelo visual Studio)
// para gerar a versão para vb6 o componente precisa estar assinado em propriedades
[assembly: ComVisible(true)]
[assembly: CLSCompliant(false)]
#endif

#if NET2 || NET4
[assembly: WebResource("Impactro.Resources.001.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.021.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.027.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.033.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.041.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.047.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.070.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.136.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.104.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.151.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.237.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.341.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.347.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.353.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.356.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.389.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.399.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.409.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.422.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.745.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.748.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.756.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.p.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.b.gif", "image/gif")]
[assembly: WebResource("Impactro.Resources.corte.gif", "image/gif")]
#endif
