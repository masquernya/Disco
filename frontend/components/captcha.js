import getConfig from "next/config";
import HCaptcha from "@hcaptcha/react-hcaptcha";

export default function Captcha({onVerify}) {
  return <>
    <HCaptcha sitekey={getConfig().publicRuntimeConfig.hcaptchaPublic} onVerify={(token,ekey) => {
      onVerify(token);
    }} />
  </>
}