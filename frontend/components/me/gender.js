import {useEffect, useState} from "react";
import s from './me.module.css';
import {getUser, setUser} from "../../lib/globalState";
import api from "../../lib/api";

export default function Gender(props) {
  const [gender, setGender] = useState(null);
  const [genderSelect, setGenderSelect]=  useState(null);
  const [locked, setLocked] = useState(false);
  useEffect(() => {
    setGender(getUser().data.gender);
    setGenderSelect(getUser().data.gender);
  }, [getUser()]);
  const showCustomField = genderSelect && genderSelect !== 'Male' && genderSelect !== 'Female';

  const updateGender = gender => {
    setLocked(true);
    api.request('/api/user/Gender', {
      method: 'POST',
      body: {
        gender: gender,
      }
    }).then(() => {
      setUser({
        ...getUser(),
        data: {
          ...getUser().data,
          gender: gender,
        }
      })
    }).catch(e => {
      alert(e.message);
    }).finally(() => {
      setLocked(false);
    })
  }

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Gender</h4>
    <select disabled={locked} className='form-control' value={genderSelect || 'Unspecified'} onChange={e => {
      let realGender = e.currentTarget.value;
      if (realGender === 'Unspecified') {
        realGender = null;
      }
      setGenderSelect(realGender);
      if (realGender === 'Other') {
        setGender('Other');
        return
      }
      updateGender(realGender);
    }}>
      <option value='Unspecified'>Unspecified/Hidden</option>
      <option value='Male'>Male</option>
      <option value='Female'>Female</option>
      <option value='Other'>Other</option>
    </select>


    {showCustomField ? <>
    <input placeholder='Enter a gender' type='text' value={gender || ''} className={s.description} onChange={e => {
      setGender(e.currentTarget.value || null);
    }} />
    <button className={s.saveButton} disabled={locked} onClick={() => {
      updateGender(gender);
    }}>Save</button>
    </> : null}
  </div>
}