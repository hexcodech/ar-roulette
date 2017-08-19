import {Component} from '@angular/core';
import {Http} from '@angular/http';
import {Observable} from "rxjs/Rx";

@Component({
  selector: 'page-home',
  templateUrl: 'home.html'
})
export class HomePage {

  private showRouletteIntro: boolean = true;
  private showRouletteIntroLoop: boolean = false;
  private fadeInOverlay: boolean = false;
  private unlocked: boolean = false;

  private blueSelected: number = 0;
  private blueValues: string[] = ['485678', '443658', '201524', '201542', '834542', '654565', '425102'];
  private yellowSelected: number = 0;
  private yellowValues: string[] = ['42497', '22497', '14987', '44834', '43214', '42442', '42424'];
  private redSelected: number = 0;
  private redValues: string[] = ['707', '777', '107', '207', '307', '007', '907'];
  private greenSelected: number = 0;
  private greenValues: string[] = ['1989', '1953', '1891', '1591', '7913', '2519', '6232'];
  private whiteSelected: number = 0;
  private whiteValues: string[] = ['1989', '1953', '1891', '1591', '2591', '2519', '6232'];

  private solved: boolean = false;
  private failed: boolean = false;

  constructor(private http: Http) {
    let observable = Observable.interval(100)
      .switchMap(() => http.get('http://localhost:8080/fingerprint')).map((data) => data.json())
      .subscribe((data) => {
        if(data === true) {
          this.unlocked = true;

          // Don't unsubscribe to prevent multiple scans to skip next time
          // observable.unsubscribe();
        }
      });

    setTimeout(() => {
      this.fadeInOverlay = true;
    }, 3500);
  }

  private rouletteLoopIntroEnded()  {
    this.showRouletteIntroLoop = true;
    setTimeout(() => {
      this.showRouletteIntro = false;
    }, 250);
  }

  private increase(color: string) {
    switch (color) {
      case 'blue':
        this.blueSelected++;
        if(this.blueSelected >= this.blueValues.length)
          this.blueSelected = 0;
        break;
      case 'yellow':
        this.yellowSelected++;
        if(this.yellowSelected >= this.yellowValues.length)
          this.yellowSelected = 0;
        break;
      case 'red':
        this.redSelected++;
        if(this.redSelected >= this.redValues.length)
          this.redSelected = 0;
        break;
      case 'green':
        this.greenSelected++;
        if(this.greenSelected >= this.greenValues.length)
          this.greenSelected = 0;
        break;
      case 'white':
        this.whiteSelected++;
        if(this.whiteSelected >= this.whiteValues.length)
          this.whiteSelected = 0;
        break;
    }
  }

  private decrease(color: string) {
    switch (color) {
      case 'blue':
        this.blueSelected--;
        if(this.blueSelected < 0)
          this.blueSelected = this.blueValues.length - 1;
        break;
      case 'yellow':
        this.yellowSelected--;
        if(this.yellowSelected < 0)
          this.yellowSelected = this.yellowValues.length - 1;
        break;
      case 'red':
        this.redSelected--;
        if(this.redSelected < 0)
          this.redSelected = this.redValues.length - 1;
        break;
      case 'green':
        this.greenSelected--;
        if(this.greenSelected < 0)
          this.greenSelected = this.greenValues.length - 1;
        break;
      case 'white':
        this.whiteSelected--;
        if(this.whiteSelected < 0)
          this.whiteSelected = this.whiteValues.length - 1;
        break;
    }

  }

  private validate() {
    if(this.redValues[this.redSelected] === '007' && this.blueValues[this.blueSelected] === '201524' && this.yellowValues[this.yellowSelected] === '42424' && this.whiteValues[this.whiteSelected] === '1953' && this.greenValues[this.greenSelected] === '1953') {
      this.solved = true;
    }
    else {
      this.failed = true;
    }
  }

  private failedEnded() {
    location.reload();
  }
  private solvedEnded() {
    location.reload();
  }

}
