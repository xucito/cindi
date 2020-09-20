import { CindiClientService } from "./../../services/cindi-client.service";
import { Component, OnInit, OnDestroy } from "@angular/core";
import { interval, Subscription, forkJoin, Observable } from "rxjs";
import { map } from 'rxjs/operators';

@Component({
  selector: "monitoring",
  templateUrl: "./monitoring.component.html",
  styleUrls: ["./monitoring.component.css"]
})
export class MonitoringComponent implements OnInit, OnDestroy {

  generatedGraphs = [
    {
      metricName: "databaseoperationlatencyms",
      aggs: [ "max" ],
      interval: 'S'
    },
    {
      metricName: "clusteroperationelapsedms",
      aggs: [ "max" ],
      interval: 'S'
    },
    {
      metricName: "schedulerlatency",
      aggs: [ "max" ],
      interval: 'S'
    },
    {
      metricName: "databasetotalsizebytes",
      aggs: [ "max" ],
      interval: 'S'
    },
    {
      metricName: "databaseoperationcount",
      aggs: [ "max" ],
      interval: 'S'
    }
  ]

  ngOnDestroy(): void {
    this.metricReload$.unsubscribe();
  }
  ngOnInit() {}
  multi: any[];
  view: any[] = [((window.innerWidth-500)/2), 300];

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
  metricReload$: Subscription;

  results: any[] = [];
  clusterMetrics;
  databaseMetrics;
  schedulerMetrics;
  pastTime;
  currentTime;
  offsetMinutes;

  constructor(private cindiClient: CindiClientService) {
    Object.assign(this, this.multi);
    this.offsetMinutes = '15';
    this.LoadPage();
    this.metricReload$ = interval(10000).subscribe(() => {
      this.LoadPage();
    });
  }

  toggleTimeframe($event)
  {
    this.offsetMinutes = $event;
    if($event > 15)
    {
      this.generatedGraphs.forEach(element => {
        element.interval = "M"
      });
    }
    else
    {
      this.generatedGraphs.forEach(element => {
        element.interval = "S"
      });
    }
    this.LoadPage();
  }

  LoadPage() {
    this.currentTime = new Date();
    this.pastTime = new Date(this.currentTime);
    this.pastTime.setMinutes(this.pastTime.getMinutes() - this.offsetMinutes);

    let tasks = [];
    this.generatedGraphs.forEach(element => {
      tasks.push(this.GetMetrics(element.metricName, element.aggs, element.interval));
    });

    forkJoin(tasks).subscribe(
      (results) => {
        let reload = [];
        results.forEach(element => {
          reload.push(element);
        });
        this.results = reload;
      }
    );
    /*this.GetClusterMetrics();
    this.GetDatabaseMetrics();
    this.GetSchedulerMetrics();*/
  }

  GetMetrics(metricName: string, aggs: any[], interval: string): Observable<any> {
    return this.cindiClient
      .GetMetrics(
        this.pastTime,
        this.currentTime,
        metricName,
        aggs,
        interval,
        true
      ).pipe(map(result => {
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

        let metricsCache = [];

        Object.keys(allMetrics).forEach(key => {
          metricsCache.push({
            name: key,
            series: allMetrics[key]
          });
        });

        return metricsCache;
      }));
  }
/*
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
      });
  }

  GetSchedulerMetrics() {
    this.cindiClient
      .GetMetrics(
        this.pastTime,
        this.currentTime,
        "schedulerlatency",
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

        this.schedulerMetrics = [];

        Object.keys(allMetrics).forEach(key => {
          this.schedulerMetrics.push({
            name: key,
            series: allMetrics[key]
          });
        });
      });
  }*/
}
