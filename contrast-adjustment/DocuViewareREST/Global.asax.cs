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
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DocuViewareManager.SetupConfiguration(StickySession, DocuviewareSessionStateMode, GetCacheDirectory());
            DocuViewareLicensing.RegisterKEY("zbfGVe-OHgdRc2noaOBNryK7VQd--xwF1kV_XwIIuo40He2evavdRg8okGatk8Wz27TAs2j4tqSQYEaITOL19vv4pQ16h6tWaYZCPj_5WTCv_umbVX5goz7mAF1UB10Co8yvPJ3PUGF7uWrcMynAzA273h6JGaK0QzXy7pRz0dPjv0iQCt5PvC6PMhwDLMU7GRJRJyM-ixxlzLsyYxQzzyInMUO28aRqYlV7jnf-NZd8J015qT8WPgrR1mPBLgZzR-8N0u-BY20-IF8r2YksWz9b7SSjNHO5bbHwJH-Jyqh8CmjM-dBse9lLFPPgoc8vYvsZfSGcGR5vJ_egmBGBYAyv59eu4QpvFqorsg8KqvxleiOHrsQCAfHi5Z6SNKqY8KfVucWdlOPfMvgk6cntvDacm5hji9WQ6ZvdDupGWtZtSWfEo80Lc1QPrJzsG3JuvCPzwyPYTmgimtBdBeUL9s7qSqTithtnww9kr3xVaWZGtuGbCOkrdFLKet5xbH2riOQnqtXv50IidN_xhWYfNjHCl38eUpvDThypp0w42M_l3dsNuIIHX2qtGAGTao0PWDcLN1txiabaQniXKe5oO8KmE3rx4aXknH-3xDS1lfqjmbg5lhc5oa9mnVMXGKahugIKhhpLC7CFcCkXJ62k_rsDapv0R_GgU3ZCKXLp8s6bcx2D2UJa8i1Kz6vxHFbXdhsXCizuD43_xDtCK7kPzRQjvy11JffT0SFe0CtpI9qc1LKVViAgH0sUv4s6gA7IU0mm2rukmXllThDvH6m4utU7iGAhp9SkyQ8nzN2ZzIGIvdZAYHFJz_KjFvsm7ZIjm0_aaIJfu_cq1y6K1_3SNkGenKUAGICa8MRN62JH2VapQFYOQ2Qnhopo7GCnpYLdxXkYlj4T7SXhc6THRo2fNBZcV_cVEZNpsZ_wGp7iooNydcm1VmInUzwvqsuCHvuQ18foObpslna3_pGatmR_MY70M8ZnGyGhOa3SmQCj5H_VHvttS2XTVCyFHsJUkgQd"); //Unlocking DocuVieware. Please insert your demo or commercial license key here.
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