<script setup>
import AppHeader from './components/AppHeader.vue'
</script>

<template>
  <main>
    <AppHeader />
    <router-view />
  </main>
</template>

<style>
main { max-width: 800px; margin: 2rem auto; padding: 0 1rem; font-family: sans-serif; }
nav { margin-bottom: 1.5rem; }
nav a { margin-right: 0.75rem; }
nav a.router-link-exact-active { font-weight: bold; }
table { border-collapse: collapse; width: 100%; }
th, td { border: 1px solid #ccc; padding: 0.5rem; text-align: left; }
label { display: block; margin-bottom: 0.25rem; }
input, textarea { box-sizing: border-box; width: 100%; padding: 0.4rem; }
.error { color: #b00020; }
button:disabled { cursor: wait; }
</style>
