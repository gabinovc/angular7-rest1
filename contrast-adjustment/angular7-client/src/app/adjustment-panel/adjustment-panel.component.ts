import {Component, OnInit} from '@angular/core';
import {DocuviewareService} from '../docuvieware.service';

declare var DocuViewareAPI: any;

@Component({
  selector: 'app-adjustment-panel',
  templateUrl: './adjustment-panel.component.html',
  styleUrls: ['./adjustment-panel.component.css']
})
export class AdjustmentPanelComponent implements OnInit {

  constructor(private docuviewareService: DocuviewareService) {
  }

  ngOnInit() {
  }

  adjustContrast(value: string) {
    const pages = DocuViewareAPI.GetSelectedThumbnailItems(this.docuviewareService.DOCUVIEWARE_CONTROL_ID);
    if (pages.length === 0) {
      pages[0] = DocuViewareAPI.GetCurrentPage(this.docuviewareService.DOCUVIEWARE_CONTROL_ID);
    }
    const roi = DocuViewareAPI.GetSelectionAreaCoordinates(this.docuviewareService.DOCUVIEWARE_CONTROL_ID);

    DocuViewareAPI.PostCustomServerAction(this.docuviewareService.DOCUVIEWARE_CONTROL_ID, false, 'adjustContrast', {
      Pages: pages,
      RegionOfInterest: roi,
      ContrastValue: Number(value)
    });
  }

  closeDocument() {

    DocuViewareAPI.PostCustomServerAction(this.docuviewareService.DOCUVIEWARE_CONTROL_ID, true, 'closeDocument', '', () => {
      DocuViewareAPI.CloseDocument(this.docuviewareService.DOCUVIEWARE_CONTROL_ID);
    });
  }

  getSourcesScanDevices(): any{
    debugger
    let sourceList = DocuViewareAPI.TwainGetSources(this.docuviewareService.DOCUVIEWARE_CONTROL_ID);
    if (sourceList != null) {
      return sourceList.sources;      
    } else {
      console.log("No available source have been found.");
    }
  }
}
