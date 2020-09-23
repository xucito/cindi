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

  interval = 'M';
  generatedGraphs = [
    /*{
      metricName: "databaseoperationlatencyms",
      aggs: ["max"]
    },*/
    {
      metricName: "clusteroperationelapsedms",
      aggs: ["max"]
    },
    {
      metricName: "schedulerlatency",
      aggs: ["max"]
    },
  /* {
      metricName: "databasetotalsizebytes",
      aggs: ["max"]
    },
    {
      metricName: "databaseoperationcount",
      aggs: ["max"]
    }*/
  ]

  ngOnDestroy(): void {
    this.metricReload$.unsubscribe();
  }
  ngOnInit() { }
  multi: any[];
  view: any[] = [((window.innerWidth - 500) / 2), 300];

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

  toggleTimeframe($event) {
    this.offsetMinutes = $event;
    if ($event > 15) {
      this.interval = "M"
    }
    else {
      this.interval = "M"
    }
    this.LoadPage();
  }

  LoadPage() {
    this.currentTime = new Date();
    this.pastTime = new Date(this.currentTime);
    this.pastTime.setMinutes(this.pastTime.getMinutes() - this.offsetMinutes);

    let tasks = [];
    /*this.generatedGraphs.forEach(element => {
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
    );*/

    var metrics = {};

    this.generatedGraphs.forEach(element => {
      metrics[element.metricName] = element.aggs
    });

    this.GetMetrics(metrics, this.interval).subscribe(result => {
      this.results = result;
    });
    /*this.GetClusterMetrics();
    this.GetDatabaseMetrics();
    this.GetSchedulerMetrics();*/
  }

  GetMetrics(metrics: any, interval: string): Observable<any> {
    return this.cindiClient
      .GetMetrics(
        this.pastTime,
        this.currentTime,
        metrics,
        interval,
        true
      ).pipe(map(result => {
        var metrics = result.result;
        let allMetrics = {};
        Object.keys(metrics).forEach(key => {
          var metric = key;
          if (Object.keys(metrics[key]).length > 0) {
            allMetrics[metric] = {};
            allMetrics[metric]['max'] = [];
            allMetrics[metric]['min'] = [];
            allMetrics[metric]['avg'] = [];
            Object.keys(metrics[key]).forEach(date => {
              allMetrics[metric]['max'].push({
                name: new Date(date),
                value: metrics[key][date].max
              });
              allMetrics[metric]['min'].push({
                name: new Date(date),
                value: metrics[key][date].min
              });
              allMetrics[metric]['avg'].push({
                name: new Date(date),
                value: metrics[key][date].avg
              });
            });
          }
        });
        /*metrics.forEach(metric => {
          if (!allMetrics.hasOwnProperty(metric._id.subcategory)) {
            allMetrics[metric._id.subcategory] = [];
          }
          allMetrics[metric._id.subcategory].push({
            name: new Date(metric._id.date),
            value: metric.max
          });
        });*/
        let metricsCache = {};

        Object.keys(allMetrics).forEach(key => {
          metricsCache[key] = [];
          Object.keys(allMetrics[key]).forEach(aggs => {
            metricsCache[key].push({
              name: aggs,
              series: allMetrics[key][aggs]
            });
          })
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
