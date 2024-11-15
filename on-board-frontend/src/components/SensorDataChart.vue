<script setup>
import { ref } from 'vue'
import VueApexCharts from 'vue3-apexcharts'
import { useSensorDataStore } from '../stores/sensorDataStore.js'

const sensorDataStore = useSensorDataStore()

const chartGenerated = ref(false)
const chartGenerationErrorMessage = ref('')
const series = ref([])
const chartOptions = ref({})

function generateChartButtonClick() {
  if (sensorDataStore.idFilter == '') {
    chartGenerationErrorMessage.value = 'Please set a sensor filter first'
    setTimeout(() => {
      chartGenerationErrorMessage.value = ''
    }, 2000)
    return
  }
  series.value = [
    {
      name: 'Sensor ' + sensorDataStore.idFilter,
      data: sensorDataStore.dataForSeries,
    },
  ]
  chartOptions.value = {
    chart: {
      height: 350,
      type: 'line',
      zoom: {
        enabled: true,
      },
    },
    dataLabels: {
      enabled: false,
    },
    colors: ['#00bd7e'],
    stroke: {
      curve: 'straight',
    },
    markers: {
      size: 5,
    },
    title: {
      text:
        'Values of sensor ' +
        sensorDataStore.idFilter +
        ` (${sensorDataStore.currentSensorUnit})`,
      align: 'left',
    },
    grid: {
      row: {
        colors: ['#f3f3f3', 'transparent'], // takes an array which will be repeated on columns
        opacity: 0.5,
      },
    },
    // xaxis: {
    //   categories: sensorDataStore.categoriesForChart,
    // },
    // Proper labels for normal timestamps
    xaxis: {
      type: 'datetime',
      labels: {
        datetimeUTC: false,
        format: 'mm:ss',
      },
    },
    tooltip: {
      x: {
        format: 'mm:ss',
      },
    },
  }
  chartGenerated.value = true
}
</script>

<template>
  <button @click="generateChartButtonClick()">Generate a chart</button>
  <p class="chart_generation_error">{{ chartGenerationErrorMessage }}</p>
  <VueApexCharts
    v-if="chartGenerated"
    type="line"
    height="350"
    :options="chartOptions"
    :series="series"
  ></VueApexCharts>
</template>

<style scoped>
/* Button Styles */
button {
  background-color: #00bd7e;
  color: #ffffff;
  border: none;
  padding: 10px 20px;
  font-size: 16px;
  font-weight: bold;
  border-radius: 5px;
  cursor: pointer;
  transition:
    background-color 0.3s,
    box-shadow 0.3s;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.2);
  margin: 15px 0;
}

button:hover {
  background-color: #009a63;
  box-shadow: 0px 6px 12px rgba(0, 0, 0, 0.3);
}

button:active {
  background-color: #007a4b;
  box-shadow: 0px 3px 6px rgba(0, 0, 0, 0.2);
}

/* Error Message Styles */
.chart_generation_error {
  color: #d9534f; /* Red color to indicate an error */
  font-size: 14px;
  font-weight: bold;
  margin: 5px 0;
  opacity: 0;
  transition: opacity 0.3s ease-in-out;
}

.chart_generation_error:empty {
  display: none;
}

.chart_generation_error:not(:empty) {
  opacity: 1;
}

/* Chart Container */
.vue-apexcharts {
  margin-top: 20px;
  background-color: #f9f9f9;
  padding: 15px;
  border-radius: 8px;
  box-shadow: 0px 4px 12px rgba(0, 0, 0, 0.1);
}
</style>

<style>
.apexcharts-tooltip {
  color: black;
}
</style>
