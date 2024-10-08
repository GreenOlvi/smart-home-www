﻿using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using SmartHomeWWW.Core.Domain.Repositories;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Events;
using SmartHomeWWW.Server.Repositories;
using SmartHomeWWW.Server.TelegramBotModule;
using SmartHomeWWW.Server.Weather;

namespace SmartHomeWWW.Server.Tests.Weather;

[TestFixture]
public class WeatherAdapterJobTests
{
    private ServiceProvider _sp = null!;

    [SetUp]
    public void Setup()
    {
        var sc = new ServiceCollection();

        sc.AddScoped<IMessageBus, BasicMessageBus>();
        sc.AddSingleton(sp => Substitute.For<IHubConnection>());
        sc.AddSingleton(sp => Substitute.For<IKeyValueStore>());
        sc.AddSingleton<TelegramConfig>();
        sc.AddLogging(o =>
        {
            o.ClearProviders();
        });

        sc.AddScoped(sp => CreateInMemory());
        sc.AddScoped<IWeatherReportRepository, WeatherReportRepository>();
        sc.AddTransient<WeatherAdapterJob>();

        _sp = sc.BuildServiceProvider();
    }

    [Test]
    public async Task WeatherReportIsStoredInCacheTest()
    {
        var repo = _sp.GetRequiredService<IWeatherReportRepository>();
        (await repo.GetCurrentWeatherReport()).Should().BeNull();

        var job = _sp.GetRequiredService<WeatherAdapterJob>();
        await job.Handle(new MqttMessageReceivedEvent
        {
            Topic = "env/out/weather",
            Payload = WeatherExample,
        });

        var report = await repo.GetCurrentWeatherReport();
        report.Should().NotBeNull();
        report.Value.Current.Timestamp.Should().Be(new DateTime(2023, 6, 11, 8, 22, 57, DateTimeKind.Utc));
    }

