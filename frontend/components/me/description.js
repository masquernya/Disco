import {useEffect, useState} from "react";
import s from './me.module.css';
import {getUser, setUser} from "../../lib/globalState";
import api from "../../lib/api";

export default function Description(props) {
  const [description, setDescription] = useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    if (getUser().data.description) {
      setDescription(getUser().data.description);
    }
  }, [getUser()]);

  const isDirty = getUser().data.description !== description;

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Description</h4>
    <textarea maxLength={1024} rows={6} value={description || ''} className={s.description} onChange={e => {
      setDescription(e.currentTarget.value || null);
    }} />
    <button className={s.saveButton} disabled={locked || !isDirty} onClick={() => {
      setLocked(true);
      api.request('/api/user/Description', {
        method: 'POST',
        body: {
          description,
        }
      }).then(() => {
        setUser({
          ...getUser(),
          data: {
            ...getUser().data,
            description,
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