import Head from "next/head";

export default function Blog(props) {
  return <div className='container min-vh-100 mt-4'>
    <Head>
      <title>{props.title + ' - DiscoFriends'}</title>
    </Head>
    <div className='row'>
      <div className='col-12 col-lg-6 mx-auto'>
        <h1 className='fw-bold'>{props.title}</h1>
        <p>Updated: {props.created}</p>
        {props.children}
      </div>
    </div>
  </div>
}