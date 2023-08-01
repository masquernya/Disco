import {useEffect, useState} from "react";
import s from './login.module.css';
export default function Username({username, setUsername}) {
  return <div className={s.inputContainer}>
    <label className={s.label}>Username</label>
    <input id='loginUsername' type='text' className={s.input} value={username} onChange={e => {
      setUsername(e.currentTarget.value);
    }} />
  </div>
}