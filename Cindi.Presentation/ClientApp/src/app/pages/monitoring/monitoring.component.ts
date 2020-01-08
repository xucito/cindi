import { CindiClientService } from "./../../services/cindi-client.service";
import { Component, OnInit } from "@angular/core";

@Component({
  selector: "monitoring",
  templateUrl: "./monitoring.component.html",
  styleUrls: ["./monitoring.component.css"]
})
export class MonitoringComponent implements OnInit {
  ngOnInit() {}
  multi: any[];
  view: any[] = [500, 300];

  // options
  legend: boolean = true;
  showLabels: boolean = true;
  animations: boolean = true;
  xAxis: boolean = true;
  yAxis: boolean = true;
  showYAxisLabel: boolean = true;
  showXAxisLabel: boolean = true;
  xAxisLabel: string = "Year";
  yAxisLabel: string = "Population";
  timeline: boolean = true;

  colorScheme = {
    domain: ["#5AA454", "#E44D25", "#CFC0BB", "#7aa3e5", "#a8385d", "#aae3f5"]
  };
  metricReload;

  clusterMetrics;
  databaseMetrics;
  pastTime;
  currentTime;

  constructor(private cindiClient: CindiClientService) {
    Object.assign(this, this.multi);

    this.metricReload = setInterval(() => {
      this.currentTime = new Date();
      this.pastTime = new Date(this.currentTime);
      this.pastTime.setMinutes(this.pastTime.getMinutes() - 10);
      this.GetClusterMetrics();
      this.GetDatabaseMetrics();
      console.log("loaded");
    }, 5000);
  }

  GetDatabaseMetrics() {
    this.cindiClient
      .GetMetrics(
        this.pastTime,
        this.currentTime,
        "databaseoperationlatencyms",
        ["max"],
        "S",
        true
      )
      .subscribe(result => {
        var metrics = result.result;
        let allMetrics = {};
        metrics.forEach(metric => {
          if (!allMetrics.hasOwnProperty(metric._id.subcategory)) {
            allMetrics[metric._id.subcategory] = [];
          }
          allMetrics[metric._id.subcategory].push({
            name: new Date(metric._id.date),
            value: metric.max
          });
        });

        this.databaseMetrics = [];

        Object.keys(allMetrics).forEach(key => {
          this.databaseMetrics.push({
            name: key,
            series: allMetrics[key]
          });
        });
      });
  }

  GetClusterMetrics() {
    this.cindiClient
      .GetMetrics(
        this.pastTime,
        this.currentTime,
        "clusteroperationelapsedms",
        ["max"],
        "S",
        true
      )
      .subscribe(result => {
        var metrics = result.result;
        let allMetrics = {};
        metrics.forEach(metric => {
          if (!allMetrics.hasOwnProperty(metric._id.subcategory)) {
            allMetrics[metric._id.subcategory] = [];
          }
          allMetrics[metric._id.subcategory].push({
            name: new Date(metric._id.date),
            value: metric.max
          });
        });

        this.clusterMetrics = [];

        Object.keys(allMetrics).forEach(key => {
          this.clusterMetrics.push({
            name: key,
            series: allMetrics[key]
          });
        });

        console.log(result);
      });
  }
}
