import getConfig from "next/config";

class ApiException extends Error {
  response = null;
  body = null;
  code = null;
  constructor(url, config, response, body) {
    super('ApiException: '+  config.method + ' ' + url + ': ' + response.status);
    this.response = response;
    this.config = config;
    this.url= url;
    this.body = body;
    if (typeof body === 'object' && body && body.code) {
      this.code = body.code;
    }
  }
}

class API {
  _xsrf = ""
  async request(path, config) {
    if (!config) {
      config = {}
    }
    if (config._xsrfAttemptCount && config._xsrfAttemptCount > 3)
      throw new Error('XSRF retry reached. Are cookies enabled?');
    if (!config.credentials) {
      config.credentials = 'include';
    }
    if (!config.method) {
      config.method ='GET';
    }
    if (!config.headers) {
      config.headers = {}
    }
    if (config.method !== 'GET') {
      if (!config._xsrfAttemptCount)
        config._xsrfAttemptCount = 0;
      config._xsrfAttemptCount++
      config.headers['xsrf'] = this._xsrf;
    }
    if (config.body !== undefined && !config.headers['content-type']) {
      config.headers['content-type'] = 'application/json';
    }
    if (typeof config.body === 'object')
      config.body = JSON.stringify(config.body);
    const data = await fetch(getConfig().publicRuntimeConfig.baseUrl + path, config);
    let text = await data.text();
    if (data.headers) {
      if (data.headers.get('content-type') && data.headers.get('content-type').indexOf('application/json') !== -1) {
        text = JSON.parse(text);
      }
      if (data.status === 403 && data.headers.get('xsrf') && config.method !== 'GET') {
        this._xsrf = data.headers.get('xsrf');
        return await this.request(path, config);
      }
    }
    // validate
    if (!(data.status >= 200 && data.status <= 299)) {
      throw new ApiException(path, config, data, text);
    }
    return {
      ...data,
      body: text,
    }
  }
}

const api = new API();
export default api;