import { createRouter, createWebHistory } from 'vue-router'
import WasteRequestList from './pages/WasteRequestList.vue'
import WasteRequestNew from './pages/WasteRequestNew.vue'
import Login from './pages/Login.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: WasteRequestList },
    { path: '/new', component: WasteRequestNew },
    { path: '/login', component: Login },
  ],
})
