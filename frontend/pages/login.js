import Head from "next/head";
import LoginComponent from '../components/login';

export default function Login() {
  return <div className='container min-vh-100'>
    <Head>
      <title>Login - DiscoFriends</title>
    </Head>
    <div className='row'>
      <div className='col-12 col-lg-6 mx-auto'>
        <LoginComponent />
      </div>
    </div>
  </div>
}