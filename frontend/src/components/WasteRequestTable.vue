<script setup>
defineProps({ items: { type: Array, required: true } })
</script>

<template>
  <table>
    <thead>
      <tr>
        <th>Номер</th><th>Тема</th><th>Лаборатория</th><th>Тип отхода</th><th>Статус</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item.id">
        <td>{{ item.number }}</td>
        <td>{{ item.title }}</td>
        <td>{{ item.laboratoryId }}</td>
        <td>{{ item.wasteTypeId }}</td>
        <td>{{ item.status }}</td>
      </tr>
    </tbody>
  </table>
</template>
