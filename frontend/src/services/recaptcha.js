let recaptchaScriptPromise = null

const RECAPTCHA_LOAD_TIMEOUT_MS = 12000

export function isRecaptchaEnabled() {
  return String(import.meta.env.VITE_RECAPTCHA_ENABLED || 'false').toLowerCase() === 'true'
}

export function getRecaptchaVersion() {
  return String(import.meta.env.VITE_RECAPTCHA_VERSION || 'v2').toLowerCase()
}

export function isRecaptchaV2() {
  return getRecaptchaVersion() === 'v2'
}

export function getRecaptchaSiteKey() {
  return import.meta.env.VITE_RECAPTCHA_SITE_KEY || ''
}

export function ensureRecaptchaLoaded() {
  if (!isRecaptchaEnabled()) {
    return Promise.resolve()
  }

  const siteKey = getRecaptchaSiteKey()
  if (!siteKey) {
    return Promise.reject(new Error('Thiếu VITE_RECAPTCHA_SITE_KEY trong môi trường frontend'))
  }

  if (window.grecaptcha?.render) {
    return Promise.resolve()
  }

  if (recaptchaScriptPromise) {
    return recaptchaScriptPromise
  }

  const isV2 = isRecaptchaV2()
  const scriptUrls = isV2
    ? [
        'https://www.google.com/recaptcha/api.js?render=explicit',
        'https://www.recaptcha.net/recaptcha/api.js?render=explicit'
      ]
    : [
        `https://www.google.com/recaptcha/api.js?render=${encodeURIComponent(siteKey)}`,
        `https://www.recaptcha.net/recaptcha/api.js?render=${encodeURIComponent(siteKey)}`
      ]

  const loadScript = (src) => new Promise((resolve, reject) => {
    const staleScript = document.querySelector('script[data-recaptcha="true"]')
    if (staleScript) {
      staleScript.remove()
    }

    const script = document.createElement('script')
    script.src = src
    script.async = true
    script.defer = true
    script.dataset.recaptcha = 'true'

    const timeoutId = setTimeout(() => {
      script.remove()
      reject(new Error('Tải reCAPTCHA bị timeout'))
    }, RECAPTCHA_LOAD_TIMEOUT_MS)

    script.onload = () => {
      clearTimeout(timeoutId)
      if (window.grecaptcha?.render) {
        resolve()
        return
      }
      reject(new Error('reCAPTCHA script đã tải nhưng API không sẵn sàng'))
    }

    script.onerror = () => {
      clearTimeout(timeoutId)
      script.remove()
      reject(new Error('Không thể tải reCAPTCHA script'))
    }

    document.head.appendChild(script)
  })

  recaptchaScriptPromise = (async () => {
    let lastError = null

    for (const url of scriptUrls) {
      try {
        await loadScript(url)
        return
      } catch (error) {
        lastError = error
      }
    }

    throw lastError || new Error('Không thể tải reCAPTCHA script')
  })().finally(() => {
    if (!window.grecaptcha?.render) {
      recaptchaScriptPromise = null
    }
  })

  return recaptchaScriptPromise
}

export async function executeRecaptchaV3(action) {
  if (!isRecaptchaEnabled()) {
    return null
  }

  if (isRecaptchaV2()) {
    throw new Error('Cấu hình hiện tại đang dùng reCAPTCHA v2. Không thể execute v3 token.')
  }

  const siteKey = getRecaptchaSiteKey()
  if (!siteKey) {
    throw new Error('Thiếu VITE_RECAPTCHA_SITE_KEY trong môi trường frontend')
  }

  await ensureRecaptchaLoaded()

  return await new Promise((resolve, reject) => {
    window.grecaptcha.ready(async () => {
      try {
        const token = await window.grecaptcha.execute(siteKey, { action })
        resolve(token)
      } catch (error) {
        reject(error)
      }
    })
  })
}
