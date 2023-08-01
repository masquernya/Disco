export default function reportEvent(eventLabel, eventData) {
  if (typeof umami === 'undefined' || !umami || typeof umami.trackEvent !== 'function') return; // TODO: something? maybe log?
  umami.trackEvent(eventLabel, eventData);
}