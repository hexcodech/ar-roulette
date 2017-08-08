import {Component, ElementRef, ViewChild} from '@angular/core';

@Component({
  selector: 'page-home',
  templateUrl: 'home.html'
})
export class HomePage {

  private showRouletteIntro: boolean = true;
  private showRouletteIntroLoop: boolean = false;
  private unlockCode: string = '1234';
  private passCode: string = '';

  constructor() {
  }

  private rouletteLoopIntroEnded()  {
    this.showRouletteIntroLoop = true;
    setTimeout(() => {
      this.showRouletteIntro = false;
    }, 250);
  }

  private add(value: number)  {
    if(this.passCode.length < 4)  {
      this.passCode += value;
    }
  }

  private delete()  {
    if(this.passCode.length>0)  {
      this.passCode = this.passCode.substring(0, this.passCode.length - 1);
    }
  }

  private clear() {
    this.passCode = '';
  }

}
