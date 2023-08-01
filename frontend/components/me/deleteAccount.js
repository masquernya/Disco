import {useEffect, useState} from "react";
import {getUser, setUser} from "../../lib/globalState";
import s from "./me.module.css";
import api from "../../lib/api";

export default function DeleteAccount() {
  const [username, setUsername] = useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    setUsername(null);
  }, [getUser()]);

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Delete Account</h4>
    <p className={s.subtitleHelp}>Deleting your account is permanent and cannot be undone. To delete your account, type your username in the box below and press "Delete Account".</p>
    <input type='text' value={username || ''} className={s.description} onChange={e => {
      setUsername(e.currentTarget.value || null);
    }} />
    <button className='btn btn-sm btn-outline-danger mt-4' disabled={locked || username !== getUser().data.username} onClick={() => {
      setLocked(true);
      api.request('/api/user/DeleteAccount', {
        method: 'POST',
      }).then(() => {
        alert('Your account has been successfully deleted.');
        window.location.href = '/';
      }).catch(e => {
        alert(e.message);
      }).finally(() => {
        setLocked(false);
      })
    }}>Delete Account</button>
  </div>
}