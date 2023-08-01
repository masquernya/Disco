import {useEffect, useState} from "react";
import s from './me.module.css';
import {getUser, setUser} from "../../lib/globalState";
import api from "../../lib/api";

const isPronounValid = pronouns => {
  if (!pronouns) return true;
  if (pronouns.length >= 64) return false;
  if (pronouns.length < 3) return false;
  const all = pronouns.split('/');
  if (all.length !== 2) return false;
  if (all[0] === all[1]) return false;
  if (all[0].length < 2) return false;
  if (all[1].length < 2) return false;
  return true;
}

const defaultPronouns = ['He/Him', 'She/Her', 'They/Them'];
export default function Pronouns(props) {
  const [pronouns, setPronouns] = useState(null);
  const [pronounSelect, setPronounSelect]=  useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    setPronouns(getUser().data.pronouns);
    if (getUser().data.pronouns) {
      if (defaultPronouns.includes(getUser().data.pronouns)) {
        setPronounSelect(getUser().data.pronouns);
      }else{
        setPronounSelect('Other');
      }
    }
  }, [getUser()]);
  const showCustomField = pronounSelect && !defaultPronouns.includes(pronounSelect);
  const isDirty = getUser().data.pronouns !== pronouns;

  const updatePronouns = pronouns => {
    setLocked(true);
    api.request('/api/user/Pronouns', {
      method: 'POST',
      body: {
        pronouns: pronouns,
      }
    }).then(() => {
      setUser({
        ...getUser(),
        data: {
          ...getUser().data,
          pronouns: pronouns,
        }
      })
    }).catch(e => {
      alert(e.message);
    }).finally(() => {
      setLocked(false);
    })
  }

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Pronouns</h4>
    <select disabled={locked} className='form-control' value={pronounSelect || 'Unspecified'} onChange={e => {
      let realPronouns = e.currentTarget.value;
      if (realPronouns === 'Unspecified') {
        realPronouns = null;
      }
      setPronounSelect(realPronouns);
      if (realPronouns === 'Other') {
        setPronouns('Other');
        return
      }
      updatePronouns(realPronouns);
    }}>
      <option value='Unspecified'>Unspecified</option>
      <option value='He/Him'>He/Him</option>
      <option value='She/Her'>She/Her</option>
      <option value='They/Them'>They/Them</option>
      <option value='Other'>Other</option>
    </select>


    {showCustomField ? <>
      <input placeholder='Enter your desired pronouns' type='text' value={pronouns || ''} className={s.description} onChange={e => {
        setPronouns(e.currentTarget.value || null);
      }} />
      <button className={s.saveButton} disabled={locked || !isDirty || !isPronounValid(pronouns)} onClick={() => {
        updatePronouns(pronouns);
      }}>Save</button>
    </> : null}
  </div>
}