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
			DocuViewareLicensing.RegisterKEY("jq1SpUabTOSolnYFMqDgO8iQlsqB_d063wpU5yxV93UCE_VQvibXuVO_0i4XqPBoKpVjwdY3VaaDum8pGlwKDY-OMMktbck7L8tydtXbhr708akijiZapwuhEY4TB_L-TEcG7_uYMqL1aTYsNtqO7AO95G63wC0UsHTRwLlxKmVM3mR56Hq6o1hCi2PkUhbKwf5fj-VyqwK1SABGIhwUfij9Od4dDEts1KbyiBZRjWbru5pWJj9oSCyinvLK84LxQ2OpDwBMfKDKReNuZnT9srUbPN9PcXca7KMW0kXt3T3Qz48A-UZqT4xIlGcsY07djZ7YM6oiwgcrh1bErQFSr_ggFf36vLm7WCEydjFNPznevtG7VYkKd0PLSOVFj1Iw2tS4nr_8eQirHhxn7a00vIBu0nC_IbMjt6qzXxu70zFZpcCzZj-mUyHJRK-VC0Unr5ePgQBKsIdEc3R6KiNpy8Z3jLJTm4JBxFizN9HF1pVn7s9LRuchC3ecwS_8Z_3vbLcpli2-uNKdN3uFf1Gtw5QLi_jpSNwOLFOWbpIwDMWo5_U7M8U8Rh-Iibw0vSMvnX854NJf1T_3PGka0R6vVayDC4Db8RRn7l8xv3tzKQwlYz58phUHU8YFmrh_j1hNKZzc1k_o4uU-V03d280b6uWn14RHVpfCxGyuU4pcEYpiWahGNDS1dVpan5FZqijYYh1xJ0E8AeB1XuzvE3_fKpDiUx34tYiLcT7zUQiDZM7rV-a5msxOgHpj9wsKTZbe-jMepPFnSnmKpBH2Vgof0Xx_hr9UcW-7exjHQP6sx_p7nOVaFjVGrhBGNPijt6AqhRcEjBslBxKX1AGVHcV40xLBpkTB2z4xYT-dryaBFFgoi7y8rxCafMm6SW7FOc4pX7k7x4BXnVnkbWkpFhxbhF1e9zL58iSbcOC9ZTb5Us8eka8Xv4rfDv6BZXW_4WCSxe2J-7BuHi-zWxI1FXSmodaT2ZSXt4oQ_2bNFmoK9oRy7FlJwjVbH9JPiZyVQH0T"); //Unlocking DocuVieware. Please insert your demo or commercial license key here.
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