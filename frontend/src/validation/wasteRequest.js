export function validateWasteRequest(form) {
  const errors = {}
  const title = typeof form.title === 'string' ? form.title.trim() : ''
  const description = typeof form.description === 'string' ? form.description.trim() : ''
  if (title.length < 5 || title.length > 80) {
    errors.title = 'Тема должна содержать от 5 до 80 символов'
  }
  if (!Number.isInteger(form.laboratoryId) || form.laboratoryId <= 0) {
    errors.laboratoryId = 'Номер лаборатории должен быть целым числом больше нуля'
  }
  if (!Number.isInteger(form.wasteTypeId) || form.wasteTypeId <= 0) {
    errors.wasteTypeId = 'Номер типа отхода должен быть целым числом больше нуля'
  }
  if (description.length < 10 || description.length > 500) {
    errors.description = 'Описание должно содержать от 10 до 500 символов'
  }
  return errors
}
