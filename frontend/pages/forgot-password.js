import Head from 'next/head';
import HomeComponent from '../components/home';
import ResetPassword from "../components/resetPassword";

export default function Home() {
  return <div className='container min-vh-100'>
    <Head>
      <title>Reset Password</title>
    </Head>

    <div className='row'>
      <div className='col-12 col-lg-6 mx-auto'>
        <ResetPassword />
      </div>
    </div>
  </div>
}