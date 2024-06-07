using DocuViewareREST.Models;
using GdPicture14;
using GdPicture14.WEB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;

namespace DocuViewareREST
{
    public class WebApiApplication : HttpApplication
    {
        public static readonly int SessionTimeout = 1440;//Set to 20 minutes. Use -1 to handle DocuVieware session timeout through ASP.NET session mechanism.
        private const bool StickySession = true; //Set false to use DocuVieware on Servers Farm with non sticky sessions.
        private const DocuViewareSessionStateMode DocuviewareSessionStateMode = DocuViewareSessionStateMode.InProc; //Set DocuViewareSessionStateMode.File is StickySession = false.

        private static readonly GdPictureImaging GdPictureImaging = new GdPictureImaging();
        private static int _nativeOriginalImage = 0;
        private static readonly List<int> PreviewImageIds = new List<int>();

        private static string GetCacheDirectory()
        {
            return HttpRuntime.AppDomainAppPath + "\\Cache";
        }

        protected void Application_Start()
        {
			DocuViewareLicensing.RegisterKEY("JxfSGLnUd5L4w02eojBJHgL7ycX6lJjgsOYe4Ralhz8yyqNkOjbgNXt8ObuvdChToxT-A_DL32OWm8mcwaPsVB8CzSToZELjw1_fJmwVqAlh-WJJAKkJeTvRNDKp2oQvp75HPgNN8zT5ZOMeAczyL8BQ39BLc_ol0QvobD2VE9Ly_mYnbJSKCYQzVZMvsKTN2EEQAdIKeVJGch0U5mJgK_d31xE2vNx2L8LtFEFu1oLvL5hAu-MqGUavVbcCpCcJsXz56_OyANqqGurUvCZXamDxVhgeI_pC4RPn6w2Qb7VaDXYjuGXKV1K5IiEcEGxYRekeqCnyLRs36wYRatNeoOtyeYcwmYk0z0mVrlqNj-m23VHhboqrHoW1Yy6_lwhnhw4mxvY6Y0VzGcWX8lIcA8XM9qJcFOQroqeyW4nZgMGoRVeESaKIL2OD8kLIe_brIdBXQevgoHTlhBeIC4eU3bNszMrXikXXDpWdcGgJYky37e6LP2xzmm6WHMwpBMfELPpfsivBma2DiNDkbwH0jcqmxATibrAS0psjXMYM03XC5lo7sd65tR1uHF14Lmh2ZczMKWQH_2mbX1CN_XbUfbTifuvelTJ78u2xUW-7CMeeLKBg6ZhrL4Hn5efmqKfIR8GHPXgo79YEv1BQQJVR1rs8p7-trCSb_72Qg7w6Jyesn6tM4PvdtSFFoGxQK7e7qzYzicJKh3yHPTEXsWf7XuZHL_fv2ZVQkeeCKQ1gLX75XdlnCCAsRF0d-f1JQtcZY3f-sZ3AJwYICCGmLCmefeG4lrsBQOqRusjvGtJrjbNC9JvhX3N4_qb6Y89WhblanQyfM8aa6NZ1vj7FRu8RQKzGztM3YskmkhGtv-rTfolkChtvM52RYDIdmNL0L7mHyfJAWg8WQkc8dSO3aB1eSfq4fQKsgQe87zhIgGp4sBbpUxUrebcaphfBnUFg0A1rvZOMhUQ9saG-T2fvVLFfju9_ji0CBzEEbdLovmB7_VhZTiJ_Xaot-z6OEX1khbXfOH5i74vS_fD2A844dhpgkd7d2JosRlv4eap6ikUTJXwSJRkOQzm4MGb1L1PkwOid"); //Unlocking DocuVieware. Please insert your demo or commercial license key here.
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DocuViewareManager.SetupConfiguration(StickySession, DocuviewareSessionStateMode, GetCacheDirectory());
            
            DocuViewareEventsHandler.CustomAction += CustomActionHandler;
        }

