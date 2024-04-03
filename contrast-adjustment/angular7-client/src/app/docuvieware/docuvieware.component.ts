import {AfterViewInit, Component, OnInit} from '@angular/core';
import {DocuviewareService} from '../docuvieware.service';
import {DocuViewareRESTOutputResponse} from '../docuvieware-restoutput-response';

declare var DocuViewareAPI: any;

@Component({
  selector: 'app-docuvieware',
  templateUrl: './docuvieware.component.html',
  providers: [DocuviewareService]
})
export class DocuviewareComponent implements OnInit, AfterViewInit {

  constructor(private docuviewareService: DocuviewareService) {
  }

  htmlMarkup: any;

  private static insertInDOM(content: string): void {
    const fragment = document.createRange().createContextualFragment(content);
    document.getElementById('dvContainer').appendChild(fragment);
  }

  ngOnInit() {
    this.docuviewareService.getDocuViewareMarkup().subscribe(
      response => DocuviewareComponent.insertInDOM((response as DocuViewareRESTOutputResponse).HtmlContent),
      error => this.htmlMarkup = error as any
    );
  }

  ngAfterViewInit(): void {
    this.RegisterOnDocuViewareAPIReady();
  }

  private RegisterOnDocuViewareAPIReady() {
    if (typeof DocuViewareAPI !== 'undefined' && DocuViewareAPI.IsInitialized(this.docuviewareService.DOCUVIEWARE_CONTROL_ID)) {
      DocuViewareAPI.RegisterOnNewDocumentLoaded(this.docuviewareService.DOCUVIEWARE_CONTROL_ID, () => {
        DocuViewareAPI.PostCustomServerAction(this.docuviewareService.DOCUVIEWARE_CONTROL_ID, true, 'documentLoaded');
      });
    } else {
      setTimeout(() => {
        this.RegisterOnDocuViewareAPIReady();
      }, 10);
    }
  }
}
