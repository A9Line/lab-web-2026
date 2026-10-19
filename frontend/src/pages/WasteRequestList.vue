<script setup>
import { onMounted, ref } from 'vue'
import WasteRequestTable from '../components/WasteRequestTable.vue'
import { items } from '../wasteRequestsStore.js'
import { getWasteRequests } from '../api/wasteRequests.js'

const screen = ref('loading')
const error = ref('')

async function load() {
  screen.value = 'loading'
  error.value = ''
  try {
    items.value = await getWasteRequests()
    screen.value = items.value.length ? 'data' : 'empty'
  } catch (e) {
    error.value = e.message
    screen.value = 'error'
  }
}

onMounted(load)
</script>

<template>
  <section aria-live="polite">
    <h2>Заявки</h2>
    <p v-if="screen === 'loading'">Загрузка…</p>
    <p v-else-if="screen === 'empty'">Заявок пока нет</p>
    <WasteRequestTable v-else-if="screen === 'data'" :items="items" />
    <div v-else>
      <p class="error" role="alert">{{ error }}</p>
      <button type="button" @click="load">Повторить</button>
    </div>
  </section>
</template>
