<script setup>
import { ref } from 'vue'

const form = ref({ title: '', laboratoryId: null, wasteTypeId: null, description: '' })
</script>

<template>
  <section>
    <h2>Новая заявка</h2>
    <form @submit.prevent>
      <p>
        <label for="title">Тема (5–80 символов)</label>
        <input id="title" v-model="form.title" type="text" autofocus />
      </p>
      <p>
        <label for="laboratoryId">Номер лаборатории</label>
        <input id="laboratoryId" v-model.number="form.laboratoryId" type="number" />
      </p>
      <p>
        <label for="wasteTypeId">Номер типа отхода</label>
        <input id="wasteTypeId" v-model.number="form.wasteTypeId" type="number" />
      </p>
      <p>
        <label for="description">Описание (10–500 символов)</label>
        <textarea id="description" v-model="form.description"></textarea>
      </p>
      <button type="submit">Создать</button>
    </form>
  </section>
</template>
