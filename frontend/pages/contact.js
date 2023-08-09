import Head from "next/head";
import config from 'next/config';

export default function Contact() {
  return <div className='container min-vh-100'>
    <Head>
      <title>Contact - DiscoFriends</title>
    </Head>
    <div className='row mt-4'>
      <div className='col-12'>
        <h3 className='fw-bold text-uppercase'>Contact</h3>
        <p>To contact the operator of DiscoFriends, you can send an email to: <span className='fw-bold'>{config().publicRuntimeConfig.contactEmail}</span></p>
      </div>
    </div>
  </div>
}