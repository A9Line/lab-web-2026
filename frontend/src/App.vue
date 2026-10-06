<script setup>
import { ref } from 'vue'

const screen = ref('list')

const items = ref([
  {
    id: 1,
    number: 'WR-2026-0001',
    title: 'Вывоз отработанных кислот',
    laboratoryId: 3,
    wasteTypeId: 7,
    status: 'New'
  },
  {
    id: 2,
    number: 'WR-2026-0002',
    title: 'Вывоз люминесцентных ламп',
    laboratoryId: 1,
    wasteTypeId: 4,
    status: 'InProgress'
  }
])

const newTitle = ref('')
const newLaboratoryId = ref(null)
const newWasteTypeId = ref(null)
const newDescription = ref('')
</script>

<template>
  <main>
    <h1>Вывоз отходов лаборатории</h1>

    <nav>
      <button type="button" @click="screen = 'list'">Список</button>
      <button type="button" @click="screen = 'new'">Создать</button>
      <button type="button" @click="screen = 'login'">Вход</button>
    </nav>

    <section v-if="screen === 'list'">
      <h2>Заявки</h2>
      <table>
        <thead>
          <tr>
            <th>Номер</th>
            <th>Тема</th>
            <th>Лаборатория</th>
            <th>Тип отхода</th>
            <th>Статус</th>
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
    </section>

    <section v-else-if="screen === 'new'">
      <h2>Новая заявка</h2>
      <form @submit.prevent>
        <p>
          <label for="title">Тема (5–80 символов)</label>
          <input id="title" v-model="newTitle" type="text" autofocus />
        </p>
        <p>
          <label for="laboratoryId">Номер лаборатории</label>
          <input id="laboratoryId" v-model="newLaboratoryId" type="number" />
        </p>
        <p>
          <label for="wasteTypeId">Номер типа отхода</label>
          <input id="wasteTypeId" v-model="newWasteTypeId" type="number" />
        </p>
        <p>
          <label for="description">Описание (10–500 символов)</label>
          <textarea id="description" v-model="newDescription"></textarea>
        </p>
        <button type="submit">Создать</button>
      </form>
    </section>

    <section v-else-if="screen === 'login'">
      <h2>Вход</h2>
      <form @submit.prevent>
        <p>
          <label for="login">Логин</label>
          <input id="login" type="text" />
        </p>
        <p>
          <label for="password">Пароль</label>
          <input id="password" type="password" />
        </p>
        <button type="submit">Войти</button>
      </form>
    </section>
  </main>
</template>

<style scoped>
main {
  max-width: 800px;
  margin: 2rem auto;
  font-family: sans-serif;
}
nav {
  margin-bottom: 1.5rem;
}
nav button {
  margin-right: 0.5rem;
}
table {
  border-collapse: collapse;
  width: 100%;
}
th, td {
  border: 1px solid #ccc;
  padding: 0.5rem;
  text-align: left;
}
label {
  display: block;
  margin-bottom: 0.25rem;
}
input, textarea {
  width: 100%;
  padding: 0.4rem;
  margin-bottom: 0.75rem;
}
</style>