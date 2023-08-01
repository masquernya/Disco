import {useEffect, useState} from "react";
import {getUser, setUser} from "../../lib/globalState";
import s from "./me.module.css";
import api from "../../lib/api";

export default function Password() {
  const [newPassword, setNewPassword] = useState(null);
  const [oldPassword, setOldPassword] = useState(null);
  const [locked, setLocked] = useState(false);

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Update Password</h4>
    <input placeholder='Original Password' type='password' value={oldPassword || ''} className={s.description + ' mb-1'} onChange={e => {
      setOldPassword(e.currentTarget.value || null);
    }} />
    <input placeholder='New Password' type='password' value={newPassword || ''} className={s.description} onChange={e => {
      setNewPassword(e.currentTarget.value || null);
    }} />
    <button className={s.saveButton} disabled={locked} onClick={() => {
      setLocked(true);
      api.request('/api/user/Password', {
        method: 'POST',
        body: {
          originalPassword: oldPassword,
          newPassword,
        }
      }).then(() => {
        // :)
        alert('Password has been updated successfully.');
      }).catch(e => {
        alert(e.message);
      }).finally(() => {
        setLocked(false);
      })
    }}>Save</button>
  </div>
}