import { ref } from 'vue'

export const items = ref([
  { id: 1, number: 'WR-2026-0001', title: 'Вывоз отработанных кислот', laboratoryId: 3, wasteTypeId: 7, status: 'New' },
  { id: 2, number: 'WR-2026-0002', title: 'Вывоз люминесцентных ламп', laboratoryId: 1, wasteTypeId: 4, status: 'InProgress' },
])

let nextId = 3

export function addItem(body) {
  const id = nextId++
  const item = { ...body, id, number: `TMP-${id}`, status: 'New' }
  items.value.push(item)
  return item
}
