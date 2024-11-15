<script setup>
import SensorDataTable from '../components/SensorDataTable.vue'
import SensorDataChart from '../components/SensorDataChart.vue'
import { useSensorDataStore } from '../stores/sensorDataStore.js'

const sensorDataStore = useSensorDataStore()

sensorDataStore.fetchSensorData()
</script>

<template>
  <h1>Sensor data:</h1>
  <div class="data">
    <SensorDataTable />
  </div>
  <div class="chart">
    <SensorDataChart />
  </div>
</template>

<style scoped>
.data {
  margin: 25px;
}

h1 {
  align-items: center;
  justify-content: center;
  margin: 25px;
}
</style>
