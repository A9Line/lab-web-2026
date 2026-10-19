<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { addItem } from '../wasteRequestsStore.js'
import { validateWasteRequest } from '../validation/wasteRequest.js'

const form = ref({ title: '', laboratoryId: null, wasteTypeId: null, description: '' })
const errors = ref({})
const router = useRouter()

function onCreate() {
  errors.value = validateWasteRequest(form.value)
  if (Object.keys(errors.value).length) return
  addItem({
    title: form.value.title.trim(),
    laboratoryId: form.value.laboratoryId,
    wasteTypeId: form.value.wasteTypeId,
    description: form.value.description.trim(),
  })
  form.value = { title: '', laboratoryId: null, wasteTypeId: null, description: '' }
  router.push('/')
}
</script>

<template>
  <section>
    <h2>Новая заявка</h2>
    <form novalidate @submit.prevent="onCreate">
      <p>
        <label for="title">Тема (5–80 символов)</label>
        <input id="title" v-model="form.title" type="text" autofocus />
        <span v-if="errors.title" class="error" role="alert">{{ errors.title }}</span>
      </p>
      <p>
        <label for="laboratoryId">Номер лаборатории</label>
        <input id="laboratoryId" v-model.number="form.laboratoryId" type="number" />
        <span v-if="errors.laboratoryId" class="error" role="alert">{{ errors.laboratoryId }}</span>
      </p>
      <p>
        <label for="wasteTypeId">Номер типа отхода</label>
        <input id="wasteTypeId" v-model.number="form.wasteTypeId" type="number" />
        <span v-if="errors.wasteTypeId" class="error" role="alert">{{ errors.wasteTypeId }}</span>
      </p>
      <p>
        <label for="description">Описание (10–500 символов)</label>
        <textarea id="description" v-model="form.description"></textarea>
        <span v-if="errors.description" class="error" role="alert">{{ errors.description }}</span>
      </p>
      <button type="submit">Создать</button>
    </form>
  </section>
</template>
