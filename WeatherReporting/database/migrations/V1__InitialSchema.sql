create table WeatherReportingDB.PublishedWeatherReports
(
    ReportId     BIGINT UNSIGNED AUTO_INCREMENT not null primary key,
    City         varchar(400) not null,
    ReportedOn   datetime(6)  not null,
    TemperatureC float        null,
    TemperatureF float        null
);