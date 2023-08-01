/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
  /* config options here */
  publicRuntimeConfig: {
    baseUrl: process.env.BASE_URL,
    hcaptchaPublic: process.env.HCAPTCHA_PUBLIC,
  },
}

module.exports = nextConfig
