import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SidenavComponent } from './sidenav/sidenav';

@Component({
  selector: 'app-mainpage',
  standalone: true,
  imports: [RouterModule, SidenavComponent],
  templateUrl: './mainpage.html',
  styleUrls: ['./mainpage.css']
})
export class Mainpage { }