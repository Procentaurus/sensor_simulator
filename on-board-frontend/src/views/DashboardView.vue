<script setup>
import { onMounted, ref, computed } from 'vue'
import { Dataset, DatasetItem, DatasetShow, DatasetInfo } from 'vue-dataset'

const currentMessage = ref('No message')
let socket = null
let socketUrl = 'ws://localhost:5109/ws'
const latestSensorData = ref([])

onMounted(() => {
  fetchInitialData()

  socket = new WebSocket(socketUrl)
  socket.onmessage = onMessageHandler
})

async function fetchInitialData() {
  const response = await fetch('http://localhost:5109/api/sensors/latest', {
    method: 'GET',
  })
  const data = await response.json()

  latestSensorData.value = data
}

function onMessageHandler(event) {
  var message = JSON.parse(event.data)

  currentMessage.value = message

  if (latestSensorData.value.length != 0) {
    let sensorToUpdate = latestSensorData.value.find(
      item => item.id == message.id,
    )
    sensorToUpdate.value = message.value
    sensorToUpdate.timestamp = message.timestamp
    sensorToUpdate.avgValue = message.avgValue
  }
}

const cols = ref([
  {
    name: 'Sensor ID',
    field: 'id',
    sort: '',
  },
  {
    name: 'Latest value',
    field: 'value',
    sort: '',
  },
  {
    name: 'Latest timestamp',
    field: 'timestamp',
    sort: '',
  },
  {
    name: 'Average from latest 100',
    field: 'avgValue',
    sort: '',
  },
])

const sortBy = computed(() => {
  return cols.value.reduce((acc, o) => {
    if (o.sort) {
      o.sort === 'asc' ? acc.push(o.field) : acc.push('-' + o.field)
    }

    return acc
  }, [])
})

function clickSort(event, i) {
  let toset
  const sortEl = cols[i]

  if (!event.shiftKey) {
    cols.value.forEach(o => {
      if (o.field !== sortEl.field) {
        o.sort = ''
      }
    })
  }
  if (!sortEl.sort) {
    toset = 'asc'
  }
  if (sortEl.sort === 'desc') {
    toset = event.shiftKey ? '' : 'asc'
  }
  if (sortEl.sort === 'asc') {
    toset = 'desc'
  }
  sortEl.sort = toset
}
</script>

<template>
  <div class="dashboard">
    <h1>This is the dashboard page</h1>
    <Dataset :ds-data="latestSensorData" :ds-sortby="sortBy">
      <div class="row data-set-header">
        <DatasetShow :ds-show-entries="25" />
      </div>
      <div class="row">
        <table class="table table-striped">
          <thead>
            <tr>
              <th
                v-for="(th, index) in cols"
                :key="th.field"
                :class="['sort', th.sort]"
                @click="clickSort($event, index)"
              >
                {{ th.name }}
                <div class="column-buttons float-right">
                  <i class="gg-select float-right"></i>
                </div>
              </th>
            </tr>
          </thead>
          <DatasetItem tag="tbody">
            <template #default="{ row, rowIndex }">
              <tr>
                <th scope="row">{{ rowIndex + 1 }}</th>
                <td>{{ row.value + ' ' + row.unit }}</td>
                <td>{{ row.timestamp }}</td>
                <td>{{ row.avgValue.toFixed(2) }}</td>
              </tr>
            </template>
          </DatasetItem>
        </table>
      </div>
      <div class="row data-set-footer">
        <DatasetInfo />
      </div>
    </Dataset>
  </div>
</template>

<style>
.dashboard {
  min-height: 100vh;
  align-items: center;
}

h1 {
  align-items: center;
  justify-content: center;
  margin: 25px;
}
</style>
