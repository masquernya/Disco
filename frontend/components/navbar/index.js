import Link from "next/link";
const logoUrl = "/logo_500.png";
import s from './nvabar.module.css';
import dynamic from "next/dynamic";
// const AuthenticationArea = dynamic(() => import('./authenticationArea'), {ssr: false});
import AuthenticationArea from "./authenticationArea";
import {useState} from "react";

export default function Navbar() {
  const [isVisible, setIsVisible] = useState(false);
  return <nav className={"navbar navbar-light " + s.navbar}>
    <div className="container">
      <Link className="navbar-brand" href="/">
        <img src={logoUrl} alt="" height="24" className={s.logo} />
      </Link>
      <div className={s.hamburger} onClick={() => {
        setIsVisible(!isVisible);
      }}>
        <svg viewBox="0 0 100 80" width="40" height="40">
          <rect width="100" height="10" fill='#fff'></rect>
          <rect y="35" width="100" height="10" fill='#fff'></rect>
          <rect y="70" width="100" height="10" fill='#fff'></rect>
        </svg>
      </div>
      <div className={s.navbarContainer + ' ' + (isVisible ? s.containerVisible : '')}>
        <div className={s.navbarItem}>
          <Link href={'/'} className={s.navbarLink + ' text-uppercase'}>Home</Link>
        </div>
        <div className={s.navbarItem}>
          <Link href={'/blog'} className={s.navbarLink + ' text-uppercase'}>Blog</Link>
        </div>
        <div className={s.navbarItem}>
          <Link href={'/list'} className={s.navbarLink + ' text-uppercase'}>People</Link>
        </div>
        <div className={s.navbarItem}>
          <Link href={'/matches'} className={s.navbarLink + ' text-uppercase'}>Matches</Link>
        </div>
        <AuthenticationArea />
      </div>
    </div>
  </nav>
}