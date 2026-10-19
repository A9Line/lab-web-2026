<template>
  <section>
    <h2>Вход</h2>
    <p>Учебный экран. Авторизация будет добавлена в следующих работах.</p>
    <form @submit.prevent>
      <p>
        <label for="login">Логин</label>
        <input id="login" type="text" autocomplete="username" />
      </p>
      <p>
        <label for="password">Пароль</label>
        <input id="password" type="password" autocomplete="current-password" />
      </p>
      <button type="submit">Войти</button>
    </form>
  </section>
</template>
