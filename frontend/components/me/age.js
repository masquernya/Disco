import {useEffect, useState} from "react";
import s from './me.module.css';
import {getUser, setUser} from "../../lib/globalState";
import api from "../../lib/api";
const minAge = 13;
const maxAge = 99;

export default function Age(props) {
  const [age, setAge] = useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    setAge(getUser().data.age);
  }, [getUser()]);

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Age</h4>
    <select disabled={locked} className='form-control' value={age || 'Unspecified'} onChange={e => {
      setLocked(true);
      let realAge = e.currentTarget.value;
      if (realAge === 'Unspecified') {
        realAge = null;
      }else{
        realAge = parseInt(realAge, 10);
      }
      api.request('/api/user/Age', {
        method: 'POST',
        body: {
          age: realAge,
        }
      }).then(() => {
        setUser({
          ...getUser(),
          data: {
            ...getUser().data,
            age: realAge,
          }
        })
      }).catch(e => {
        alert(e.message);
      }).finally(() => {
        setLocked(false);
      })
    }}>
      <option value='Unspecified'>Unspecified/Hidden</option>
      {
        [... new Array((maxAge+1)-minAge)].map((_, i) => {
          return <option key={i} value={i+minAge}>{i+minAge}</option>
        })
      }
    </select>
  </div>
}