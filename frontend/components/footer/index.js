import s from './footer.module.css';
import Link from "next/link";
export default function Footer(props) {
  return <footer className={s.footer}>
    <div className='container'>
      <div className='row'>
        <div className='col-12 col-lg-6 mx-auto'>
          <p className='text-light mb-4 text-center small'>By accessing this website, you agree to our <Link className={s.link} href={'/terms-of-service'}>Terms of Service</Link> and <Link className={s.link} href='/privacy-policy'>Privacy Policy</Link>.</p>
        </div>
      </div>
      <div className='row'>
        <div className='col-12 col-lg-6 mx-auto'>
          <p className='text-light mb-0 small text-center'>Copyright {new Date().getFullYear()} DiscoFriends - <Link className={s.link} href={'/contact'}>Contact</Link> <Link className={s.link} target='_blank' href={'https://github.com/masquernya/Disco'}>Source Code</Link>
          </p>
        </div>
      </div>
    </div>
  </footer>
}