    private const string WeatherExample = """{"lat":12.34,"lon":56.78,"timezone":"Europe/Warsaw","timezone_offset":7200,"current":{"dt":1686471777,"sunrise":1686450628,"sunset":1686510804,"temp":22.12,"feels_like":21.67,"pressure":1022,"humidity":49,"dew_point":10.92,"uvi":3.8,"clouds":0,"visibility":10000,"wind_speed":3.09,"wind_deg":120,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}]},"minutely":[{"dt":1686471780,"precipitation":0},{"dt":1686471840,"precipitation":0},{"dt":1686471900,"precipitation":0},{"dt":1686471960,"precipitation":0},{"dt":1686472020,"precipitation":0},{"dt":1686472080,"precipitation":0},{"dt":1686472140,"precipitation":0},{"dt":1686472200,"precipitation":0},{"dt":1686472260,"precipitation":0},{"dt":1686472320,"precipitation":0},{"dt":1686472380,"precipitation":0},{"dt":1686472440,"precipitation":0},{"dt":1686472500,"precipitation":0},{"dt":1686472560,"precipitation":0},{"dt":1686472620,"precipitation":0},{"dt":1686472680,"precipitation":0},{"dt":1686472740,"precipitation":0},{"dt":1686472800,"precipitation":0},{"dt":1686472860,"precipitation":0},{"dt":1686472920,"precipitation":0},{"dt":1686472980,"precipitation":0},{"dt":1686473040,"precipitation":0},{"dt":1686473100,"precipitation":0},{"dt":1686473160,"precipitation":0},{"dt":1686473220,"precipitation":0},{"dt":1686473280,"precipitation":0},{"dt":1686473340,"precipitation":0},{"dt":1686473400,"precipitation":0},{"dt":1686473460,"precipitation":0},{"dt":1686473520,"precipitation":0},{"dt":1686473580,"precipitation":0},{"dt":1686473640,"precipitation":0},{"dt":1686473700,"precipitation":0},{"dt":1686473760,"precipitation":0},{"dt":1686473820,"precipitation":0},{"dt":1686473880,"precipitation":0},{"dt":1686473940,"precipitation":0},{"dt":1686474000,"precipitation":0},{"dt":1686474060,"precipitation":0},{"dt":1686474120,"precipitation":0},{"dt":1686474180,"precipitation":0},{"dt":1686474240,"precipitation":0},{"dt":1686474300,"precipitation":0},{"dt":1686474360,"precipitation":0},{"dt":1686474420,"precipitation":0},{"dt":1686474480,"precipitation":0},{"dt":1686474540,"precipitation":0},{"dt":1686474600,"precipitation":0},{"dt":1686474660,"precipitation":0},{"dt":1686474720,"precipitation":0},{"dt":1686474780,"precipitation":0},{"dt":1686474840,"precipitation":0},{"dt":1686474900,"precipitation":0},{"dt":1686474960,"precipitation":0},{"dt":1686475020,"precipitation":0},{"dt":1686475080,"precipitation":0},{"dt":1686475140,"precipitation":0},{"dt":1686475200,"precipitation":0},{"dt":1686475260,"precipitation":0},{"dt":1686475320,"precipitation":0},{"dt":1686475380,"precipitation":0}],"hourly":[{"dt":1686470400,"temp":22.12,"feels_like":21.67,"pressure":1022,"humidity":49,"dew_point":10.92,"uvi":3.8,"clouds":0,"visibility":10000,"wind_speed":5.14,"wind_deg":86,"wind_gust":6.88,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686474000,"temp":22.05,"feels_like":21.56,"pressure":1022,"humidity":48,"dew_point":10.54,"uvi":5.03,"clouds":0,"visibility":10000,"wind_speed":5.88,"wind_deg":86,"wind_gust":7.07,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686477600,"temp":22.28,"feels_like":21.74,"pressure":1022,"humidity":45,"dew_point":9.79,"uvi":5.86,"clouds":0,"visibility":10000,"wind_speed":5.98,"wind_deg":86,"wind_gust":6.81,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686481200,"temp":22.62,"feels_like":22.03,"pressure":1022,"humidity":42,"dew_point":9.07,"uvi":6.13,"clouds":1,"visibility":10000,"wind_speed":5.93,"wind_deg":85,"wind_gust":6.43,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686484800,"temp":23.06,"feels_like":22.41,"pressure":1021,"humidity":38,"dew_point":7.99,"uvi":5.73,"clouds":1,"visibility":10000,"wind_speed":5.84,"wind_deg":83,"wind_gust":6.42,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686488400,"temp":23.43,"feels_like":22.72,"pressure":1021,"humidity":34,"dew_point":6.66,"uvi":4.78,"clouds":5,"visibility":10000,"wind_speed":5.75,"wind_deg":80,"wind_gust":6.25,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686492000,"temp":23.33,"feels_like":22.61,"pressure":1021,"humidity":34,"dew_point":6.45,"uvi":3.51,"clouds":4,"visibility":10000,"wind_speed":5.58,"wind_deg":79,"wind_gust":5.99,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686495600,"temp":23.02,"feels_like":22.29,"pressure":1021,"humidity":35,"dew_point":6.65,"uvi":2.24,"clouds":4,"visibility":10000,"wind_speed":5.6,"wind_deg":79,"wind_gust":5.94,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686499200,"temp":22.44,"feels_like":21.73,"pressure":1021,"humidity":38,"dew_point":7.52,"uvi":1.22,"clouds":3,"visibility":10000,"wind_speed":5.29,"wind_deg":78,"wind_gust":5.68,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686502800,"temp":21.24,"feels_like":20.52,"pressure":1021,"humidity":42,"dew_point":7.6,"uvi":0.53,"clouds":3,"visibility":10000,"wind_speed":4.92,"wind_deg":76,"wind_gust":7.19,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686506400,"temp":19.1,"feels_like":18.35,"pressure":1021,"humidity":49,"dew_point":8.02,"uvi":0.17,"clouds":3,"visibility":10000,"wind_speed":4.04,"wind_deg":69,"wind_gust":7.87,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686510000,"temp":16.74,"feels_like":15.85,"pressure":1022,"humidity":53,"dew_point":7.21,"uvi":0,"clouds":1,"visibility":10000,"wind_speed":4.18,"wind_deg":68,"wind_gust":10.26,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686513600,"temp":15.75,"feels_like":14.82,"pressure":1022,"humidity":55,"dew_point":6.56,"uvi":0,"clouds":2,"visibility":10000,"wind_speed":4.9,"wind_deg":72,"wind_gust":12.37,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01n"}],"pop":0},{"dt":1686517200,"temp":14.93,"feels_like":14.02,"pressure":1022,"humidity":59,"dew_point":6.99,"uvi":0,"clouds":2,"visibility":10000,"wind_speed":4.96,"wind_deg":83,"wind_gust":11.81,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01n"}],"pop":0},{"dt":1686520800,"temp":14.02,"feels_like":13.23,"pressure":1023,"humidity":67,"dew_point":7.97,"uvi":0,"clouds":2,"visibility":10000,"wind_speed":4.25,"wind_deg":92,"wind_gust":10.58,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01n"}],"pop":0},{"dt":1686524400,"temp":13.41,"feels_like":12.69,"pressure":1023,"humidity":72,"dew_point":8.32,"uvi":0,"clouds":2,"visibility":10000,"wind_speed":3.99,"wind_deg":95,"wind_gust":9.55,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01n"}],"pop":0},{"dt":1686528000,"temp":12.57,"feels_like":11.87,"pressure":1022,"humidity":76,"dew_point":8.35,"uvi":0,"clouds":2,"visibility":10000,"wind_speed":3.78,"wind_deg":97,"wind_gust":8.7,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01n"}],"pop":0},{"dt":1686531600,"temp":11.88,"feels_like":11.16,"pressure":1023,"humidity":78,"dew_point":8.22,"uvi":0,"clouds":1,"visibility":10000,"wind_speed":3.69,"wind_deg":96,"wind_gust":8.06,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01n"}],"pop":0},{"dt":1686535200,"temp":11.19,"feels_like":10.48,"pressure":1023,"humidity":81,"dew_point":7.95,"uvi":0,"clouds":2,"visibility":10000,"wind_speed":3.55,"wind_deg":95,"wind_gust":7.89,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01n"}],"pop":0},{"dt":1686538800,"temp":10.6,"feels_like":9.78,"pressure":1023,"humidity":79,"dew_point":7.11,"uvi":0,"clouds":3,"visibility":10000,"wind_speed":3.51,"wind_deg":87,"wind_gust":7.26,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686542400,"temp":11.49,"feels_like":10.57,"pressure":1023,"humidity":72,"dew_point":6.64,"uvi":0.22,"clouds":5,"visibility":10000,"wind_speed":3.53,"wind_deg":79,"wind_gust":8.19,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686546000,"temp":13.79,"feels_like":12.77,"pressure":1023,"humidity":59,"dew_point":5.98,"uvi":0.64,"clouds":5,"visibility":10000,"wind_speed":4.4,"wind_deg":84,"wind_gust":7.96,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686549600,"temp":15.88,"feels_like":14.88,"pressure":1023,"humidity":52,"dew_point":6.06,"uvi":1.42,"clouds":5,"visibility":10000,"wind_speed":4.92,"wind_deg":85,"wind_gust":7.76,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686553200,"temp":17.58,"feels_like":16.59,"pressure":1023,"humidity":46,"dew_point":5.65,"uvi":2.52,"clouds":0,"visibility":10000,"wind_speed":5.55,"wind_deg":85,"wind_gust":7.81,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686556800,"temp":18.81,"feels_like":17.74,"pressure":1023,"humidity":38,"dew_point":4.1,"uvi":3.83,"clouds":0,"visibility":10000,"wind_speed":6.17,"wind_deg":81,"wind_gust":7.86,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686560400,"temp":19.69,"feels_like":18.58,"pressure":1023,"humidity":33,"dew_point":3.02,"uvi":5.07,"clouds":0,"visibility":10000,"wind_speed":6.46,"wind_deg":80,"wind_gust":7.9,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686564000,"temp":20.37,"feels_like":19.27,"pressure":1022,"humidity":31,"dew_point":2.96,"uvi":5.85,"clouds":1,"visibility":10000,"wind_speed":6.49,"wind_deg":82,"wind_gust":7.71,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686567600,"temp":20.98,"feels_like":19.94,"pressure":1022,"humidity":31,"dew_point":3.03,"uvi":6.12,"clouds":2,"visibility":10000,"wind_speed":6.31,"wind_deg":83,"wind_gust":7.35,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686571200,"temp":21.38,"feels_like":20.38,"pressure":1022,"humidity":31,"dew_point":3.4,"uvi":5.72,"clouds":3,"visibility":10000,"wind_speed":6.2,"wind_deg":83,"wind_gust":7.11,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686574800,"temp":21.64,"feels_like":20.67,"pressure":1021,"humidity":31,"dew_point":3.88,"uvi":4.76,"clouds":5,"visibility":10000,"wind_speed":5.9,"wind_deg":82,"wind_gust":6.87,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686578400,"temp":21.68,"feels_like":20.74,"pressure":1021,"humidity":32,"dew_point":4.02,"uvi":3.5,"clouds":5,"visibility":10000,"wind_speed":5.55,"wind_deg":82,"wind_gust":6.56,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686582000,"temp":21.49,"feels_like":20.53,"pressure":1020,"humidity":32,"dew_point":4.01,"uvi":2.23,"clouds":6,"visibility":10000,"wind_speed":5.34,"wind_deg":83,"wind_gust":6.44,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686585600,"temp":21.03,"feels_like":20.05,"pressure":1020,"humidity":33,"dew_point":4.33,"uvi":1.18,"clouds":6,"visibility":10000,"wind_speed":4.99,"wind_deg":84,"wind_gust":6.47,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686589200,"temp":20.23,"feels_like":19.33,"pressure":1020,"humidity":39,"dew_point":5.67,"uvi":0.52,"clouds":7,"visibility":10000,"wind_speed":3.62,"wind_deg":87,"wind_gust":5.65,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686592800,"temp":18.3,"feels_like":17.39,"pressure":1020,"humidity":46,"dew_point":6.28,"uvi":0.17,"clouds":7,"visibility":10000,"wind_speed":2.05,"wind_deg":67,"wind_gust":3.53,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"pop":0},{"dt":1686596400,"temp":17.1,"feels_like":16.01,"pressure":1020,"humidity":44,"dew_point":4.76,"uvi":0,"clouds":81,"visibility":10000,"wind_speed":5.24,"wind_deg":78,"wind_gust":7.55,"weather":[{"id":803,"main":"Clouds","description":"broken clouds","icon":"04d"}],"pop":0},{"dt":1686600000,"temp":14.69,"feels_like":13.73,"pressure":1021,"humidity":58,"dew_point":6.41,"uvi":0,"clouds":90,"visibility":10000,"wind_speed":7.09,"wind_deg":105,"wind_gust":12.5,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"pop":0},{"dt":1686603600,"temp":13.78,"feels_like":12.75,"pressure":1021,"humidity":59,"dew_point":5.89,"uvi":0,"clouds":93,"visibility":10000,"wind_speed":5.9,"wind_deg":104,"wind_gust":11.17,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"pop":0},{"dt":1686607200,"temp":12.79,"feels_like":11.67,"pressure":1021,"humidity":59,"dew_point":4.99,"uvi":0,"clouds":95,"visibility":10000,"wind_speed":5.74,"wind_deg":92,"wind_gust":10.65,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"pop":0},{"dt":1686610800,"temp":11.41,"feels_like":10.25,"pressure":1021,"humidity":63,"dew_point":4.68,"uvi":0,"clouds":96,"visibility":10000,"wind_speed":4.7,"wind_deg":85,"wind_gust":10.42,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"pop":0},{"dt":1686614400,"temp":10.56,"feels_like":9.42,"pressure":1021,"humidity":67,"dew_point":4.66,"uvi":0,"clouds":97,"visibility":10000,"wind_speed":4.26,"wind_deg":65,"wind_gust":9.51,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"pop":0},{"dt":1686618000,"temp":9.4,"feels_like":7.27,"pressure":1021,"humidity":73,"dew_point":4.83,"uvi":0,"clouds":99,"visibility":10000,"wind_speed":3.98,"wind_deg":60,"wind_gust":8.28,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"pop":0},{"dt":1686621600,"temp":8,"feels_like":5.94,"pressure":1020,"humidity":81,"dew_point":4.86,"uvi":0,"clouds":92,"visibility":10000,"wind_speed":3.27,"wind_deg":52,"wind_gust":6.78,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"pop":0},{"dt":1686625200,"temp":7.44,"feels_like":5.24,"pressure":1020,"humidity":85,"dew_point":4.97,"uvi":0,"clouds":82,"visibility":10000,"wind_speed":3.31,"wind_deg":43,"wind_gust":7.16,"weather":[{"id":803,"main":"Clouds","description":"broken clouds","icon":"04d"}],"pop":0},{"dt":1686628800,"temp":9.58,"feels_like":7.48,"pressure":1020,"humidity":76,"dew_point":5.42,"uvi":0.17,"clouds":86,"visibility":10000,"wind_speed":4.01,"wind_deg":38,"wind_gust":8.76,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04d"}],"pop":0},{"dt":1686632400,"temp":10.88,"feels_like":9.98,"pressure":1020,"humidity":75,"dew_point":6.56,"uvi":0.48,"clouds":86,"visibility":10000,"wind_speed":5.15,"wind_deg":41,"wind_gust":9.71,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04d"}],"pop":0},{"dt":1686636000,"temp":11.03,"feels_like":10.25,"pressure":1020,"humidity":79,"dew_point":7.48,"uvi":1.05,"clouds":88,"visibility":10000,"wind_speed":5.43,"wind_deg":43,"wind_gust":10.24,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04d"}],"pop":0},{"dt":1686639600,"temp":11.16,"feels_like":10.45,"pressure":1020,"humidity":81,"dew_point":7.99,"uvi":0.57,"clouds":100,"visibility":10000,"wind_speed":5.21,"wind_deg":48,"wind_gust":10.53,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04d"}],"pop":0.12}],"daily":[{"dt":1686477600,"sunrise":1686450628,"sunset":1686510804,"moonrise":1686440760,"moonset":1686482220,"moon_phase":0.77,"temp":{"day":22.28,"min":12.1,"max":23.43,"night":14.93,"eve":22.44,"morn":15.4},"feels_like":{"day":21.74,"night":14.02,"eve":21.73,"morn":14.77},"pressure":1022,"humidity":45,"dew_point":9.79,"wind_speed":5.98,"wind_deg":86,"wind_gust":13.13,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"clouds":0,"pop":0,"uvi":6.13},{"dt":1686564000,"sunrise":1686537009,"sunset":1686597247,"moonrise":1686527880,"moonset":1686573540,"moon_phase":0.81,"temp":{"day":20.37,"min":10.6,"max":21.68,"night":13.78,"eve":21.03,"morn":11.49},"feels_like":{"day":19.27,"night":12.75,"eve":20.05,"morn":10.57},"pressure":1022,"humidity":31,"dew_point":2.96,"wind_speed":7.09,"wind_deg":105,"wind_gust":12.5,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"clouds":1,"pop":0,"uvi":6.12},{"dt":1686650400,"sunrise":1686623392,"sunset":1686683687,"moonrise":1686614940,"moonset":1686664860,"moon_phase":0.84,"temp":{"day":13.2,"min":7.44,"max":13.2,"night":10.94,"eve":11.56,"morn":9.58},"feels_like":{"day":12.27,"night":10.39,"eve":11.02,"morn":7.48},"pressure":1019,"humidity":65,"dew_point":6.59,"wind_speed":6,"wind_deg":55,"wind_gust":11.01,"weather":[{"id":500,"main":"Rain","description":"light rain","icon":"10d"}],"clouds":100,"pop":0.5,"rain":0.17,"uvi":2.52},{"dt":1686736800,"sunrise":1686709779,"sunset":1686770124,"moonrise":1686702060,"moonset":1686756180,"moon_phase":0.88,"temp":{"day":14.18,"min":9.24,"max":18.84,"night":13.46,"eve":18.31,"morn":9.24},"feels_like":{"day":13.51,"night":12.79,"eve":17.48,"morn":7.64},"pressure":1018,"humidity":71,"dew_point":9.03,"wind_speed":3.34,"wind_deg":49,"wind_gust":6.65,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04d"}],"clouds":99,"pop":0.17,"uvi":4.24},{"dt":1686823200,"sunrise":1686796168,"sunset":1686856558,"moonrise":1686789360,"moonset":1686847500,"moon_phase":0.91,"temp":{"day":14.99,"min":12.38,"max":18.68,"night":12.38,"eve":18.68,"morn":12.38},"feels_like":{"day":14.48,"night":11.97,"eve":18.14,"morn":11.95},"pressure":1018,"humidity":74,"dew_point":10.37,"wind_speed":4.78,"wind_deg":343,"wind_gust":5.46,"weather":[{"id":500,"main":"Rain","description":"light rain","icon":"10d"}],"clouds":96,"pop":0.39,"rain":0.29,"uvi":5.42},{"dt":1686909600,"sunrise":1686882562,"sunset":1686942989,"moonrise":1686876960,"moonset":1686938640,"moon_phase":0.94,"temp":{"day":21.34,"min":10.81,"max":24.97,"night":17.92,"eve":24.97,"morn":10.81},"feels_like":{"day":20.86,"night":17.62,"eve":24.44,"morn":10.4},"pressure":1016,"humidity":51,"dew_point":10.77,"wind_speed":3.18,"wind_deg":305,"wind_gust":7.23,"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"clouds":0,"pop":0,"uvi":6},{"dt":1686996000,"sunrise":1686968958,"sunset":1687029417,"moonrise":1686964920,"moonset":1687029480,"moon_phase":0.98,"temp":{"day":22.04,"min":13.24,"max":25.53,"night":18.24,"eve":22.21,"morn":13.24},"feels_like":{"day":21.74,"night":17.97,"eve":21.92,"morn":13.02},"pressure":1015,"humidity":55,"dew_point":12.51,"wind_speed":5.72,"wind_deg":338,"wind_gust":7.84,"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04d"}],"clouds":95,"pop":0.12,"uvi":6},{"dt":1687082400,"sunrise":1687055357,"sunset":1687115842,"moonrise":1687053540,"moonset":1687119480,"moon_phase":0,"temp":{"day":22.99,"min":14.18,"max":25.63,"night":17.74,"eve":25.07,"morn":14.18},"feels_like":{"day":22.65,"night":17.37,"eve":24.73,"morn":14.13},"pressure":1017,"humidity":50,"dew_point":11.97,"wind_speed":3.05,"wind_deg":329,"wind_gust":5.74,"weather":[{"id":500,"main":"Rain","description":"light rain","icon":"10d"}],"clouds":56,"pop":0.41,"rain":1.45,"uvi":6}]}""";

