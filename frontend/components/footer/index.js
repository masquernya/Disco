import s from './footer.module.css';
import Link from "next/link";
export default function Footer(props) {
  return <footer className={s.footer}>

    <div className='container'>
      <div className='row'>
        <div className='col-12 col-lg-6 mx-auto'>
          <p className='text-light mb-4 text-center small'>By accessing discofriends.net, you agree to our <Link className={s.link} href={'/terms-of-service'}>Terms of Service</Link> and <Link className={s.link} href='/privacy-policy'>Privacy Policy</Link>.</p>
        </div>
      </div>
      <div className='row'>
        <div className='col-12 mx-auto'>
          <p className='text-center'>
            <Link className={s.link + ' ' + s.linkBlog} href={'/blog/how-do-i-make-friends'}>How do I make Discord Friends?</Link>
            <Link href={`/blog/why-making-discord-friends-is-difficult`} className={s.link + ' ' + s.linkBlog}>Why Making Discord Friends Is Difficult</Link>
            <Link href={`/blog/tips-for-making-friends-on-discord`} className={s.link + ' ' + s.linkBlog}>Tips for Making Friends on Discord and DiscoFriends</Link>
          </p>
        </div>
      </div>
      <div className='row'>
        <div className='col-12 col-lg-6 mx-auto'>
          <p className='text-light mb-0 small'>Copyright {new Date().getFullYear()} DiscoFriends - <Link className={s.link} href={'/contact'}>Contact</Link>
          </p>
        </div>
      </div>
    </div>
  </footer>
}