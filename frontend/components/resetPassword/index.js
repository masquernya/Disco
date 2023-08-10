import s from "../login/login.module.css";
import api from "../../lib/api";
import {setUser} from "../../lib/globalState";
import Router from "next/router";
import Link from "next/link";
import {useState} from "react";
import config from "next/config";

export default function ResetPassword(props) {
  const [error, setError] = useState(null);
  const [username, setUsername] = useState(null);
  const [locked, setLocked] = useState(false);
  const [password, setPassword] = useState(null);

  const [matrixUsername, setMatrixUsername] = useState(null);
  const [discordUsername, setDiscordUsername] = useState(null);

  const [token, setToken] = useState(props.token || null);
  const verified = props.verified;

  const errorString = (() => {
    if (error)
      return error;
    if (username === '')
      return 'Username is a required field.';

    if (username && username.length > 64)
      return 'Username is too long.';

    if (matrixUsername === '')
      return 'Your matrix username is required';

    // if (matrixUsername && matrixUsername.split(':').length !== 2)
    //   return 'Invalid matrix username.';

    return null;
  })();

  return <div className={s.loginForm}>
    <h3 className={s.header}>Reset Password</h3>
    {
      errorString ? <p className={s.error}>{errorString}</p> : null
    }

    {
      verified && token ? <>
          <p className={s.description}>Enter a new password, then press "Update Password".</p>
          <div className={s.inputContainer}>
            <label className={s.label + ' text-uppercase'}>New Password</label>
            <input disabled={locked} id='loginPassword' type='text' className={s.input} value={password || ''} onChange={e => {
              setPassword(e.currentTarget.value);
            }} />
          </div>


          <button disabled={locked} className={s.loginButton + ' text-uppercase'} onClick={() => {
            if (!token || !password || locked)
              return;

            setError(null);
            api.request('/api/User/PasswordReset/Submit', {
              method: 'POST',
              body: {
                token: token,
                newPassword: password,
              }
            }).then(() => {
              window.location.href = '/login';
            }).catch(err => {
              setError(err.message);
            })
          }}>Update Password</button>
        </> :
      token ? <>
        <p className={s.description}>Send the following message exactly as it appears to our matrix bot: "{config().publicRuntimeConfig.matrixBotUsername}". You must use the account you entered, "{matrixUsername}".</p>
        <p className={s.verificationMessage}>!disco resetpassword {token}</p>
      </> : <>
        <p className={s.description}>You'll need access to the Matrix account you added to your account.</p>
        <div className={s.inputContainer}>
          <label className={s.label + ' text-uppercase'}>DiscoFriends Username</label>
          <input disabled={locked} id='loginUsername' type='text' className={s.input} value={username || ''} onChange={e => {
            setUsername(e.currentTarget.value);
          }} />
        </div>
        <div className={s.inputContainer}>
          <label className={s.label + ' text-uppercase'}>Matrix Username</label>
          <input disabled={locked} id='loginMatrixUsername' type='text' className={s.input} value={matrixUsername || ''} onChange={e => {
            setMatrixUsername(e.currentTarget.value);
          }} />
        </div>

        <button disabled={locked} className={s.loginButton + ' text-uppercase'} onClick={() => {
          if (!matrixUsername || !username || locked)
            return;

          setError(null);
          api.request('/api/User/PasswordReset/Matrix', {
            method: 'POST',
            body: {
              matrixUserId: matrixUsername,
              username: username,
            }
          }).then(resetToken => {
            setToken(resetToken.body.token);
          }).catch(err => {
            if (err.code === 'InvalidMatrixUsernameException') {
              return setError('Invalid matrix username.');
            }
            setError(err.message);
          })
        }}>Continue</button>
      </>
    }

    <p className='mt-2 mb-0'>Don't have an account? <Link href={'/join'}>Sign Up</Link></p>
  </div>
}