    [Test]
    public void GetAlertHashCodeSanityTest()
    {
        var alert1 = new WeatherAlert
        {
            Description = "Very hot",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
        };

        var alert2 = new WeatherAlert
        {
            Description = "Very hot",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
        };

        WeatherAdapterJob.GetAlertHashCode(alert1)
            .Should().Be(WeatherAdapterJob.GetAlertHashCode(alert2));
    }

    [Test]
    public void GetAlertHashCodeSanity2Test()
    {
        var alert1 = new WeatherAlert
        {
            Description = "Very hot",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
        };

        var alert2 = new WeatherAlert
        {
            Description = "Very cold",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
        };

        WeatherAdapterJob.GetAlertHashCode(alert1)
            .Should().NotBe(WeatherAdapterJob.GetAlertHashCode(alert2));
    }

    [Test]
    public void GetAlertHashCodeSanityWithTagsTest()
    {
        var alert1 = new WeatherAlert
        {
            Description = "Very hot",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
            Tags = ["hot"],
        };

        var alert2 = new WeatherAlert
        {
            Description = "Very hot",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
            Tags = ["hot"],
        };

        WeatherAdapterJob.GetAlertHashCode(alert1)
            .Should().Be(WeatherAdapterJob.GetAlertHashCode(alert2));
    }

    [Test]
    public void GetAlertHashCodeSanityWithTags2Test()
    {
        var alert1 = new WeatherAlert
        {
            Description = "Very hot",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
            Tags = ["hot"],
        };

        var alert2 = new WeatherAlert
        {
            Description = "Very hot",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1),
            SenderName = "Weather Bureau",
            Tags = ["cold"],
        };

        WeatherAdapterJob.GetAlertHashCode(alert1)
            .Should().NotBe(WeatherAdapterJob.GetAlertHashCode(alert2));
    }
}
