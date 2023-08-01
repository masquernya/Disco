import {useEffect, useState} from "react";
import s from './me.module.css';
import {getUser, setUser} from "../../lib/globalState";
import api from "../../lib/api";

export default function DisplayName(props) {
  const [name, setName] = useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    if (getUser().data.displayName) {
      setName(getUser().data.displayName);
    }
  }, [getUser()]);

  const isDirty = getUser().data.displayName !== setName;

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Display Name</h4>
    <input type='text' value={name || ''} className={s.description} onChange={e => {
      setName(e.currentTarget.value || null);
    }} />
    <button className={s.saveButton} disabled={locked || !isDirty} onClick={() => {
      setLocked(true);
      api.request('/api/user/DisplayName', {
        method: 'POST',
        body: {
          displayName: name,
        }
      }).then(() => {
        setUser({
          ...getUser(),
          data: {
            ...getUser().data,
            displayName: name,
          }
        })
      }).catch(e => {
        alert(e.message);
      }).finally(() => {
        setLocked(false);
      })
    }}>Save</button>
  </div>
}