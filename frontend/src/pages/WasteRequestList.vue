<script setup>
import WasteRequestTable from '../components/WasteRequestTable.vue'
import { items } from '../wasteRequestsStore.js'
</script>

<template>
  <section>
    <h2>Заявки</h2>
    <WasteRequestTable :items="items" />
  </section>
</template>
