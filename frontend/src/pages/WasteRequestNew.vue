<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { items } from '../wasteRequestsStore.js'
import { createWasteRequest } from '../api/wasteRequests.js'
import { validateWasteRequest } from '../validation/wasteRequest.js'

const form = ref({ title: '', laboratoryId: null, wasteTypeId: null, description: '' })
const errors = ref({})
const router = useRouter()

const isSubmitting = ref(false)
const requestError = ref('')

async function onCreate() {
  if (isSubmitting.value) return
  requestError.value = ''
  errors.value = validateWasteRequest(form.value)
  if (Object.keys(errors.value).length) return
  isSubmitting.value = true
  try {
    const created = await createWasteRequest({
      title: form.value.title.trim(),
      laboratoryId: form.value.laboratoryId,
      wasteTypeId: form.value.wasteTypeId,
      description: form.value.description.trim(),
    })
    // Номер, id и статус берём из ответа, а не вычисляем в форме.
    items.value.push(created)
    form.value = { title: '', laboratoryId: null, wasteTypeId: null, description: '' }
    await router.push('/')
  } catch (e) {
    requestError.value = e.message
    errors.value = e.errors || {}
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <section>
    <h2>Новая заявка</h2>
    <p v-if="requestError" class="error" role="alert">{{ requestError }}</p>
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
      <button type="submit" :disabled="isSubmitting">{{ isSubmitting ? 'Создаём…' : 'Создать' }}</button>
    </form>
  </section>
</template>
