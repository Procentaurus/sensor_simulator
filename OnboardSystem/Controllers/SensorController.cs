using Microsoft.AspNetCore.Mvc;
using OnboardSystem.Models;
using OnboardSystem.Services;
using System.Text;

namespace OnboardSystem.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorController : ControllerBase
{
    private readonly SensorService _sensorService;

    public SensorController(SensorService sensorService)
    {
        _sensorService = sensorService;
    }

    [HttpGet]
    public List<Sensor> GetAll(
        [FromQuery] List<int> sensorIds,
        [FromQuery] List<long> timestamp,
        [FromQuery] double minValue = -999,
        [FromQuery] double maxValue = -999,
        [FromQuery] bool orderByTime = false,
        [FromQuery] bool orderById = false)
    {
        List<Sensor> result = _sensorService.GetAllAsync().Result;

        result = filterData(result, sensorIds, timestamp, minValue, maxValue);
        result = sortData(result, orderByTime, orderById);

        return result;
    }

    [HttpGet("csv")]
    public IActionResult GetAllCsv(
        [FromQuery] List<int> sensorIds,
        [FromQuery] List<long> timestamp,
        [FromQuery] double minValue = -999,
        [FromQuery] double maxValue = -999,
        [FromQuery] bool orderByTime = false,
        [FromQuery] bool orderById = false)
    {
        List<Sensor> result = GetAll(sensorIds, timestamp, minValue, maxValue, orderByTime, orderById);

        var csvFormat = new StringBuilder();
        csvFormat.AppendLine("dataId, id, value, unit, timestamp");

        foreach (var sensor in result)
        {
            csvFormat.AppendLine($"{sensor.dataId}, {sensor.id}, {sensor.value}, {sensor.unit}, {sensor.timestamp}");
        }

        return File(Encoding.UTF8.GetBytes(csvFormat.ToString()), "text/csv", "data.csv");
    }

    [HttpGet("latest")]
    public List<Sensor> GetLatest()
    {
        List<Sensor> latestData = new List<Sensor>();

        for (int i = 1; i <= 16; i++)
        {
            List<Sensor> result = GetAll([i]);

            result.Sort((sensor1, sensor2) => (int)(sensor2.timestamp - sensor1.timestamp));

            Sensor latestSensor = new Sensor(Guid.Parse(result[0].dataId), i, result[0].value, result[0].unit, result[0].timestamp);
            latestSensor.avgValue = result.Take(100).Average(sensor => sensor.value);

            latestData.Add(latestSensor);
        }

        return latestData;
    }

    private List<Sensor> filterData(
        List<Sensor> sensorsList,
        List<int> sensorIds,
        List<long> timestamp,
        double minValue,
        double maxValue)
    {
        if (sensorIds != null && sensorIds.Count > 0)
        {
            List<Sensor> filteredResult = new List<Sensor>();

            foreach (Sensor sensor in sensorsList)
            {
                if (sensorIds.Contains(sensor.id))
                {
                    filteredResult.Add(sensor);
                }
            }
            sensorsList = filteredResult;
        }

        if (timestamp != null && timestamp.Count > 0)
        {
            List<Sensor> filteredResult = new List<Sensor>();

            foreach (Sensor sensor in sensorsList)
            {
                if (timestamp.Contains(sensor.timestamp))
                {
                    filteredResult.Add(sensor);
                }
            }
            sensorsList = filteredResult;
        }

        if (minValue != -999)
        {
            List<Sensor> filteredResult = new List<Sensor>();

            foreach (Sensor sensor in sensorsList)
            {
                if (sensor.value >= minValue)
                {
                    filteredResult.Add(sensor);
                }
            }
            sensorsList = filteredResult;
        }

        if (maxValue != -999)
        {
            List<Sensor> filteredResult = new List<Sensor>();

            foreach (Sensor sensor in sensorsList)
            {
                if (sensor.value <= maxValue)
                {
                    filteredResult.Add(sensor);
                }
            }
            sensorsList = filteredResult;
        }
        return sensorsList;
    }

    private List<Sensor> sortData(
        List<Sensor> sensorsList,
        bool orderByTime,
        bool orderById)
    {
        if (orderByTime)
        {
            sensorsList.Sort(
                (sensor1, sensor2) => (int)(sensor1.timestamp - sensor2.timestamp));
        }

        if (orderById)
        {
            sensorsList.Sort(
                (sensor1, sensor2) => sensor1.id - sensor2.id);
        }

        return sensorsList;
    }
}
