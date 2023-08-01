import s from './nvabar.module.css';
import {useEffect, useState} from "react";
import getConfig from 'next/config';
import api from "../../lib/api";
import Link from "next/link";
import {getUser, setUser} from "../../lib/globalState";
import reportEvent from "../../events/eventBase";
export default function AuthenticationArea(props) {
  const isLoggedIn = getUser();
  useEffect(() => {
    api.request('/api/user/MyUser').then(data => {
      setUser({
        isLoggedIn: true,
        data: data.body,
      });
    }).catch(e => {
      if (e.code === 'UnauthorizedException') {
        setUser({
          isLoggedIn: false,
          data: null,
        });
      }else{
        // unknown error?
        console.log('unknown error',e.code,e.body);
      }
    })
  }, []);
  return <div className={s.navbarItem}>
    {isLoggedIn === null ? <div className='spinner-border spinner-border-sm'/>
      : (isLoggedIn.isLoggedIn === false ? <Link href='/join' className={s.signUp}  onClick={_ => {
        reportEvent('JoinButtonClick_Nav');
      }}>Join</Link> : <Link className={s.navbarLink} href='/me'>@{isLoggedIn.data.username}</Link> )}
  </div>
}