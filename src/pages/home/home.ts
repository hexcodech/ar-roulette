import {Component, ElementRef, ViewChild} from '@angular/core';

@Component({
  selector: 'page-home',
  templateUrl: 'home.html'
})
export class HomePage {

  private showRouletteIntro: boolean = true;
  private showRouletteIntroLoop: boolean = false;
  //@ViewChild('RouletteLoop') elRouletteIntroLoop: ElementRef;

  constructor() {

  }

  private rouletteLoopIntroEnded()  {
    this.showRouletteIntroLoop = true;
    setTimeout(() => {
      this.showRouletteIntro = false;
    }, 250);
  }

}
