import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DocuviewareService {

  public DOCUVIEWARE_CONTROL_ID = 'DocuVieware1';
  private DOCUVIEWARE_ENDPOINT_BASE_URL = 'http://localhost:62968/api/DocuViewareREST';
  private DOCUVIEWARE_GETMARKUP_ENDPOINT = 'GetDocuViewareControl';

  constructor(private httpClient: HttpClient) {
  }

  getDocuViewareMarkup(): Observable<object> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    // const docuViewareConfig = {
    //   SessionId: 'mySessionId', // Set to an arbitrary value, should be replaced by the session identifier from your session mechanism
    //   ControlId: this.DOCUVIEWARE_CONTROL_ID,
    //   AllowPrint: true,
    //   EnablePrintButton: true,
    //   AllowUpload: true,
    //   EnableFileUploadButton: true,
    //   CollapsedSnapIn: true,
    //   ShowAnnotationsSnapIn: true,
    //   EnableRotateButtons: true,
    //   EnableZoomButtons: true,
    //   EnablePageViewButtons: true,
    //   EnableMultipleThumbnailSelection: true,
    //   EnableMouseModeButtons: true,
    //   EnableFormFieldsEdition: true,
    //   EnableTwainAcquisitionButton: true
    // };
    const docuViewareConfig = {
      SessionId: 'mySessionId', // Set to an arbitrary value, should be replaced by the session identifier from your session mechanism
      ControlId: this.DOCUVIEWARE_CONTROL_ID,
      AllowPrint: true,
      EnablePrintButton: true,
      EnableLoadFromUriButton: false,
      AllowUpload: true,
      EnableFileUploadButton: true,
      CollapsedSnapIn: true,
      EnableRotateButtons: true,
      EnableZoomButtons: true,
      EnablePageViewButtons: true,
      EnableMultipleThumbnailSelection: false,
      EnableMouseModeButtons: true,
      EnableFormFieldsEdition: true,
      EnableTwainAcquisitionButton: true,
      EnableFullScreenButton: true,
      ShowAnnotationsSnapIn: true,
      ShowThumbnailsSnapIn: false,
      ShowBookmarksSnapIn:false,
      ShowAnnotationsCommentsSnapIn:false,
      TwainAcquisitionFormat: "tiff",
      Zoom: 1
    };

    return this.httpClient.post(
      `${this.DOCUVIEWARE_ENDPOINT_BASE_URL}/${this.DOCUVIEWARE_GETMARKUP_ENDPOINT}/`,
      JSON.stringify(docuViewareConfig),
      httpOptions);
  }
}
