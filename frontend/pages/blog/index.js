import {blogs} from "./[title]";
import s from '../../styles/Blog.module.css';
import Head from "next/head";
import Link from "next/link";
export default function BlogIndex() {
  return <div className='container mt-4 min-vh-100'>
    <Head>
      <title>DiscoFriends Blog</title>
    </Head>
    <div className='row'>
      <div className='col-12 col-lg-10 mx-auto'>
        <h1 className='fw-bold text-uppercase'>DiscoFriends Blog</h1>
        <p>We release a variety of Blog posts every week. We cover everything from making friends on Discord, to tips on Discord friend making and DiscoFriends, and more.</p>
        <div className='row'>
          {
            Object.getOwnPropertyNames(blogs).reverse().map(v => {
              return <div className='col-12 col-md-6 mb-4' key={v}>
                <Link href={'/blog/' + v} className={s.blogLink}>
                  <div className={s.blogCard}>
                    <h2 className={s.blogTitle}>{blogs[v].title}</h2>
                    <p className={'fst-italic mb-0'}>Posted {blogs[v].created}</p>
                  </div>
                </Link>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}