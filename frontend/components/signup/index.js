import s from '../login/login.module.css';
import {useRef, useState} from "react";
import api from "../../lib/api";
import Router from "next/router";
import {setUser} from "../../lib/globalState";
import Link from "next/link";
import dynamic from "next/dynamic";
import HCaptcha from "@hcaptcha/react-hcaptcha";
import getConfig from "next/config";
import reportEvent from "../../events/eventBase";
const Captcha = dynamic(() => import('../../components/captcha'), {ssr: false});

export default function SignUp(props) {
  const [error, setError] = useState(null);
  const [username, setUsername] = useState(null);
  const [password, setPassword] = useState(null);
  const [passwordConfirm, setPasswordConfirm] = useState(null);
  const [locked, setLocked] = useState(false);
  const [showCaptcha, setShowCaptcha] = useState(true);
  const [captchaToken, setCaptchaToken] = useState(null);
  const [createdAt] = useState(Date.now());
  const captchaRef = useRef(null);

  const errorString = (() => {
    if (error)
      return error;
    if (username === '')
      return 'Username is a required field.';
    if (password === '')
      return 'Password is a required field.';
    if (password && password.length < 3)
      return 'Password is too short.';
    if (username && username.length > 64)
      return 'Username is too long.';
    if (username && username.length < 3)
      return 'Username is too short';
    if (typeof username === 'string') {
      let user = username.match(/[^a-zA-Z0-9]/gi);
      if (user !== null)
        return 'Username can only contain letters and numbers.';
    }


    if (password && passwordConfirm !== password)
      return 'Passwords do not match.';

    return null;
  })();

  return <div className={s.loginForm}>
    <h3 className={s.header}>Sign Up</h3>
    {
      errorString ? <p className={s.error}>{errorString}</p> : null
    }
    <div className={s.inputContainer}>
      <label className={s.label + ' text-uppercase'}>Username</label>
      <input disabled={locked} id='signUpUsername' type='text' className={s.input} value={username || ''} onChange={e => {
        setUsername(e.currentTarget.value);
      }} />
    </div>
    <div className={s.inputContainer}>
      <label className={s.label + ' text-uppercase'}>Password</label>
      <input disabled={locked} id='signUpPassword' type='password' className={s.input} value={password || ''} onChange={e => {
        setPassword(e.currentTarget.value);
      }} />
    </div>
    <div className={s.inputContainer}>
      <label className={s.label + ' text-uppercase'}>Confirm Password</label>
      <input disabled={locked} id='passwordConfirm' type='password' className={s.input} value={passwordConfirm || ''} onChange={e => {
        setPasswordConfirm(e.currentTarget.value);
      }} />
    </div>
    {showCaptcha ? <div className={s.inputContainer}>
      <label className={s.label + ' text-uppercase'}>Captcha</label>
      <HCaptcha ref={captchaRef} sitekey={getConfig().publicRuntimeConfig.hcaptchaPublic} onVerify={(token,ekey) => {
        setCaptchaToken(token);
      }} />
    </div> : null}

    <p className='mt-2 mb-4'>By creating an account, you agree to our <Link href={'/terms-of-service'}>Terms of Service</Link> and <Link href='/privacy-policy'>Privacy Policy</Link>. You must be 13 years of age or older to create an account. If you are 16 years of age or under, you must have parental consent to create an account.</p>
    <button disabled={locked} className={s.loginButton + ' text-uppercase'} onClick={() => {
      if (!captchaToken && showCaptcha) {
        setError('Captcha is required.');
        return;
      }
      setLocked(true);
      setError(null);
      let trueUsername = username;
      if (trueUsername.startsWith('@')) {
        trueUsername = trueUsername.substr(1);
      }
      api.request('/api/user/SignUp', {
        method: 'POST',
        body: {
          username: trueUsername,
          password,
          captcha: captchaToken || undefined,
        }
      }).then(result => {
        // OK
        reportEvent('SignUpSuccess', {durationMs: Date.now()-createdAt});
        api.request('/api/user/MyUser').then(data => {
          console.log('logged in as', data.body);
          setUser({
            isLoggedIn: true,
            data: data.body,
          });
          Router.push('/onboarding')
        }).catch(e => {
          setError(e.message);
          setLocked(false);
        })
      }).catch(err => {
        if (captchaRef.current)
          captchaRef.current.resetCaptcha();
        if (err.code === 'UsernameTakenException') {
          setError('Username has been taken. Please try another one.');
          reportEvent('SignupFailUsernameTakenException');
        }else if (err.code === 'InvalidUsernameException') {
          setError('Username is invalid. Please only use numbers and letters.');
          reportEvent('SignupFailInvalidUsernameException'); // should not happen
        }else if (err.code === 'CaptchaFailedException') {
          setShowCaptcha(true);
          setError('Please complete the captcha.');
          reportEvent('SignUpFailCaptchaFailedException_' + (showCaptcha ? 'AfterShowCaptcha' : 'BeforeShowCaptcha'));
        }else{
          setError(err.message);
          reportEvent('SignUpFailUnknownException', {message: err.message, code: err.code});
        }
        setLocked(false);
      })
    }}>Sign Up</button>
    <p className='mt-2 mb-0'>Already have an account? <Link href={'/login'}>Login</Link></p>
  </div>
}