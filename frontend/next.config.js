/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
  /* config options here */
  publicRuntimeConfig: {
    baseUrl: process.env.BASE_URL,
    hcaptchaPublic: process.env.HCAPTCHA_PUBLIC,
    contactEmail: process.env.CONTACT_EMAIL,
    matrixBotUsername: process.env.MATRIX_BOT_USERNAME,
  },
}

module.exports = nextConfig
