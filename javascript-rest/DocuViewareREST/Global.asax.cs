using GdPicture14.WEB;
using System.Web;
using System.Web.Http;

namespace DocuViewareREST
{
    public class WebApiApplication : HttpApplication
    {
        public static readonly int SESSION_TIMEOUT = 20; //Set to 20 minutes. Use -1 to handle DocuVieware session timeout through ASP.NET session mechanism.
        private static readonly bool STICKY_SESSION = true; //Set false to use DocuVieware on Servers Farm with non sticky sessions.
        private const DocuViewareSessionStateMode DOCUVIEWARE_SESSION_STATE_MODE = DocuViewareSessionStateMode.InProc; //Set DocuViewareSessionStateMode.File is STICKY_SESSION is False.

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DocuViewareManager.SetupConfiguration(STICKY_SESSION, DOCUVIEWARE_SESSION_STATE_MODE, GetCacheDirectory());
            DocuViewareLicensing.RegisterKEY("JxfSGLnUd5L4w02eojBJHgL7ycX6lJjgsOYe4Ralhz8yyqNkOjbgNXt8ObuvdChToxT-A_DL32OWm8mcwaPsVB8CzSToZELjw1_fJmwVqAlh-WJJAKkJeTvRNDKp2oQvp75HPgNN8zT5ZOMeAczyL8BQ39BLc_ol0QvobD2VE9Ly_mYnbJSKCYQzVZMvsKTN2EEQAdIKeVJGch0U5mJgK_d31xE2vNx2L8LtFEFu1oLvL5hAu-MqGUavVbcCpCcJsXz56_OyANqqGurUvCZXamDxVhgeI_pC4RPn6w2Qb7VaDXYjuGXKV1K5IiEcEGxYRekeqCnyLRs36wYRatNeoOtyeYcwmYk0z0mVrlqNj-m23VHhboqrHoW1Yy6_lwhnhw4mxvY6Y0VzGcWX8lIcA8XM9qJcFOQroqeyW4nZgMGoRVeESaKIL2OD8kLIe_brIdBXQevgoHTlhBeIC4eU3bNszMrXikXXDpWdcGgJYky37e6LP2xzmm6WHMwpBMfELPpfsivBma2DiNDkbwH0jcqmxATibrAS0psjXMYM03XC5lo7sd65tR1uHF14Lmh2ZczMKWQH_2mbX1CN_XbUfbTifuvelTJ78u2xUW-7CMeeLKBg6ZhrL4Hn5efmqKfIR8GHPXgo79YEv1BQQJVR1rs8p7-trCSb_72Qg7w6Jyesn6tM4PvdtSFFoGxQK7e7qzYzicJKh3yHPTEXsWf7XuZHL_fv2ZVQkeeCKQ1gLX75XdlnCCAsRF0d-f1JQtcZY3f-sZ3AJwYICCGmLCmefeG4lrsBQOqRusjvGtJrjbNC9JvhX3N4_qb6Y89WhblanQyfM8aa6NZ1vj7FRu8RQKzGztM3YskmkhGtv-rTfolkChtvM52RYDIdmNL0L7mHyfJAWg8WQkc8dSO3aB1eSfq4fQKsgQe87zhIgGp4sBbpUxUrebcaphfBnUFg0A1rvZOMhUQ9saG-T2fvVLFfju9_ji0CBzEEbdLovmB7_VhZTiJ_Xaot-z6OEX1khbXfOH5i74vS_fD2A844dhpgkd7d2JosRlv4eap6ikUTJXwSJRkOQzm4MGb1L1PkwOid"); //Unlocking DocuVieware. Please insert your demo or commercial license key here.
            DocuViewareEventsHandler.NewDocumentLoaded += NewDocumentLoadedHandler;
        }

        private static string GetCacheDirectory()
        {
            return HttpRuntime.AppDomainAppPath + "\\Cache";
        }

        private static void NewDocumentLoadedHandler(object sender, NewDocumentLoadedEventArgs e)
        {
            e.docuVieware.PagePreload = e.docuVieware.PageCount <= 50 ? PagePreloadMode.AllPages : PagePreloadMode.AdjacentPages;
        }
    }
}
