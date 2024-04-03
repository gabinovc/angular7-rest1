import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import {HttpClientModule} from '@angular/common/http';

import {AppComponent} from './app.component';
import {DocuviewareComponent} from './docuvieware/docuvieware.component';
import {AdjustmentPanelComponent} from './adjustment-panel/adjustment-panel.component';

@NgModule({
  declarations: [
    AppComponent,
    DocuviewareComponent,
    AdjustmentPanelComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}
