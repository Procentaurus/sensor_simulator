import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useSensorDataStore = defineStore('sensorData', () => {
  const sensorData = ref([])

  const idFilter = ref('')
  const timestampFilter = ref('')

  const idCategories = computed(() => {
    return [...new Set(sensorData.value.map(item => item.id))].sort(
      (a, b) => a - b,
    )
  })
  const timestampCategories = computed(() => {
    return [...new Set(sensorData.value.map(item => item.timestamp))].sort(
      (a, b) => a - b,
    )
  })

  const cols = ref([
    {
      name: 'Sensor ID',
      field: 'id',
      sort: '',
      categories: idCategories,
      filter: idFilter,
    },
    {
      name: 'Value',
      field: 'value',
      sort: '',
    },
    {
      name: 'Timestamp (s)',
      field: 'timestamp',
      sort: '',
      categories: timestampCategories,
      filter: timestampFilter,
    },
  ])

  const dataForSeries = computed(() => {
    if (idFilter.value == '') {
      throw new Error('Error when computing series: Sensor filter not set')
    }
    return (
      sensorData.value
        .filter(item => {
          return item.id == idFilter.value
        })
        .sort((a, b) => a.timestamp - b.timestamp)
        // .map(item => item.value)
        .map(item => ({
          x: item.timestamp * 1000,
          y: item.value,
        }))
    )
  })

  const currentSensorUnit = computed(() => {
    if (idFilter.value == '') {
      throw new Error(
        'Error when computing currentSensorUnit: Sensor filter not set',
      )
    }

    return sensorData.value.find(item => item.id == idFilter.value).unit
  })

  const categoriesForChart = computed(() => {
    if (idFilter.value == '') {
      throw new Error(
        'Error when computing categoriesForChart: Sensor filter not set',
      )
    }

    return [
      ...new Set(
        sensorData.value
          .filter(item => {
            return item.id == idFilter.value
          })
          .map(item => item.timestamp),
      ),
    ].sort((a, b) => a - b)
  })

  function fetchBuilder(csv = false) {
    const sensorDataUrl = 'http://localhost:5109/api/sensors'
    const csvUrl = csv ? '/csv' : ''
    const params = new URLSearchParams()

    if (idFilter.value) {
      params.append('sensorIds', idFilter.value)
    }

    if (timestampFilter.value) {
      params.append('timestamp', timestampFilter.value)
    }

    return fetch(`${sensorDataUrl}${csvUrl}?${params.toString()}`, {
      method: 'GET',
    })
  }

  async function fetchSensorData() {
    const response = await fetchBuilder()
    const data = await response.json()

    sensorData.value = data
  }

  return {
    sensorData,
    idFilter,
    timestampFilter,
    idCategories,
    timestampCategories,
    cols,
    dataForSeries,
    currentSensorUnit,
    categoriesForChart,
    fetchBuilder,
    fetchSensorData,
  }
})
