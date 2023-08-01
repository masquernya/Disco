import s from './login.module.css';
import {useState} from "react";
import api from "../../lib/api";
import Router from "next/router";
import {setUser} from "../../lib/globalState";
import Link from "next/link";
export default function Login(props) {
  const [error, setError] = useState(null);
  const [username, setUsername] = useState(null);
  const [password, setPassword] = useState(null);
  const [locked, setLocked] = useState(false);

  const errorString = (() => {
    if (error)
       return error;
    if (username === '')
      return 'Username is a required field.';
    if (password === '')
      return 'Password is a required field.';
    if (username && username.length > 64)
      return 'Username is too long.';

    return null;
  })();

  return <div className={s.loginForm}>
    <h3 className={s.header}>Login</h3>
    {
      errorString ? <p className={s.error}>{errorString}</p> : null
    }
    <div className={s.inputContainer}>
      <label className={s.label + ' text-uppercase'}>Username</label>
      <input disabled={locked} id='loginUsername' type='text' className={s.input} value={username || ''} onChange={e => {
        setUsername(e.currentTarget.value);
      }} />
    </div>
    <div className={s.inputContainer}>
      <label className={s.label + ' text-uppercase'}>Password</label>
      <input disabled={locked} id='loginPassword' type='password' className={s.input} value={password || ''} onChange={e => {
        setPassword(e.currentTarget.value);
      }} />
    </div>

    <button disabled={locked} className={s.loginButton + ' text-uppercase'} onClick={() => {
      setLocked(true);
      setError(null);
      let trueUsername = username;
      if (trueUsername.startsWith('@')) {
        trueUsername = trueUsername.substr(1);
      }
      api.request('/api/user/Login', {
        method: 'POST',
        body: {
          username: trueUsername,
          password,
        }
      }).then(result => {
        // OK
        api.request('/api/user/MyUser').then(data => {
          console.log('logged in as', data.body);
          setUser({
            isLoggedIn: true,
            data: data.body,
          });
          Router.push('/')
        }).catch(e => {
          setError(e.message);
          setLocked(false);
        })
      }).catch(err => {
        if (err.code === 'InvalidUsernameOrPasswordException') {
          setError('Username or password is invalid');
        }else if (err.code === 'AccountBannedException') {
          // explicit
          setError(err.body.message || err.code);
        }else{
          setError(err.message);
        }
        setLocked(false);
      })
    }}>Login</button>
    <p className='mt-2 mb-0'>Don't have an account? <Link href={'/join'}>Sign Up</Link></p>
  </div>
}