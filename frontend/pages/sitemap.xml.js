import getConfig from "next/config";

const baseUrl = getConfig().publicRuntimeConfig.baseUrl;
function generateSiteMap() {
  return `<?xml version="1.0" encoding="UTF-8"?>
  <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
    <url>
      <loc>${baseUrl}</loc>
    </url>
    <url>
      <loc>${baseUrl}/join</loc>
    </url>
    <url>
      <loc>${baseUrl}/login</loc>
    </url>
    <url>
      <loc>${baseUrl}/contact</loc>
    </url>
    <url>
      <loc>${baseUrl}/terms-of-service</loc>
    </url>
    <url>
      <loc>${baseUrl}/privacy-policy</loc>
    </url>
   </urlset>
 `;
}

function SiteMap() {
  // getServerSideProps will do the heavy lifting
}

export async function getServerSideProps({ res }) {
  // We generate the XML sitemap with the posts data
  const sitemap = generateSiteMap();

  res.setHeader('Content-Type', 'text/xml');
  // we send the XML to the browser
  res.write(sitemap);
  res.end();

  return {
    props: {},
  };
}

export default SiteMap;