/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
  /* config options here */
  publicRuntimeConfig: {
    baseUrl: process.env.BASE_URL,
    hcaptchaPublic: process.env.HCAPTCHA_PUBLIC,
    contactEmail: process.env.CONTACT_EMAIL,
  },
}

module.exports = nextConfig
