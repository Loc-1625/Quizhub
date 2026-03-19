<template>
  <div>
    <div :id="elementId"></div>
    <p v-if="loadError" class="mt-1 text-sm text-red-500">{{ loadError }}</p>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { ensureRecaptchaLoaded, getRecaptchaSiteKey } from '@/services/recaptcha'

const emit = defineEmits(['update:modelValue'])

const elementId = `recaptcha-v2-${Math.random().toString(36).slice(2)}`
const widgetId = ref(null)
const loadError = ref('')

function emitToken(token) {
  emit('update:modelValue', token || '')
}

async function renderWidget() {
  loadError.value = ''

  try {
    await ensureRecaptchaLoaded()

    const siteKey = getRecaptchaSiteKey()
    if (!siteKey) {
      throw new Error('Thiếu VITE_RECAPTCHA_SITE_KEY trong môi trường frontend')
    }

    widgetId.value = window.grecaptcha.render(elementId, {
      sitekey: siteKey,
      callback: (token) => {
        emitToken(token)
      },
      'expired-callback': () => {
        emitToken('')
      },
      'error-callback': () => {
        emitToken('')
      }
    })
  } catch (error) {
    loadError.value = 'Không thể tải reCAPTCHA. Vui lòng tải lại trang.'
  }
}

function reset() {
  if (widgetId.value !== null && window.grecaptcha?.reset) {
    window.grecaptcha.reset(widgetId.value)
  }
  emitToken('')
}

defineExpose({
  reset
})

onMounted(async () => {
  await renderWidget()
})
</script>