        private static void CustomActionHandler(object sender, CustomActionEventArgs e)
        {
            if (e.docuVieware.PageCount > 0)
            {
                if (e.docuVieware.GetDocumentType() == DocumentType.DocumentTypeBitmap)
                {
                    GdPictureStatus status;
                    switch (e.actionName)
                    {
                        case "documentLoaded":
                            {
                                status = e.docuVieware.GetNativeImage(out int imageId);
                                if (status == GdPictureStatus.OK)
                                {
                                    MemoryStream nativeImageStream = new MemoryStream();
                                    status = GdPictureImaging.SaveAsStream(imageId, nativeImageStream, GdPicture14.DocumentFormat.DocumentFormatTIFF, 65536);
                                    if (status == GdPictureStatus.OK)
                                    {
                                        _nativeOriginalImage = GdPictureImaging.CreateGdPictureImageFromStream(nativeImageStream);
                                    }
                                    else
                                    {
                                        e.message = new DocuViewareMessage("Error cloning pages : " + status + ".", icon: DocuViewareMessageIcon.Error);
                                    }
                                }
                                else
                                {
                                    e.message = new DocuViewareMessage("Error getting native image : " + status + ".", icon: DocuViewareMessageIcon.Error);
                                }
                            }
                            return;
                        case "closeDocument":
                            if (_nativeOriginalImage != 0)
                            {
                                GdPictureImaging.ReleaseGdPictureImage(_nativeOriginalImage);
                                _nativeOriginalImage = 0;
                                foreach (int previewImageId in PreviewImageIds)
                                {
                                    GdPictureImaging.ReleaseGdPictureImage(previewImageId);
                                }
                                PreviewImageIds.Clear();
                            }
                            return;
                        case "adjustContrast":
                            if (_nativeOriginalImage != 0)
                            {
                                MemoryStream nativeImageStream = new MemoryStream();
                                status = GdPictureImaging.SaveAsStream(_nativeOriginalImage, nativeImageStream, GdPicture14.DocumentFormat.DocumentFormatTIFF, 65536);
                                if (status == GdPictureStatus.OK)
                                {
                                    int previewImageId = GdPictureImaging.CreateGdPictureImageFromStream(nativeImageStream);
                                    if (previewImageId != 0)
                                    {
                                        PreviewImageIds.Add(previewImageId);
                                        status = GdPictureStatus.GenericError;
                                        AdjustmentActionParameters cleanupParameters = JsonConvert.DeserializeObject<AdjustmentActionParameters>(e.args.ToString());
                                        if (cleanupParameters.RegionOfInterest != null && cleanupParameters.RegionOfInterest.Width > 0 && cleanupParameters.RegionOfInterest.Height > 0)
                                        {
                                            GdPictureImaging.SetROI((int)Math.Round(cleanupParameters.RegionOfInterest.Left * GdPictureImaging.GetHorizontalResolution(previewImageId), 0),
                                                (int)Math.Round(cleanupParameters.RegionOfInterest.Top * GdPictureImaging.GetVerticalResolution(previewImageId), 0),
                                                (int)Math.Round(cleanupParameters.RegionOfInterest.Width * GdPictureImaging.GetHorizontalResolution(previewImageId), 0),
                                                (int)Math.Round(cleanupParameters.RegionOfInterest.Height * GdPictureImaging.GetVerticalResolution(previewImageId), 0));
                                        }
                                        foreach (int page in cleanupParameters.Pages)
                                        {
                                            status = GdPictureImaging.SelectPage(previewImageId, page);
                                            if (status == GdPictureStatus.OK)
                                            {
                                                status = GdPictureImaging.SetContrast(previewImageId, cleanupParameters.ContrastValue);
                                                if (status != GdPictureStatus.OK)
                                                {
                                                    e.message = new DocuViewareMessage("Error during contrast adjustment: " + status + " on page " + page, icon: DocuViewareMessageIcon.Error);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                e.message = new DocuViewareMessage("Error during page selection: " + status + ".", icon: DocuViewareMessageIcon.Error);
                                                break;
                                            }
                                        }
                                        if (status == GdPictureStatus.OK)
                                        {
                                            status = e.docuVieware.LoadFromGdPictureImage(previewImageId);
                                            if (status != GdPictureStatus.OK)
                                            {
                                                e.message = new DocuViewareMessage("Error during redraw pages : " + status + ".", icon: DocuViewareMessageIcon.Error);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    e.message = new DocuViewareMessage("Error cloning pages : " + status + ".", icon: DocuViewareMessageIcon.Error);
                                }
                            }
                            break;
                    }
                }
                else
                {
                    e.message = new DocuViewareMessage("Only raster formats are supported!", icon: DocuViewareMessageIcon.Error);
                }
            }
            else
            {
                e.message = new DocuViewareMessage("Please load an image document first!", icon: DocuViewareMessageIcon.Error);
            }
        }
    }
}