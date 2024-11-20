<script setup>
import { ref, computed } from 'vue'
import {
  Dataset,
  DatasetItem,
  DatasetShow,
  DatasetInfo,
  DatasetPager,
} from 'vue-dataset'
import { useSensorDataStore } from '../stores/sensorDataStore.js'

const sensorDataStore = useSensorDataStore()

const listboxActive = ref(false)
const listboxColumn = ref(0)

const sortBy = computed(() => {
  return sensorDataStore.cols.reduce((acc, o) => {
    if (o.sort) {
      o.sort === 'asc' ? acc.push(o.field) : acc.push('-' + o.field)
    }

    return acc
  }, [])
})

function clickSort(event, i) {
  let toset
  const sortEl = sensorDataStore.cols[i]

  if (!event.shiftKey) {
    sensorDataStore.cols.forEach(o => {
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

function clickFilter(event, index) {
  event.stopPropagation()

  if (!listboxActive.value) {
    listboxColumn.value = index
    listboxActive.value = true
  } else if (listboxColumn.value != index) {
    listboxColumn.value = index
  } else {
    listboxActive.value = false
  }
}

function clickFilterItem(event, index, category) {
  event.stopPropagation()

  sensorDataStore.cols[index].filter = category

  listboxActive.value = false
}

function clickResetFilter(event, index) {
  event.stopPropagation()

  sensorDataStore.cols[index].filter = ''

  listboxActive.value = false
}

async function clickDownloadJSON() {
  const response = await sensorDataStore.fetchBuilder()
  const data = await response.json()

  const jsonData = JSON.stringify(data, null, 2)
  const blob = new Blob([jsonData], { type: 'application/json' })
  const url = URL.createObjectURL(blob)

  const link = document.createElement('a')
  link.href = url
  link.download = 'sensorData.json'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)

  URL.revokeObjectURL(url)
}

async function clickDownloadCSV() {
  const response = await sensorDataStore.fetchBuilder(true)
  const blob = await response.blob()

  const url = URL.createObjectURL(blob)

  const link = document.createElement('a')
  link.href = url
  link.download = 'sensorData.csv'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)

  URL.revokeObjectURL(url)
}
</script>

<template>
  <Dataset
    :ds-data="sensorDataStore.sensorData"
    :ds-filter-fields="{
      id: sensorDataStore.idFilter,
      timestamp: sensorDataStore.timestampFilter,
    }"
    :ds-sortby="sortBy"
  >
    <div class="row data-set-header">
      <DatasetShow />
      <div>
        <button class="download-button" @click.prevent="clickDownloadJSON()">
          Download JSON
        </button>
        <button class="download-button" @click.prevent="clickDownloadCSV()">
          Download CSV
        </button>
      </div>
    </div>
    <div class="row">
      <table class="table table-striped">
        <thead>
          <tr>
            <th scope="col">#</th>
            <th
              v-for="(th, index) in sensorDataStore.cols"
              :key="th.field"
              :class="['sort', th.sort]"
              @click="clickSort($event, index)"
            >
              {{ th.name }}
              <div class="column-buttons float-right">
                <i class="gg-filter" @click="clickFilter($event, index)"></i>
                <i class="gg-select float-right"></i>
              </div>
              <div
                class="listbox dropdown"
                v-if="listboxActive && index == listboxColumn"
              >
                <ul>
                  <li @click="clickResetFilter($event, index)">Reset</li>
                  <li
                    v-for="category in th.categories"
                    :key="category"
                    @click="clickFilterItem($event, index, category)"
                  >
                    {{ category }}
                  </li>
                </ul>
              </div>
            </th>
          </tr>
        </thead>
        <DatasetItem tag="tbody">
          <template #default="{ row, rowIndex }">
            <tr>
              <th scope="row">{{ rowIndex + 1 }}</th>
              <td>{{ row.id }}</td>
              <td>{{ row.value + ' ' + row.unit }}</td>
              <td>{{ row.timestamp }}</td>
            </tr>
          </template>
        </DatasetItem>
      </table>
    </div>
    <div class="row data-set-footer">
      <DatasetInfo />
      <DatasetPager />
    </div>
  </Dataset>
</template>

<style>
.download-button {
  background-color: hsla(160, 100%, 37%, 1);
  color: rgb(224, 224, 224);
  border: 2px solid #b1b1b1;
  padding: 5px 8px;
  font-size: 15px;
  cursor: pointer;
  border-radius: 4px;
  margin: 5px;
  transition:
    background-color 0.3s,
    color 0.3s,
    box-shadow 0.3s;
  box-shadow: 0px 0px 7px black;
}

.download-button:hover {
  background-color: hsla(160, 100%, 90%, 0.2);
  color: hsla(160, 100%, 37%, 1);
  box-shadow: 0px 0px 10px hsla(160, 100%, 37%, 0.7);
}

.download-button:active {
  background-color: hsla(160, 100%, 37%, 0.8);
  box-shadow: 0px 0px 5px hsla(160, 100%, 37%, 1);
}
</style>
